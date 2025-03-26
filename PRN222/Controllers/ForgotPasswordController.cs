using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models; // Model của bảng Users
using PRN222.ViewModel;
using System.Net;
using System.Net.Mail;

namespace PRN222.Controllers
{
    [Route("forgot-password")]
    public class ForgotPasswordController : Controller
    {
        private readonly IConfiguration _config;
        private readonly Prn222Context _context; // DbContext

        public ForgotPasswordController(IConfiguration config, Prn222Context context)
        {
            _config = config;
            _context = context;
        }

        // Hiển thị form quên mật khẩu
        [HttpGet("")]
        public IActionResult ForgotPassword()
        {
            return View("~/Views/User/ForgotPassword.cshtml");
        }

        // Xử lý quên mật khẩu
        [HttpPost("")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null) // Nếu email không có trong DB
            {
                ViewBag.Error = "Email không tồn tại!";
                return View("~/Views/User/ForgotPassword.cshtml");
            }

            var otpCode = new Random().Next(100000, 999999).ToString();

            // Lưu vào session
            HttpContext.Session.SetString("ResetOTP", otpCode);
            HttpContext.Session.SetString("ResetEmail", model.Email);
            HttpContext.Session.SetString("ResetExpiry", DateTime.UtcNow.AddMinutes(10).ToString());

            await SendEmail(model.Email, "Mã OTP đặt lại mật khẩu",
                $"Mã OTP của bạn là: <b>{otpCode}</b>. Mã này sẽ hết hạn sau 10 phút.");

            return RedirectToAction("VerifyOtp");
        }

        // Gửi email trực tiếp từ Controller
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

        // Nhập mã OTP
        [HttpGet("verify-otp")]
        public IActionResult VerifyOtp()
        {
            return View("~/Views/User/VerifyOtp.cshtml");
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            var savedOtp = HttpContext.Session.GetString("ResetOTP");
            var savedEmail = HttpContext.Session.GetString("ResetEmail");
            var expiryTime = HttpContext.Session.GetString("ResetExpiry");

            if (savedOtp == null || savedEmail == null || expiryTime == null)
            {
                ViewBag.Error = "Mã xác minh không hợp lệ hoặc đã hết hạn.";
                return View("~/Views/User/VerifyOtp.cshtml");
            }

            if (DateTime.UtcNow > DateTime.Parse(expiryTime))
            {
                ViewBag.Error = "Mã xác minh đã hết hạn.";
                return View("~/Views/User/VerifyOtp.cshtml");
            }

            if (model.OtpCode != savedOtp || model.Email != savedEmail)
            {
                ViewBag.Error = "Mã xác minh không đúng.";
                return View("~/Views/User/VerifyOtp.cshtml");
            }

            HttpContext.Session.Remove("ResetOTP");
            HttpContext.Session.SetString("VerifiedEmail", model.Email);

            return RedirectToAction("ResetPassword");
        }

        // Hiển thị form đặt lại mật khẩu
        [HttpGet("reset-password")]
        public IActionResult ResetPassword()
        {
            return View("~/Views/User/ResetPassword.cshtml");
        }

        

[HttpPost("reset-password")]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        var verifiedEmail = HttpContext.Session.GetString("VerifiedEmail");
        if (verifiedEmail == null || verifiedEmail != model.Email)
        {
            ViewBag.Error = "Email chưa được xác minh.";
            return View("~/Views/User/ResetPassword.cshtml");
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user != null)
        {
            var passwordHasher = new PasswordHasher<User>();

            // Mã hóa mật khẩu trước khi lưu
            user.Password = passwordHasher.HashPassword(user, model.NewPassword);
            _context.SaveChanges();
        }

        HttpContext.Session.Remove("VerifiedEmail");

        ViewBag.Message = "Mật khẩu đã được đặt lại thành công!";
        return RedirectToAction("Index", "Login");
    }

}
}
