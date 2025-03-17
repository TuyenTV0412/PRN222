using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using PRN222.Models; // Import model của bạn

namespace PRN222.Controllers
{
    public class GoogleAuthController : Controller
    {
        private readonly Prn222Context _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public GoogleAuthController(Prn222Context context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // Bắt đầu đăng nhập với Google
        public IActionResult Login()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Xử lý phản hồi từ Google
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin từ Google
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            string email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra xem người dùng đã tồn tại trong DB chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser == null)
            {
                // Nếu chưa có tài khoản, tạo mới
                var newUser = new User
                {
                    Username = email.Split('@')[0], // Lấy phần đầu của email làm username
                    Email = email,
                    Password = _passwordHasher.HashPassword(null, "GoogleUser123!"), // Mật khẩu giả (không sử dụng)
                    RoleId = 3 // Giả sử RoleId = 2 là user bình thường
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                existingUser = newUser; // Gán lại để lưu session
            }

            // Lưu thông tin đăng nhập vào session
            HttpContext.Session.SetInt32("UserId", existingUser.PersonId);
            HttpContext.Session.SetString("Username", existingUser.Username);
            HttpContext.Session.SetInt32("RoleId", existingUser.RoleId);

            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
