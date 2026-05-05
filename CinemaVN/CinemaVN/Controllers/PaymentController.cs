using CinemaVN.DatModels;
using CinemaVN.Models; // Đảm bảo namespace models của bạn đúng
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CinemaVN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly CinemaVNContext db = new CinemaVNContext();

        private readonly string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private readonly string vnp_Returnurl = "https://localhost:7293/Payment/VnpayReturn";
        private readonly string vnp_TmnCode = "WC6JR9O3";
        private readonly string vnp_HashSecret = "SZMKHEL6PM97IPBOMU5H45CE11ED20E6";
        private readonly EmailService _emailService;

        public PaymentController(EmailService emailService)
        {
            _emailService = emailService;
        }
        private string GetIpAddress()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ip) || ip == "::1")
            {
                return "127.0.0.1"; 
            }
            return ip;
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }

        // ================== 1. TẠO THANH TOÁN (GỬI ĐI VNPAY) ==================
        public IActionResult ThanhToanVnpay(int mahd)
        {
            var hd = db.HoaDons.FirstOrDefault(x => x.MaHd == mahd);
            if (hd == null) return NotFound("Không tìm thấy hóa đơn cần thanh toán.");

            var vnpay = new SortedList<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnp_TmnCode },
                { "vnp_Amount", ((int)((hd.TongTien ?? 0) * 100)).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", GetIpAddress() },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", "ThanhToanHoaDon_" + mahd },
                { "vnp_OrderType", "billpayment" },
                { "vnp_ReturnUrl", vnp_Returnurl },
                { "vnp_TxnRef", mahd.ToString() },
                { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss") }
            };

            var queryBuilder = new StringBuilder();
            foreach (var kvp in vnpay)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    queryBuilder.Append(Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value) + "&");
                }
            }
            string rawQuery = queryBuilder.ToString().TrimEnd('&');

            string secureHash = HmacSHA512(vnp_HashSecret, rawQuery);

            string paymentUrl = vnp_Url + "?" + rawQuery + "&vnp_SecureHash=" + secureHash;
            return Redirect(paymentUrl);
        }

        // ================== 2. NHẬN KẾT QUẢ TỪ VNPAY ==================
        public IActionResult VnpayReturn()
        {
            var queryCollection = Request.Query;
            string vnp_SecureHash = queryCollection["vnp_SecureHash"].ToString();

            var sortedData = new SortedList<string, string>();
            foreach (var key in queryCollection.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_")
                    && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                {
                    sortedData.Add(key, queryCollection[key].ToString());
                }
            }

            var hashBuilder = new StringBuilder();
            foreach (var kvp in sortedData)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    hashBuilder.Append(Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value) + "&");
                }
            }

            string rawHashData = hashBuilder.ToString().TrimEnd('&');
            string checkHash = HmacSHA512(vnp_HashSecret, rawHashData);

            if (!vnp_SecureHash.Equals(checkHash, StringComparison.OrdinalIgnoreCase))
            {
                return Content($"LỖI CHỮ KÝ!\n\nChuỗi băm tạo ra: {rawHashData}\n\nHash Sinh ra: {checkHash}\nHash VNPAY: {vnp_SecureHash}");
            }


            int mahd = int.Parse(queryCollection["vnp_TxnRef"].ToString());
            string responseCode = queryCollection["vnp_ResponseCode"].ToString();

            var hd = db.HoaDons.FirstOrDefault(x => x.MaHd == mahd);
            if (hd == null) return Content("Không tìm thấy hóa đơn");

            HttpContext.Session.Remove("SessionKey");

            if (responseCode == "00")
            {
                hd.TrangThai = 1;

                var ves = db.Ves.Where(v => v.MaHd == mahd).ToList();
                foreach (var v in ves)
                {
                    v.TrangThai = 1;
                }

                var chiTietDichVus = db.ChiTietDichVus.Where(ct => ct.MaHd == hd.MaHd).ToList();

                if (chiTietDichVus.Any())
                {
                    foreach (var ct in chiTietDichVus)
                    {
                        Kho? kho = db.Khos.FirstOrDefault(k => k.MaDv == ct.MaDv && k.MaCn == hd.MaCn);

                        if (kho != null)
                        {
                            kho.SoLuongTon -= ct.SoLuong;
                            if (kho.SoLuongTon < 0)
                            {
                                kho.SoLuongTon = 0;
                            }
                        }
                    }
                }

                var nd = db.NguoiDungs
                    .FirstOrDefault(n => n.MaNd == HttpContext.Session.GetInt32("UserId"));

                if (nd != null)
                {
                    if (hd.MaKm != null)
                    {
                        var km = db.KhuyenMais.FirstOrDefault(x => x.MaKm == hd.MaKm);

                        if (km?.SoDiem != null && nd.DiemHoiVien >= km.SoDiem)
                        {
                            nd.DiemHoiVien -= km.SoDiem.Value;
                        }
                    }

                    int diemCong = (int)Math.Floor((hd.TongTien ?? 0) / 1000);

                    nd.DiemHoiVien = (nd.DiemHoiVien ?? 0) + diemCong;
                }

                db.SaveChanges();

                var user = db.NguoiDungs.FirstOrDefault(u => u.MaNd == hd.MaNd);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    // Bỏ qua await bằng cách gán cho _, giúp gửi mail chạy ngầm, không làm chậm chuyển trang
                    _ = _emailService.SendEmailQRTicket(user.Email, hd.MaHd);
                }

                return RedirectToAction("ThanhCong");
            }
            else
            {
                var ves = db.Ves.Where(v => v.MaHd == mahd).ToList();
                var dv = db.ChiTietDichVus.Where(v => v.MaHd == mahd).ToList();
                db.Ves.RemoveRange(ves);
                db.ChiTietDichVus.RemoveRange(dv);
                db.HoaDons.Remove(hd);
                db.SaveChanges();
                return RedirectToAction("ThatBai");
            }
        }

        public IActionResult ThanhCong()
        {
            return View();
        }

        public IActionResult ThatBai()
        {
            return View();
        }
    }
}