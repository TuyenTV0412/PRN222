using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PRN222.Controllers
{
    public class LoginController : Controller
    {
        private readonly Prn222Context _prn222Context;

        public LoginController(Prn222Context prn222Context)
        {
            _prn222Context = prn222Context;
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
            // Kiểm tra nếu tên đăng nhập và mật khẩu hợp lệ
            var userRecord = await _prn222Context.Users
                .Include(u => u.Role)  // Bao gồm thông tin Role nếu cần
                .FirstOrDefaultAsync(u => u.Username == user && u.Password == pass);

            if (userRecord == null)
            {
                // Nếu không tìm thấy tài khoản hợp lệ
                ViewBag.Message = "Sai tên đăng nhập hoặc mật khẩu!";
                return View("~/Views/User/Login.cshtml");
            }

            // Lưu thông tin người dùng vào session hoặc cookie
            if (remember)
            {
                // Thiết lập cookie nếu "remember me" được chọn
                Response.Cookies.Append("cname", user, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
                Response.Cookies.Append("cpass", pass, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
                Response.Cookies.Append("crem", "1", new CookieOptions { Expires = DateTime.Now.AddDays(7) });
            }
            else
            {
                // Xóa cookies nếu không nhớ mật khẩu
                Response.Cookies.Delete("cname");
                Response.Cookies.Delete("cpass");
                Response.Cookies.Delete("crem");
            }

            // Lưu thông tin người dùng vào session
            HttpContext.Session.SetInt32("UserId", userRecord.PersonId);
            HttpContext.Session.SetString("Username", userRecord.Username);
            HttpContext.Session.SetInt32("RoleId", userRecord.RoleId);


            if(userRecord.RoleId == 3) {
                return View("~/Views/Admin/AdminHome.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            // Chuyển hướng đến trang chính
           
        }

        // Xử lý logout
        public IActionResult Logout()
        {
            // Xóa session khi logout
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("RoleId");

            // Xóa cookies
            Response.Cookies.Delete("cname");
            Response.Cookies.Delete("cpass");
            Response.Cookies.Delete("crem");

            return RedirectToAction("Index", "Home");
        }

    }
}
