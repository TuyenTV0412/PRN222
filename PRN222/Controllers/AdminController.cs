using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;


namespace PRN222.Controllers
{
    public class AdminController : Controller
    {

        private readonly Prn222Context _prn222Context;

        public AdminController(Prn222Context prn222Context)
        {
            _prn222Context = prn222Context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAllUser()
        {
            List<User> users;
            users = await _prn222Context.Users
                                               .Include(u => u.Role) 
                                               .Where(u => u.RoleId != 3) 
                                               .ToListAsync();

            return View("~/Views/Admin/ListUser.cshtml", users);
        }


        [HttpPost]
        public async Task<IActionResult> GetAllUser(string txt)
        {
            List<User> users;

            if (!string.IsNullOrEmpty(txt))
            {
                // Tìm kiếm người dùng theo tên
                users = await _prn222Context.Users
                                            .Include(u => u.Role)
                                            .Where(u => u.Username.Contains(txt) && u.RoleId != 3)
                                            .ToListAsync();
            }
            else
            {
                // Lấy tất cả người dùng nếu không có từ khóa tìm kiếm
                users = await _prn222Context.Users
                                            .Include(u => u.Role)
                                            .Where(u => u.RoleId != 3)
                                            .ToListAsync();
            }
            ViewBag.SearchTerm = txt;

            return View("~/Views/Admin/ListUser.cshtml", users);
        }

        public async Task<IActionResult> UserDetail(int id)
        {
            var user = await _prn222Context.Users
                                            .Include(u => u.Role)
                                            .FirstOrDefaultAsync(u => u.PersonId == id);

            if (user == null)
            {
                return NotFound(); // Nếu không tìm thấy người dùng, trả về lỗi 404
            }

            var roles = await _prn222Context.Roles
                .Where(u => u.RoleId != 3).ToListAsync();
            ViewBag.Roles = roles;

            return View("~/Views/User/UserDetail.cshtml", user);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(User updatedUser)
        {
            var user = await _prn222Context.Users.FirstOrDefaultAsync(u => u.PersonId == updatedUser.PersonId);
            if (user == null)
            {
                return NotFound();
            }

            user.RoleId = updatedUser.RoleId;

            await _prn222Context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật vai trò thành công!";
            return RedirectToAction("GetAllUser", "Admin");
        }





    }
}
