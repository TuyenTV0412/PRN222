using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;

namespace PRN222.Controllers
{
    public class BorrowController : Controller
    {
        private readonly Prn222Context _context;

        public BorrowController(Prn222Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> HistoryBorrow(int id)
        {
            // Lấy danh sách các phiếu mượn của người dùng
            var borrows = await _context.Borrows
     .Include(b => b.BorrowDetails)
     .ThenInclude(bd => bd.Book)
     .Include(b => b.BorrowDetails)
     .ThenInclude(bd => bd.Status)
     .Where(b => b.PersonId == id)
     .ToListAsync();


            var borrowDetails = borrows.SelectMany(b => b.BorrowDetails).ToList();

            return View(borrowDetails);
        }


    }
}
