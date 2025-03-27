using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;


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
                .ThenInclude(bd => bd.Book)  // Include Book to update quantity
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

            if (borrow == null)
            {
                return NotFound();
            }

            // Update the return date if provided
            if (returnDate.HasValue)
            {
                borrow.ReturnDate = returnDate.Value;
            }

            // Update status for all borrow details
            foreach (var detail in borrow.BorrowDetails)
            {
                // Check if status is changing to "Đã trả" (status ID 2)
                if (statusId == 2 && detail.StatusId != 2)
                {
                    // Increase book quantity by the borrowed amount
                    detail.Book.Quantity += detail.Amount;
                }

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
        public async Task<IActionResult> SendBorrowEmails([FromForm] List<int> personIds, DateTime? deadlineDate)
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
                .Where(b => personIds.Contains(b.PersonId) 
            && (deadlineDate == null || b.Deadline.Equals(DateOnly.FromDateTime(deadlineDate.Value)))) // Chỉ lấy dữ liệu của PersonId được search
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
                <img src='data:image/jpeg;base64,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' alt='Library Logo' style='width:150px;'>
            </div>");

                emailContent.AppendLine($"<h2 style='color:#2c3e50;'>📚 Thông báo sắp đến hạn trả sách</h2>");
               emailContent.AppendLine($"<h2 style='color:yellowgreen;'>Xin Chào {person.Name}</h2>");
                emailContent.AppendLine("<p>Chúng tôi gửi đến bạn thông tin mượn sách hiện tại và nhắc nhở về hạn trả sách. Vui lòng trả sách đúng hạn để tránh phí trễ.</p>");

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
            <p style='text-align:center;'>❤️ Cảm ơn bạn đã sử dụng dịch vụ của thư viện. Nếu cần hỗ trợ, vui lòng liên hệ qua thông tin bên dưới!</p>");

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
