using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolutionSharingApp.Data;
using SolutionSharingApp.Models;
using SolutionSharingApp.Services;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SecondController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminService _adminService;

        public SecondController(ApplicationDbContext context, AdminService adminService)
        {
            _context = context;
            _adminService = adminService;
        }

        private void SetUserViewBag()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.Admin = adminName;
        }

        // Kullanıcı yönetimi sayfası için
        public async Task<IActionResult> SolutionUser()
        {
            SetUserViewBag(); // ViewBag ayarlarını burada yapıyoruz

            var users = await _context.Users
                .Select(u => new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName ?? "Belirtilmemiş",
                    LastName = u.LastName ?? "Belirtilmemiş",
                    Email = u.Email ?? "Belirtilmemiş",
                    Role = u.Role ?? "Belirtilmemiş",
                    IsActive = u.IsActive
                }).ToListAsync();

            return View(users);
        }

        // Kullanıcı durumunu değiştirmek için
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id, bool isActive)
        {
            SetUserViewBag(); // ViewBag ayarlarını burada yapıyoruz

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = isActive;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
       public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }


        // Admin anasayfa
        public IActionResult IndexAd()
        {
            SetUserViewBag(); // ViewBag ayarlarını burada yapıyoruz
            return View();
        }

        // Dashboard sayfası
        public IActionResult Dashboard()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            var adminName = HttpContext.Session.GetString("AdminName");

            // Oturum değişkenlerini loglayın
            Console.WriteLine($"Dashboard: adminEmail: {adminEmail}, adminName: {adminName}");

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminName))
            {
                Console.WriteLine("Dashboard: Oturum değişkenleri null, AdminLogin sayfasına yönlendiriliyor.");
                return RedirectToAction("AdminLogin", "Home");
            }

            SetUserViewBag(); // ViewBag ayarlarını burada yapıyoruz

            // Dashboard için gerekli verileri burada hazırlayın
            var viewModel = new DashboardViewModel
            {
                // Verilerinizi burada doldurun
            };

            return View(viewModel);
        }

        // Çözüm yönetimi sayfası
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SolutionAdmin()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            var adminSubject = HttpContext.Session.GetString("AdminSubject");

            // Oturum değişkenlerini loglayın
            Console.WriteLine($"SolutionAdmin: adminEmail: {adminEmail}, adminSubject: {adminSubject}");

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminSubject))
            {
                Console.WriteLine("SolutionAdmin: Oturum değişkenleri null, AdminLogin sayfasına yönlendiriliyor.");
                return RedirectToAction("AdminLogin", "Home");
            }

            SetUserViewBag(); // ViewBag ayarlarını burada yapıyoruz

            var solutions = await _context.Solutions
                .Where(s => s.Category != null && s.Category.Contains(adminSubject))
                .ToListAsync();

            Console.WriteLine($"SolutionAdmin: {solutions.Count} solutions found.");

            return View(solutions);
        }
    }
}
