using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;
using System.Net.Mail;
using System.Net;
using System.Text;


namespace PRN222.Controllers
{
    public class BorrowController : Controller
    {
        private readonly IConfiguration _config;
        private readonly Prn222Context _context;

        public BorrowController(IConfiguration config,Prn222Context context)
        {
            _config = config;
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
            ViewBag.PersonIds = borrowMap.Keys.ToList();

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

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SendBorrowEmails([FromForm] List<int> personIds)
        {
            if (personIds == null || !personIds.Any())
            {
                TempData["ErrorMessage"] = "Không có PersonID nào hợp lệ để gửi email.";
                return RedirectToAction("ManageBorrow");
            }

            var borrowRecords = _context.Borrows
                .Include(b => b.Person)
                .Include(b => b.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .Where(b => personIds.Contains(b.PersonId)) // Chỉ lấy dữ liệu của PersonId được search
                .ToList();

            foreach (var personId in personIds)
            {
                var personBorrows = borrowRecords.Where(b => b.PersonId == personId).ToList();
                var person = personBorrows.FirstOrDefault()?.Person;

                if (person == null || string.IsNullOrEmpty(person.Email))
                {
                    continue; // Bỏ qua nếu không có email
                }

                var emailContent = new StringBuilder();

                // Thêm logo vào email
                emailContent.AppendLine($@"
            <div style='text-align:center; margin-bottom:20px;'>
                <img src='https://yourwebsite.com/logo.png' alt='Library Logo' style='width:150px;'>
            </div>");

                emailContent.AppendLine($"<h2 style='color:#2c3e50;'>📚 Thông tin mượn sách của {person.Name}</h2>");
                emailContent.AppendLine("<p>Xin chào, vui lòng kiểm tra thông tin mượn sách của bạn và trả sách trước hạn để tránh phí trễ hạn.</p>");

                foreach (var borrow in personBorrows)
                {
                    emailContent.AppendLine($"<h3 style='color:#16a085;'>Phiếu mượn ID: {borrow.BorrowId}</h3>");
                    emailContent.AppendLine($"<p><strong>📅 Ngày mượn:</strong> {borrow.BorrowDate:dd-MM-yyyy}</p>");
                    emailContent.AppendLine($"<p><strong>⏳ Hạn trả:</strong> {borrow.Deadline:dd-MM-yyyy}</p>");
                    emailContent.AppendLine($"<p><strong>🔄 Ngày trả:</strong> {(borrow.ReturnDate.HasValue ? borrow.ReturnDate.Value.ToString("dd-MM-yyyy") : "Chưa trả")}</p>");

                    // Bảng danh sách sách mượn
                    emailContent.AppendLine(@"
                <table style='width:100%; border-collapse: collapse; margin-top:10px;'>
                    <thead>
                        <tr style='background:#16a085; color:#fff;'>
                            <th style='padding:10px; border:1px solid #ddd;'>📖 Tên sách</th>
                            <th style='padding:10px; border:1px solid #ddd;'>📦 Số lượng</th>
                        </tr>
                    </thead>
                    <tbody>");

                    foreach (var detail in borrow.BorrowDetails)
                    {
                        emailContent.AppendLine($@"
                    <tr>
                        <td style='padding:10px; border:1px solid #ddd;'>{detail.Book.BookName}</td>
                        <td style='padding:10px; border:1px solid #ddd; text-align:center;'>{detail.Amount}</td>
                    </tr>");
                    }

                    emailContent.AppendLine("</tbody></table>");
                    emailContent.AppendLine("<hr>");
                }

                // Lời nhắc trả sách
                emailContent.AppendLine(@"
            <p style='color:red; font-weight:bold;'>⚠️ Lưu ý: Vui lòng trả sách trước hạn để tránh phí trễ hạn!</p>
            <p>📍 Địa chỉ thư viện: Thư Viện Sách, Hòa Lạc</p>
            <p>📞 Liên hệ: 0867699058</p>
            <p>📩 Email hỗ trợ: support@yourlibrary.com</p>
            <br>
            <p style='text-align:center;'>❤️ Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>");

                await SendEmail(person.Email, "📚 Nhắc nhở trả sách thư viện", emailContent.ToString());
            }

            TempData["SuccessMessage"] = "Email đã được gửi thành công.";
            return RedirectToAction("ManageBorrow");
        }



        private async Task SendEmail(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_config["EmailSettings:Port"]),
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:Username"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }


    }
}
