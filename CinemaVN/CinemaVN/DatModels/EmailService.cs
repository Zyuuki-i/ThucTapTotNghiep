using QRCoder;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Threading.Tasks;

namespace CinemaVN.DatModels
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendOTP(string toEmail, string otp)
        {
            var email = _config["EmailSettings:From"];
            var password = _config["EmailSettings:Pass"];

            var smtp = new SmtpClient
            {
                Host = _config["EmailSettings:Host"],
                Port = int.Parse(_config["EmailSettings:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(email, password)
            };

            var message = new MailMessage(email, toEmail)
            {
                Subject = "Mã Xác Thực " + _config["EmailSettings:FromName"],
                Body = $"Mã OTP của bạn là: {otp} có hiệu lực trong 5 phút, KHÔNG chia sẻ với bất kỳ ai để đảm bảo thông tin bảo mật!",
                IsBodyHtml = false
            };

            smtp.Send(message);
        }

        // 1. Đổi thành public để PaymentController gọi được
        public async Task SendEmailQRTicket(string toEmail, int mahd)
        {
            try
            {
                // 1. Tạo ảnh QR
                string qrText = $"MaHD:{mahd}|CinemaVN";
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                // 2. Lấy cấu hình Email từ appsettings.json (Giống hàm SendOTP)
                var fromEmail = _config["EmailSettings:From"];
                var password = _config["EmailSettings:Pass"];
                var host = _config["EmailSettings:Host"];
                var port = int.Parse(_config["EmailSettings:Port"]);
                var fromName = _config["EmailSettings:FromName"];

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, fromName);
                mail.To.Add(toEmail);
                mail.Subject = $"[{fromName}] Xác nhận đặt vé thành công - Mã Hóa Đơn #{mahd}";
                mail.IsBodyHtml = true;

                // 3. Nhúng ảnh QR vào HTML body
                string htmlBody = $@"
                    <h2>Cảm ơn bạn đã đặt vé tại {fromName}!</h2>
                    <p>Mã hóa đơn của bạn là: <strong>#{mahd}</strong></p>
                    <p>Vui lòng đưa mã QR bên dưới cho nhân viên soát vé khi đến rạp:</p>
                    <img src='cid:QrCodeImage' style='width:250px; height:250px;' />
                    <br/><p>Hẹn gặp lại bạn tại rạp!</p>";

                // Tạo AlternateView để chứa HTML và ảnh
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                // Đính kèm ảnh từ Byte Array và đặt ContentId là "QrCodeImage"
                LinkedResource inlineQr = new LinkedResource(new MemoryStream(qrCodeBytes), "image/png");
                inlineQr.ContentId = "QrCodeImage";
                avHtml.LinkedResources.Add(inlineQr);

                mail.AlternateViews.Add(avHtml);

                // 4. Gửi Mail bằng SMTP lấy từ config
                using (SmtpClient smtp = new SmtpClient(host, port))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, password);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi gửi mail nếu cần
                Console.WriteLine("Lỗi gửi mail: " + ex.Message);
            }
        }
    }
}