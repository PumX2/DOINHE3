using DOINHE.Db;
using DOINHE.Entitys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DOINHE.Pages
{
    public class SignupModel : PageModel
    {
        private readonly MyDbContext _context;

        public SignupModel(MyDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User User { get; set; }

        private static string generatedCode;

        public IActionResult OnGet()
        {
            // Check if user is already logged in
            string? customer = HttpContext.Session.GetString("customer");
            string? admin = HttpContext.Session.GetString("admin");
            if (customer != null || admin != null)
            {
                return Redirect("/Index");
            }

            ViewData["title"] = "Register";
            return Page();
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostSendVerificationCode([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new JsonResult(new { success = false, message = "Email không hợp lệ." });
            }

            try
            {
                generatedCode = new Random().Next(100000, 999999).ToString();  // Generate a 6-digit code

                // Send email
                var mail = new MailMessage();
                mail.To.Add(email);
                mail.Subject = "Your verification code";
                mail.Body = $"Your verification code is: {generatedCode}";
                mail.From = new MailAddress("pixelsthunhat283@gmail.com");

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("pixelsthunhat283@gmail.com", "ndmhung6");
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }

                return new JsonResult(new { success = true, message = "Mã xác nhận đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new JsonResult(new { success = false, message = "Không thể gửi mã xác nhận. Vui lòng thử lại sau." });
            }
        }





        public IActionResult OnPostVerifyCode([FromBody] string code)
        {
            if (code == generatedCode)
            {
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Save user to the database
            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index", new { success = "Đăng ký thành công!" });
        }
    }
}
