using System.Net;
using System.Net.Mail;

namespace CinemaVN.MyModels
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
                Body = $"Mã OTP của bạn là: {otp} có hiệu lực trong 5 phút, KHÔNG chia sẽ với bất kỳ ai để đảm bảo thông tin bảo mật!",
                IsBodyHtml = false
            };

            smtp.Send(message);
        }
    }
}
