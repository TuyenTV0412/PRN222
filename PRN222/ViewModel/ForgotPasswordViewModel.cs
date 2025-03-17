namespace PRN222.ViewModel
{
    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
    }

    public class VerifyOtpViewModel
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
