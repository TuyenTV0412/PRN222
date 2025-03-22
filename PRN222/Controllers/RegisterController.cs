using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using PRN222.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222.Controllers
{
    public class RegisterController : Controller
    {
        private readonly Prn222Context _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegisterController(Prn222Context context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/User/Register.cshtml"); // Hiển thị form đăng ký
        }

        [HttpPost]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/User/Register.cshtml", model);
            }

            // Kiểm tra trùng lặp Username hoặc Email
            bool isExist = _context.Users.Any(u => u.Username == model.Username || u.Email == model.Email);
            if (isExist)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại.");
                return View("~/Views/User/Register.cshtml", model);
            }

            try
            {
                var user = new User
                {
                    Name = model.Name,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth != null ? DateOnly.FromDateTime(model.DateOfBirth) : default,
                    Address = model.Address,
                    Email = model.Email,
                    Phone = model.Phone,
                    Username = model.Username,
                    RoleId = 1,
                    StartDate = DateOnly.FromDateTime(DateTime.Now)

                };

                // Mã hóa mật khẩu
                user.Password = _passwordHasher.HashPassword(user, model.Password);

                // Thêm vào database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Index", "Login");      // Điều hướng về trang đăng nhập
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi khi lưu vào database: " + ex.InnerException?.Message ?? ex.Message);
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu: " + (ex.InnerException?.Message ?? ex.Message));
                return View("~/Views/User/Register.cshtml", model);
            }


        }
    }
}
