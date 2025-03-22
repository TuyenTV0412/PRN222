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
            // Lấy thông tin người dùng từ session hoặc database
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login");
            }

            // Tìm kiếm người dùng trong cơ sở dữ liệu
            var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (foundUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng với tên đăng nhập này.";
                return RedirectToAction("Login");
            }

            // Tạo mới phiếu mượn
            var newBorrow = new Borrow
            {
                PersonId = foundUser.PersonId,
                BorrowDate = DateOnly.Parse(validFrom),
                Deadline = DateOnly.Parse(validThru)
            };

            try
            {
                // Lưu phiếu mượn vào cơ sở dữ liệu
                await _context.Borrows.AddAsync(newBorrow);
                await _context.SaveChangesAsync();

                // Lấy ID của phiếu mượn vừa tạo
                var borrowId = newBorrow.BorrowId;

                // Tạo chi tiết mượn cho từng sách
                foreach (var bookId in bookID)
                {
                    var borrowDetail = new BorrowDetail
                    {
                        BorrowId = borrowId,
                        BookId = bookId,
                        Amount =1,
                        StatusId =1
                    };

                    await _context.BorrowDetails.AddAsync(borrowDetail);
                    // Cập nhật số lượng sách trong bảng Books
                    var book = await _context.Books.FindAsync(bookId);
                    if (book != null && book.Quantity > 0)
                    {
                        book.Quantity -= 1;
                        _context.Books.Update(book);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã mượn sách thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException ex)
            {
                // Ghi log lỗi chi tiết từ inner exception
                Console.WriteLine($"Error: {ex.InnerException?.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi mượn sách.";
                return RedirectToAction("BorrowDetail", "BookDetail");
            }
        }

    }
}
