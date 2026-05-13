using System;
using System.Security.Cryptography;
using System.Text;

namespace CinemaVN.HauModels // Thay đổi namespace cho khớp với dự án của bạn
{
    public class MoMoSecurity
    {
        /// <summary>
        /// Hàm tạo chữ ký điện tử (Signature) theo chuẩn HMAC-SHA256 của MoMo
        /// </summary>
        /// <param name="rawHash">Chuỗi dữ liệu đã được nối theo thứ tự quy định</param>
        /// <param name="secretKey">Secret Key do MoMo cung cấp trong Sandbox</param>
        /// <returns>Chuỗi mã hóa dạng Hex</returns>
        public string CreateSignature(string rawHash, string secretKey)
        {
            // Chuyển chuỗi sang mảng byte
            byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(rawHash);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                // Thực hiện băm dữ liệu
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);

                // Chuyển đổi từ mảng byte sang chuỗi Hex viết thường (Lower case) theo yêu cầu MoMo
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }

        /* LƯU Ý 
           Thứ tự các trường khi nối chuỗi rawHash để tạo Signature:
           accessKey=...&amount=...&extraData=...&ipnUrl=...&orderId=...&orderInfo=...&partnerCode=...&requestId=...&returnUrl=...&requestType=captureWallet
        */
    }
}