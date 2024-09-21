using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SolutionSharingApp.Data;
using SolutionSharingApp.Models;
using SolutionSharingApp.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SolutionSharingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(UserService userService, EmailService emailService, ApplicationDbContext context, IConfiguration configuration)
        {
            _userService = userService;
            _emailService = emailService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (!IsPasswordStrong(user.Password))
                {
                    TempData["ErrorMessage"] = "Şifre en az 8 karakter uzunluğunda, harf, rakam ve özel karakter içermelidir.";
                    return View(user);
                }

                await _userService.CreateUserAsync(user);
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                // Doğrulama kodu gönder
                string verificationCode = _userService.GenerateVerificationCode();
                user.VerificationCode = verificationCode;
                user.VerificationCodeExpiry = DateTime.Now.AddMinutes(10); // 10 dakika geçerli
                _context.Update(user);
                await _context.SaveChangesAsync();

                await _userService.SendVerificationCodeAsync(user);

                return RedirectToAction("VerifyCode", new { email = user.Email, isPasswordReset = false });
            }
            else
            {
                TempData["ErrorMessage"] = "Email veya şifre hatalı.";
                return RedirectToAction("Login", "Home");
            }
        }

        [HttpGet]
        public IActionResult VerifyCode(string email, bool isPasswordReset = false)
        {
            ViewBag.Email = email;
            ViewBag.IsPasswordReset = isPasswordReset;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyCode(string email, string verificationCode, bool isPasswordReset)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user != null && user.VerificationCode == verificationCode && user.VerificationCodeExpiry > DateTime.Now)
            {
                if (isPasswordReset)
                {
                    return RedirectToAction("ResetPassword", new { email });
                }
                else
                {
                    HttpContext.Session.SetString("User", user.FirstName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["ErrorMessage"] = "Geçersiz doğrulama kodu.";
            return RedirectToAction("VerifyCode", new { email, isPasswordReset });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Şifreler uyuşmuyor.";
                return View(new { email });
            }

            if (!IsPasswordStrong(newPassword))
            {
                TempData["ErrorMessage"] = "Şifre en az 8 karakter uzunluğunda, harf, rakam ve özel karakter içermelidir.";
                return View(new { email });
            }

            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Böyle bir kullanıcı bulunmamaktadır.";
                return View(new { email });
            }

            user.Password = newPassword;
            await _userService.UpdateUserAsync(email, user);
            TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.";
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Lütfen e-mailinizi girin.";
                return View();
            }

            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Böyle bir kullanıcı bulunmamaktadır.";
                return View();
            }

            string verificationCode = _userService.GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(10); // 10 dakika geçerli
            _context.Update(user);
            await _context.SaveChangesAsync();

            await _userService.SendVerificationCodeAsync(user);

            TempData["SuccessMessage"] = "Doğrulama kodu gönderildi.";
            return RedirectToAction("VerifyCode", new { email, isPasswordReset = true });
        }

        private bool IsPasswordStrong(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsLetter)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new ArgumentNullException("Key"));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60")), // Default to 60 minutes if null
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
