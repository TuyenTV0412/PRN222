using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.DTO;
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

        public IActionResult OtherSettings()
        {
            return View("~/Views/Admin/OtherSettings.cshtml");
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

        public async Task<IActionResult> ManageBooks(string txt, int page = 1)
        {
            var books = await _prn222Context.Books
                                            .Include(b => b.Author)
                                            .Include(b => b.Category)
                                            .Include(b => b.Publisher)
                                            .ToListAsync();

            if (!string.IsNullOrEmpty(txt))
            {
                books = books.Where(b => b.BookName.Contains(txt)).ToList();
            }

            // Phân trang
            var pageSize = 10;
            var totalBooks = books.Count();
            var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
            var booksToShow = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.SearchTerm = txt;

            return View("~/Views/Book/ListBook.cshtml", booksToShow);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBook(int bookID)
        {
            var book = await _prn222Context.Books.FirstOrDefaultAsync(b => b.BookId == bookID);
            if (book == null)
            {
                return NotFound(); // Nếu không tìm thấy sách, trả về lỗi 404
            }

            try
            {
                _prn222Context.Books.Remove(book);
                await _prn222Context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sách thành công!";
                return RedirectToAction("ManageBooks", "Admin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể xóa sách. Lỗi: {ex.Message}";
                return RedirectToAction("ManageBooks", "Admin");
            }
        }
        [HttpGet]
        public async Task<IActionResult> AddBook()
        {
            var categories = await _prn222Context.Categories.ToListAsync();
            var authors = await _prn222Context.Authors.ToListAsync();
            var publishers = await _prn222Context.Publishers.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Authors = authors;
            ViewBag.Publishers = publishers;

            return View("~/Views/Book/AddBook.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(Book newBook, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "Book", imageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                newBook.Images = imageFile.FileName;
            }

            try
            {
                await _prn222Context.Books.AddAsync(newBook);
                await _prn222Context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sách thành công!";
                return RedirectToAction("ManageBooks", "Admin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể thêm sách. Lỗi: {ex.Message}";
                return RedirectToAction("ManageBooks", "Admin");
            }
        }

        public async Task<IActionResult> Statistics()
        {
            var statistics = new StatisticsViewModel
            {
                TotalBooks = await _prn222Context.Books.CountAsync(),
                TotalAuthors = await _prn222Context.Authors.CountAsync(),
                TotalPublishers = await _prn222Context.Publishers.CountAsync(),
                BorrowedBooks = await _prn222Context.BorrowDetails
                    .Where(bd => bd.Status.StatusId == 1)
                    .CountAsync(),
                ReturnedBooks = await _prn222Context.BorrowDetails
                    .Where(bd => bd.Status.StatusId == 2)
                    .CountAsync(),
                OverdueBooks = await _prn222Context.BorrowDetails
                    .Where(bd => bd.Status.StatusId == 3)
                    .CountAsync(),
                TotalUsers = await _prn222Context.Users.CountAsync(),
                TotalBorrows = await _prn222Context.Borrows.CountAsync()
            };

            return View(statistics);
        }





    }
}
