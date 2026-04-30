using System.ComponentModel.DataAnnotations;

namespace CinemaVN.DatModels
{
    public class Register
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ!")]
        public string? Phone { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 ký tự trở lên!")]
        public string Password { get; set; } = null!;

        [Display(Name = "Xác nhận mật khẩu")]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu!")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp!")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
