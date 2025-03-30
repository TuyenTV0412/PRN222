using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;

namespace PRN222.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly Prn222Context _context;

        public CheckoutController(Prn222Context context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CheckoutBooks(List<int> bookID, string validFrom, string validThru)
        {
            // Kiểm tra session người dùng
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mượn sách";
                return RedirectToAction("Login");
            }

            // Tìm thông tin người dùng
            var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (foundUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng";
                return RedirectToAction("Login");
            }

            // Kiểm tra số lượng sách trước khi tạo phiếu mượn
            var errorMessages = new List<string>();
            var booksToUpdate = new List<Book>();

            foreach (var bookId in bookID)
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    errorMessages.Add($"Không tìm thấy sách ID {bookId}");
                }
                else if (book.Quantity < 1)
                {
                    errorMessages.Add($"Sách '{book.BookName}' hiện đã mượn hết, vui lòng mượn lại sau!");
                }
                else
                {
                    booksToUpdate.Add(book);
                }
            }

            // Nếu có lỗi, trả về thông báo
            if (errorMessages.Any())
            {
                TempData["ErrorMessage"] = string.Join("<br>", errorMessages);
                return RedirectToAction("BorrowDetail", "BookDetail");
            }

            // Bắt đầu transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo phiếu mượn
                var newBorrow = new Borrow
                {
                    PersonId = foundUser.PersonId,
                    BorrowDate = DateOnly.Parse(validFrom),
                    Deadline = DateOnly.Parse(validThru)
                };

                await _context.Borrows.AddAsync(newBorrow);
                await _context.SaveChangesAsync();

                // Tạo chi tiết mượn và cập nhật số lượng
                foreach (var book in booksToUpdate)
                {
                    // Thêm chi tiết mượn
                    await _context.BorrowDetails.AddAsync(new BorrowDetail
                    {
                        BorrowId = newBorrow.BorrowId,
                        BookId = book.BookId,
                        Amount = 1,
                        StatusId = 1
                    });

                    // Giảm số lượng sách
                    book.Quantity -= 1;
                    _context.Books.Update(book);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove("BorrowBooks");
                TempData["ErrorMessage"] = "Mượn sách thành công!";
                return RedirectToAction("HistoryBorrow", "Borrow", new { id = foundUser.PersonId });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.InnerException?.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi xử lý: {ex.InnerException?.Message}";
                return RedirectToAction("BorrowDetail", "BookDetail");
            }
        }




    }
}
