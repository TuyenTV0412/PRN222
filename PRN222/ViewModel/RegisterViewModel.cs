using System.ComponentModel.DataAnnotations;
namespace PRN222.ViewModel { 
public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    public string Gender { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập ngày sinh.")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
    public string Password { get; set; }
}
    }
