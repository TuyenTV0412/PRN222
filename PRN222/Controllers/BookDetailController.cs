using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PRN222.DTO;

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
        public async Task<IActionResult> Index(int id)
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

            // Lấy CategoryId từ sách
            var categoryId = book.CategoryId;

            // Lấy 4 sách ngẫu nhiên cùng category
            var randomBooks = await _prn222Context.Books
                                                   .Where(b => b.CategoryId == categoryId && b.BookId != id)  // Lọc sách theo category và loại bỏ sách hiện tại
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


        [HttpGet]
        public async Task<IActionResult> UpdateBookInfo(int bookID)
        {
            var book = await _prn222Context.Books
                                           .Include(b => b.Author)
                                           .Include(b => b.Category)
                                           .Include(b => b.Publisher)
                                           .FirstOrDefaultAsync(b => b.BookId == bookID);

            if (book == null)
            {
                return NotFound();
            }

            var categories = await _prn222Context.Categories.ToListAsync();
            var authors = await _prn222Context.Authors.ToListAsync();
            var publishers = await _prn222Context.Publishers.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Author = authors;
            ViewBag.Publisher = publishers;

            // Chỉ định đường dẫn cụ thể đến view
            return View("~/Views/Book/UpdateBookInfo.cshtml", book);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookInfo(Book updatedBook)
        {
            var book = await _prn222Context.Books.FirstOrDefaultAsync(b => b.BookId == updatedBook.BookId);
            if (book == null)
            {
                return NotFound(); // Nếu không tìm thấy sách, trả về lỗi 404
            }

            // Cập nhật các thông tin khác của sách
            book.BookName = updatedBook.BookName;
            book.AuthorId = updatedBook.AuthorId;
            book.CategoryId = updatedBook.CategoryId;
            book.PublisherId = updatedBook.PublisherId;
            book.Description = updatedBook.Description;
            book.PublishingYear = updatedBook.PublishingYear;
            book.Quantity = updatedBook.Quantity;
            if(updatedBook.Images != null)
            {
                book.Images = updatedBook.Images;
            }
            else
            {
                book.Images = book.Images;
            }
           
            try
            {
                await _prn222Context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin sách thành công!";
                return RedirectToAction("ManageBooks", "Admin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể cập nhật sách. Lỗi: {ex.Message}";
                return RedirectToAction("ManageBooks", "Admin");
            }
        }

        public async Task<IActionResult> BorrowDetail(int id)
        {
            return View("~/Views/Borrow/BorrowDetail.cshtml");  // Đảm bảo đường dẫn chính xác
        }

        public async Task<IActionResult> Borrow(int bookID)
        {
            var book = await _prn222Context.Books
                                           .Include(b => b.Author)
                                           .FirstOrDefaultAsync(b => b.BookId == bookID);

            if (book == null)
            {
                return NotFound();
            }

            var sessionBooks = HttpContext.Session.GetString("BorrowBooks");
            List<BookDto> borrowBooks;

            if (string.IsNullOrEmpty(sessionBooks))
            {
                borrowBooks = new List<BookDto>();
            }
            else
            {
                borrowBooks = JsonConvert.DeserializeObject<List<BookDto>>(sessionBooks);
            }

            if (!borrowBooks.Any(b => b.BookId == book.BookId))
            {
                borrowBooks.Add(new BookDto
                {
                    BookId = book.BookId,
                    BookName = book.BookName,
                    AuthorName = book.Author?.AuthorName ?? "Không xác định",
                    Images = book.Images,
                    Quantity = 1
                });
            }

            HttpContext.Session.SetString("BorrowBooks", JsonConvert.SerializeObject(borrowBooks));

            TempData["SuccessMessage"] = "Đã thêm sách vào danh sách mượn!";

            return RedirectToAction("BorrowDetail", "BookDetail");
        }

        public IActionResult RemoveBookFromBorrow(int bookID)
        {
            // Lấy danh sách sách từ session
            var sessionBooks = HttpContext.Session.GetString("BorrowBooks");

            if (!string.IsNullOrEmpty(sessionBooks))
            {
                // Giải tuần tự hóa chuỗi JSON thành danh sách sách
                var borrowBooks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PRN222.DTO.BookDto>>(sessionBooks);

                // Tìm quyển sách cần xóa
                var bookToRemove = borrowBooks.FirstOrDefault(b => b.BookId == bookID);

                if (bookToRemove != null)
                {
                    // Xóa quyển sách khỏi danh sách
                    borrowBooks.Remove(bookToRemove);

                    // Lưu lại danh sách vào session
                    HttpContext.Session.SetString("BorrowBooks", Newtonsoft.Json.JsonConvert.SerializeObject(borrowBooks));
                }
            }

            TempData["SuccessMessage"] = "Đã xóa sách khỏi danh sách mượn!";
            return RedirectToAction("BorrowDetail", "BookDetail");
        }




        [HttpGet]
        public async Task<IActionResult> GetInfoToCheckOut()
        {
            // Lấy danh sách sách từ session
            var sessionBooks = HttpContext.Session.GetString("BorrowBooks");
            if (string.IsNullOrEmpty(sessionBooks))
            {
                TempData["ErrorMessage"] = "Không có sách nào trong danh sách mượn.";
                return RedirectToAction("BorrowDetail");
            }

            // Lấy thông tin người dùng từ session hoặc database
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login");
            }

            // Tìm kiếm người dùng trong cơ sở dữ liệu
            var foundUser = await _prn222Context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (foundUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng với tên đăng nhập này.";
                return RedirectToAction("Login");
            }

            // Kiểm tra xem người dùng đã có thẻ hay chưa
            var existingCard = await _prn222Context.Cards.FirstOrDefaultAsync(c => c.PersonId == foundUser.PersonId);
            if (existingCard == null)
            {
                TempData["ErrorMessage"] = "Người dùng này chưa có thẻ mượn sách.";
                return RedirectToAction("BorrowDetail", "BookDetail");
            }

            // Lấy danh sách sách từ session
            var borrowBooks = JsonConvert.DeserializeObject<List<BookDto>>(sessionBooks);

            // Tìm kiếm mã phiếu mượn cuối cùng và cộng thêm 1
            var lastBorrowId = await _prn222Context.Borrows.OrderByDescending(b => b.BorrowId).FirstOrDefaultAsync();
            int borrowId = lastBorrowId != null ? lastBorrowId.BorrowId + 1 : 1;

           
            return View("~/Views/Borrow/Checkout.cshtml", new CheckoutModel
            {
                BorrowBooks = borrowBooks,
                CardId = existingCard.CardId,
                borrowId = borrowId,
                ValidFrom = DateOnly.FromDateTime(DateTime.Now).ToString("dd/MM/yyyy"),
                ValidThru = DateOnly.FromDateTime(DateTime.Now.AddDays(7)).ToString("dd/MM/yyyy")
            });

        }





    }
}
