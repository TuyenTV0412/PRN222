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
            return View("~/Views/Cart/loginCart.cshtml");
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

            return View("~/Views/Cart/CreateCard.cshtml", foundUser); // Chuyển đến View hiển thị thông tin chi tiết của người dùng
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



    }

}
