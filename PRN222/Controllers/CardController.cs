using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;

namespace PRN222.Controllers
{
    public class CardController : Controller
    {
        private readonly Prn222Context _prn222Context;

        public CardController(Prn222Context prn222Context)
        {
            _prn222Context = prn222Context;
        }
        public IActionResult CreateCard()
        {
            return View("~/Views/Card/loginCart.cshtml");
        }


        public IActionResult checkCard()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCreateCard(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                TempData["ErrorMessage"] = "Tên đăng nhập không được để trống.";
                return RedirectToAction("CreateCard");
            }

            // Tìm kiếm người dùng trong cơ sở dữ liệu
            var foundUser = await _prn222Context.Users
                                                .FirstOrDefaultAsync(u => u.Username == user);

           
            if (foundUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng với tên đăng nhập này.";
                return RedirectToAction("CreateCard");
            }
            int personId = foundUser.PersonId;
            var card = await _prn222Context.Cards.FirstOrDefaultAsync(id => id.PersonId == personId);
            if(card == null)
            {
                TempData["ErrorMessage"] = "Tài Khoản chưa có thẻ mượn sách.";
            }
            else
            {
                ViewBag.Cart = card;
            }
            // Truyền thông tin người dùng vào ViewBag hoặc Model để hiển thị
            
            ViewBag.UserDetail = foundUser;

            return View("~/Views/Card/CreateCard.cshtml", foundUser); // Chuyển đến View hiển thị thông tin chi tiết của người dùng
        }


        [HttpPost]
        public async Task<IActionResult> CreateCard(int personId)
        {

            // Tìm kiếm người dùng trong cơ sở dữ liệu
            var foundUser = await _prn222Context.Users.FirstOrDefaultAsync(u => u.PersonId == personId);
            if (foundUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng với ID này.";
                return RedirectToAction("CreateCard");
            }

            // Kiểm tra xem người dùng đã có thẻ hay chưa
            var existingCard = await _prn222Context.Cards.FirstOrDefaultAsync(c => c.PersonId == personId);
            if (existingCard != null)
            {
                TempData["ErrorMessage"] = "Người dùng này đã có thẻ mượn sách.";
                return RedirectToAction("CreateCard");
            }

            try
            {
                // Tạo thẻ mới
                var newCard = new Card
                {
                    PersonId = personId,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Now), // Ngày bắt đầu từ hôm nay
                    ValidThru = DateOnly.FromDateTime(DateTime.Now.AddYears(4)) // Ngày hết hạn sau 4 năm
                };

                // Lưu thẻ mới vào cơ sở dữ liệu
                await _prn222Context.Cards.AddAsync(newCard);
                await _prn222Context.SaveChangesAsync();

                TempData["ErrorMessage"] = "Thẻ mượn sách đã được tạo thành công!";
                return RedirectToAction("CreateCard");
            }
            catch (DbUpdateException ex)
            {
                // Ghi log lỗi chi tiết từ inner exception
                Console.WriteLine($"Error: {ex.InnerException?.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo thẻ mượn sách.";
                return RedirectToAction("CreateCard");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CheckCard(string CardId)
        {
            if (string.IsNullOrEmpty(CardId))
            {
                TempData["ErrorMessage"] = "Thẻ Không Tồn Tại";
                return RedirectToAction("checkCard");
            }

            // Find the card in the database
            var card = await _prn222Context.Cards.FirstOrDefaultAsync(c => c.CardId.ToString() == CardId);

            if (card == null)
            {
                TempData["ErrorMessage"] = "Thẻ Không Tồn Tại";
                return RedirectToAction("checkCard");
            }

            // Find the user associated with the card
            var user = await _prn222Context.Users
                .FirstOrDefaultAsync(u => u.PersonId == card.PersonId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng";
                return RedirectToAction("checkCard");
            }

            // Get all available books
            var books = await _prn222Context.Books.ToListAsync();

            // Pass the user and books to the view
            ViewBag.User = user;
            ViewBag.Card = card;

            return View("~/Views/Borrow/CreateBorrow.cshtml", books);
        }


        [HttpPost]
        public async Task<IActionResult> CreateBorrow(int PersonId, List<int> bookid)
        {
            if (bookid == null || !bookid.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một cuốn sách";
                return RedirectToAction("CheckCard");
            }

            using (var transaction = await _prn222Context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Lấy danh sách sách trong 1 lần query
                    var books = await _prn222Context.Books
                        .Where(b => bookid.Contains(b.BookId))
                        .ToListAsync();

                    // Kiểm tra sách không tồn tại
                    var missingBookIds = bookid.Except(books.Select(b => b.BookId)).ToList();
                    if (missingBookIds.Any())
                    {
                        TempData["ErrorMessage"] = $"Không tìm thấy sách với ID: {string.Join(", ", missingBookIds)}";
                        return RedirectToAction("CheckCard");
                    }

                    // Kiểm tra số lượng sách
                    var outOfStockBooks = books.Where(b => b.Quantity < 1).ToList();
                    if (outOfStockBooks.Any())
                    {
                        var bookNames = outOfStockBooks.Select(b => b.BookName);
                        TempData["ErrorMessage"] = $"Sách đã hết: {string.Join(", ", bookNames)}";
                        return RedirectToAction("CheckCard");
                    }

                    // Tạo phiếu mượn
                    var newBorrow = new Borrow
                    {
                        PersonId = PersonId,
                        BorrowDate = DateOnly.FromDateTime(DateTime.Now),
                        Deadline = DateOnly.FromDateTime(DateTime.Now.AddDays(7))
                    };

                    await _prn222Context.Borrows.AddAsync(newBorrow);
                    await _prn222Context.SaveChangesAsync(); // Lấy BorrowId

                    // Tạo chi tiết mượn và cập nhật số lượng
                    foreach (var book in books)
                    {
                        // Thêm chi tiết mượn
                        await _prn222Context.BorrowDetails.AddAsync(new BorrowDetail
                        {
                            BorrowId = newBorrow.BorrowId,
                            BookId = book.BookId,
                            Amount = 1,
                            StatusId = 1
                        });

                        // Giảm số lượng sách
                        book.Quantity -= 1;
                        _prn222Context.Entry(book).State = EntityState.Modified;
                    }

                    await _prn222Context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Tạo phiếu mượn thành công!";
                    return RedirectToAction("ManageBorrow", "Borrow", new { personId = PersonId });
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Lỗi database: {ex.InnerException?.Message}");
                    TempData["ErrorMessage"] = "Lỗi hệ thống khi xử lý mượn sách";
                    return RedirectToAction("CheckCard");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Lỗi không xác định: {ex.Message}");
                    TempData["ErrorMessage"] = "Lỗi không mong đợi xảy ra";
                    return RedirectToAction("CheckCard");
                }
            }
        }



    }

}
