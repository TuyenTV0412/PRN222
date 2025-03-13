using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using Microsoft.EntityFrameworkCore;

namespace PRN222.Controllers
{
    public class BookDetailController : Controller
    {
        private readonly Prn222Context _prn222Context;

        public BookDetailController(Prn222Context prn222Context)
        {
            _prn222Context = prn222Context;
        }

        // Hiển thị chi tiết sách
        public async Task<IActionResult> Index(int id, int category)
        {
            // Lấy thông tin sách từ database theo BookID
            var book = await _prn222Context.Books
                                             .Include(b => b.Author)      // Bao gồm thông tin tác giả
                                             .Include(b => b.Category)    // Bao gồm thông tin danh mục
                                             .Include(b => b.Publisher)   // Bao gồm thông tin nhà xuất bản
                                             .FirstOrDefaultAsync(b => b.BookId == id);  // Lấy sách theo BookID

            if (book == null)
            {
                return NotFound();  // Nếu sách không tìm thấy, trả về lỗi 404
            }

            // Lấy 4 sách ngẫu nhiên cùng category
            var randomBooks = await _prn222Context.Books
                                                   .Where(b => b.CategoryId == category)  // Lọc sách theo category
                                                   .Include(b => b.Author)
                                                  .Include(b => b.Category)
                                                  .Include(b => b.Publisher)
                                                  .OrderByDescending(b => b.BookId)
                                                  .Take(4)
                                                  .ToListAsync();


            var categories = await _prn222Context.Categories.ToListAsync(); // Lấy danh sách category
            ViewBag.Categories = categories;
            // Truyền book và danh sách sách ngẫu nhiên sang View
            ViewBag.RandomBooks = randomBooks;

            return View("~/Views/Home/BookDetail.cshtml", book);  // Đảm bảo đường dẫn chính xác
        }

    }
}
