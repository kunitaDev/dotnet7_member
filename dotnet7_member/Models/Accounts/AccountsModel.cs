using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace dotnet7_member.Models.Accounts
{
    public class AccountsModel
    {
        public class RegisterRequest
        {

            [Display(Name = "ชื่อ")]
            [Required(ErrorMessage = "กรุณากรอกชื่อจริง")]
            public string Firstname { get; set; }

            [Display(Name = "นามสกุล")]
            [Required(ErrorMessage = "กรุณากรอกนามสกุล")]
            public string Lastname { get; set; }

            [Display(Name = "อีเมล")]
            [Required(ErrorMessage = "กรุณากรอกอีเมล")]
            [EmailAddress(ErrorMessage = "กรุณากรอกอีเมลให้ถูกต้อง")]
            public string Email { get; set; }

            [Display(Name = "รหัสผ่าน")]
            [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
            [MinLength(8, ErrorMessage = "รหัสผ่านอย่าน้อย 8 ตัว")]
            public string Password { get; set; }

            [Display(Name = "ยืนยันรหัสผ่าน")]
            [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
            [MinLength(8, ErrorMessage = "รหัสผ่านอย่าน้อย 8 ตัว")]
            [Compare("Password", ErrorMessage = "กรุณายืนยันรหัสผ่านให้ถูกต้อง")]
            public string ConfirmPassword { get; set; }
        }

        public class LoginRequest
        {
            [Display(Name = "อีเมล")]
            [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้หรืออีเมล")]
            public string Email { get; set; }

            [Display(Name = "รหัสผ่าน")]
            [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
            [MinLength(8, ErrorMessage = "กรอกรหัสผ่านอย่างน้อย 8 ตัว")]
            public string Password { get; set; }
        }

        public class LoginResponse
        {
            public int UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
        }
    }
}
