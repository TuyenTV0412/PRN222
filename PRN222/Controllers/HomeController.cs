using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PRN222.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Prn222Context _prn222Context;

        public HomeController(ILogger<HomeController> logger, Prn222Context prn222Context)
        {
            _logger = logger;
            _prn222Context = prn222Context;
        }

        // Hiển thị trang chủ với danh sách sách và danh mục
        public async Task<IActionResult> Index(int? categoryId, string query, int page = 1)
        {
            var categories = await _prn222Context.Categories.ToListAsync(); // Lấy danh sách category

            List<Book> books;

            // Kiểm tra nếu có query (tìm kiếm theo tên sách)
            if (!string.IsNullOrEmpty(query))
            {
                books = await _prn222Context.Books
                                     .Include(b => b.Author)
                                     .Include(b => b.Category)
                                     .Include(b => b.Publisher)
                                     .Where(b => b.BookName.Contains(query)) // Tìm theo tên sách
                                     .ToListAsync();
            }
            // Kiểm tra nếu có categoryId (tìm kiếm theo danh mục)
            else if (categoryId.HasValue)
            {
                books = await _prn222Context.Books
                                     .Include(b => b.Author)
                                     .Include(b => b.Category)
                                     .Include(b => b.Publisher)
                                     .Where(b => b.CategoryId == categoryId) // Lọc theo danh mục
                                     .ToListAsync();
            }
            // Nếu không có query và không có categoryId, lấy tất cả sách
            else
            {
                books = await _prn222Context.Books
                                     .Include(b => b.Author)
                                     .Include(b => b.Category)
                                     .Include(b => b.Publisher)
                                     .ToListAsync();
            }

            // Lấy 4 sách mới nhất
            var newestBooks = await _prn222Context.Books
                                                  .Include(b => b.Author)
                                                  .Include(b => b.Category)
                                                  .Include(b => b.Publisher)
                                                  .OrderByDescending(b => b.BookId)
                                                  .Take(4)
                                                  .ToListAsync();
            var author = await _prn222Context.Authors
                                           .OrderByDescending(b => b.AuthorId) // Sắp xếp theo AuthorID từ mới nhất
                                           .Take(4) // Lấy 4 tác giả mới nhất
                                           .ToListAsync();

            var totalBooks = books.Count();
            var pageSize = 8; // Số sách trên mỗi trang
            var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
            var booksToShow = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.Author = author; // Truyền dữ liệu tác giả vào ViewBag
            ViewBag.Categories = categories;
            ViewBag.NewestBooks = newestBooks;
            ViewBag.Query = query; // Truyền từ khóa tìm kiếm vào ViewBag

            return View(booksToShow); // Trả về danh sách sách tìm được
        }


        // Lọc sách theo danh mục
        public async Task<IActionResult> BooksByCategory(int categoryId)
        {
            var books = await _prn222Context.Books
                                            .Include(b => b.Author)
                                            .Include(b => b.Category)
                                            .Include(b => b.Publisher)
                                            .Where(b => b.CategoryId == categoryId)
                                            .ToListAsync();

            var category = await _prn222Context.Categories.FindAsync(categoryId);
            ViewBag.SelectedCategory = category?.CategoryName;
            ViewBag.Categories = await _prn222Context.Categories.ToListAsync(); // Load lại danh mục để hiển thị bên sidebar
            return View("Index", books); // Render lại trang Index nhưng với dữ liệu lọc theo Category
        }

        public async Task<IActionResult> Author(int page = 1)
        {
            var authors = await _prn222Context.Authors.ToListAsync();
            var totalAuthors = authors.Count();
            var pageSize = 8; // Số tác giả trên mỗi trang
            var totalPages = (int)Math.Ceiling((double)totalAuthors / pageSize);
            var authorsToShow = authors.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.Authors = authors; // Để hiển thị danh sách tác giả trong sidebar

            return View("~/Views/Home/Author.cshtml", authorsToShow);
        }



    }
}
