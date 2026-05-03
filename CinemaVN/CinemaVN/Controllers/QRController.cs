using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace CinemaVN.Controllers
{
    public class QRController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GenerateQR(int mahd)
        {
            // Nội dung chứa trong mã QR (Có thể là ID hóa đơn hoặc link dẫn đến trang chi tiết)
            string qrText = $"MaHD:{mahd}|CinemaVN";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            // Tạo ảnh QR dạng Byte Array
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20); // 20 là kích thước pixel/module

            // Trả về file ảnh PNG trực tiếp cho trình duyệt
            return File(qrCodeImage, "image/png");
        }

    }
}
