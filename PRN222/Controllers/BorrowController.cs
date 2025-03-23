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

        public async Task<IActionResult> ManageBorrow(DateTime? deadlineDate)
        {
            // Get all borrow details with related entities
            var borrowDetails = await _context.BorrowDetails
                .Include(bd => bd.Borrow)
                .Include(bd => bd.Book)
                .Include(bd => bd.Status)
                .Include(bd => bd.Status)
                .ToListAsync();

            // Filter by deadline date if provided
            if (deadlineDate.HasValue)
            {
                borrowDetails = borrowDetails
      .Where(bd => bd.Borrow.Deadline.Equals(DateOnly.FromDateTime(deadlineDate.Value)) && bd.StatusId==1)
      .ToList();

                ViewBag.Deadline = deadlineDate.Value;
            }

            // Group by PersonId
            var borrowMap = borrowDetails
                .GroupBy(bd => bd.Borrow.PersonId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.BorrowMap = borrowMap;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBorrowDetail(int borrowId, int bookId)
        {
            var borrowDetail = await _context.BorrowDetails
                .FirstOrDefaultAsync(bd => bd.BorrowId == borrowId && bd.BookId == bookId);

            if (borrowDetail != null)
            {
                _context.BorrowDetails.Remove(borrowDetail);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageBorrow");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBorrow(int borrowId)
        {
            var borrowDetails = await _context.BorrowDetails
                .Where(bd => bd.BorrowId == borrowId)
                .ToListAsync();

            if (borrowDetails.Any())
            {
                _context.BorrowDetails.RemoveRange(borrowDetails);

                var borrow = await _context.Borrows.FindAsync(borrowId);
                if (borrow != null)
                {
                    _context.Borrows.Remove(borrow);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageBorrow");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAllBorrows(int personId)
        {
            var borrows = await _context.Borrows
                .Where(b => b.PersonId == personId)
                .ToListAsync();

            foreach (var borrow in borrows)
            {
                var borrowDetails = await _context.BorrowDetails
                    .Where(bd => bd.BorrowId == borrow.BorrowId)
                    .ToListAsync();

                _context.BorrowDetails.RemoveRange(borrowDetails);
            }

            _context.Borrows.RemoveRange(borrows);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageBorrow");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateBorrow(int borrowId)
        {
            // Get the borrow record with related entities
            var borrow = await _context.Borrows
                .Include(b => b.BorrowDetails)
                .ThenInclude(bd => bd.Book)
                .Include(b => b.BorrowDetails)
                .ThenInclude(bd => bd.Status)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

            if (borrow == null)
            {
                return NotFound();
            }

            // Get all available statuses for the dropdown
            var statuses = await _context.Statuses.ToListAsync();
            ViewBag.Statuses = statuses;

            return View( borrow);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBorrow(int borrowId, DateOnly? returnDate, int statusId)
        {
            var borrow = await _context.Borrows
                .Include(b => b.BorrowDetails)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

            if (borrow == null)
            {
                return NotFound();
            }

            // Update the return date if provided
            if (returnDate.HasValue)
            {
                borrow.ReturnDate = returnDate;
            }

            // Update status for all borrow details
            foreach (var detail in borrow.BorrowDetails)
            {
                detail.StatusId = statusId;
            }

            try
            {
                // Mark the entity as modified
                _context.Update(borrow);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction("ManageBorrow");
            }
            catch (DbUpdateException ex)
            {
                // Log the error
                Console.WriteLine($"Error: {ex.InnerException?.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật.";
                return RedirectToAction("UpdateBorrow", new { borrowId });
            }
        }



    }
}
