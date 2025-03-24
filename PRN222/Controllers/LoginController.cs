using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;

namespace PRN222.Controllers
{
    public class LoginController : Controller
    {
        private readonly Prn222Context _prn222Context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LoginController(Prn222Context prn222Context, IPasswordHasher<User> passwordHasher)
        {
            _prn222Context = prn222Context;
            _passwordHasher = passwordHasher;
        }

        // Trang đăng nhập (GET)
        public IActionResult Index()
        {
            return View("~/Views/User/Login.cshtml");
        }

        // Xử lý đăng nhập (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string user, string pass, bool remember)
        {
            var userRecord = await _prn222Context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == user);

            if (userRecord == null)
            {
                ViewBag.Message = "Sai tên đăng nhập hoặc mật khẩu!";
                return View("~/Views/User/Login.cshtml");
            }

            // Xác minh mật khẩu đã hash
            var result = _passwordHasher.VerifyHashedPassword(userRecord, userRecord.Password, pass);
            if (result != PasswordVerificationResult.Success)
            {
                ViewBag.Message = "Sai tên đăng nhập hoặc mật khẩu!";
                return View("~/Views/User/Login.cshtml");
            }

            // Nếu "Remember me" được chọn, lưu vào cookie
            if (remember)
            {
                Response.Cookies.Append("cname", user, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
                Response.Cookies.Append("cpass", pass, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
                Response.Cookies.Append("crem", "1", new CookieOptions { Expires = DateTime.Now.AddDays(7) });
            }
            else
            {
                Response.Cookies.Delete("cname");
                Response.Cookies.Delete("cpass");
                Response.Cookies.Delete("crem");
            }

            // Lưu thông tin đăng nhập vào session
            HttpContext.Session.SetInt32("UserId", userRecord.PersonId);
            HttpContext.Session.SetString("Username", userRecord.Username);
            HttpContext.Session.SetInt32("RoleId", userRecord.RoleId);


            if (userRecord.RoleId == 3)
            {
                return View("~/Views/Admin/AdminHome.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Xử lý logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Clear();

            Response.Cookies.Delete("cname");
            Response.Cookies.Delete("cpass");
            Response.Cookies.Delete("crem");

            return RedirectToAction("Index", "Home");
        }
    }
}
