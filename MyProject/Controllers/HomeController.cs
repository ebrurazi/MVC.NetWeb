using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolutionSharingApp.Models;
using SolutionSharingApp.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SolutionSharingApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace SolutionSharingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly AdminService _adminService;
        private readonly SolutionService _solutionService;
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(UserService userService, AdminService adminService, SolutionService solutionService, ApplicationDbContext context, Cloudinary cloudinary, IWebHostEnvironment hostingEnvironment)
        {
            _userService = userService;
            _adminService = adminService;
            _solutionService = solutionService;
            _context = context;
            _cloudinary = cloudinary;
            _hostingEnvironment = hostingEnvironment;
        }

        private void SetUserViewBag()
        {
            var user = HttpContext.Session.GetString("UserName"); // UserName olarak düzeltildi
            ViewBag.User = user;

            var admin = HttpContext.Session.GetString("AdminName");
            ViewBag.Admin = admin;
        }

        [HttpGet]
        public IActionResult AdminLogin()
        {
            SetUserViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(string email, string password)
        {
            var admin = await _adminService.GetAsync(email, password);
            if (admin != null)
            {
                var adminName = email.Split('@')[0];
                HttpContext.Session.SetString("AdminName", adminName);
                HttpContext.Session.SetString("AdminEmail", admin.Email);
                HttpContext.Session.SetString("AdminSubject", admin.Subject);

                var userClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, admin.Email),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(userClaims, "login");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);

                return RedirectToAction("Dashboard", "Second");
            }

            TempData["ErrorMessage"] = "Böyle bir kullanıcı bulunmamaktadır. Tekrar deneyiniz.";
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SolutionAdmin()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            var adminSubject = HttpContext.Session.GetString("AdminSubject");

            Console.WriteLine($"SolutionAdmin: adminEmail: {adminEmail}, adminSubject: {adminSubject}");

            if (adminEmail == null || adminSubject == null)
            {
                Console.WriteLine("SolutionAdmin: Oturum değişkenleri null, AdminLogin sayfasına yönlendiriliyor.");
                return RedirectToAction("AdminLogin", "Home");
            }

            var solutions = await _context.Solutions
                                        .Where(s => s.Category != null && s.Category.Contains(adminSubject))
                                        .ToListAsync();

            Console.WriteLine($"SolutionAdmin: {solutions.Count} solutions found.");

            return View(solutions);
        }

        public async Task<IActionResult> Index()
        {
            SetUserViewBag();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var notifications = _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();
                ViewBag.Notifications = notifications;
            }

            var topSolutions = await _context.Solutions
                .OrderByDescending(s => s.ClickCount)
                .Take(3)
                .ToListAsync();

            ViewBag.TopSolutions = topSolutions;

            return View();
        }

        public IActionResult MarkNotificationAsRead(int id)
        {
            SetUserViewBag();
            var notification = _context.Notifications.Find(id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Login()
        {
            SetUserViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User model)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var user = await _userService.GetByEmailAsync(model.Email);
                    if (user != null && user.Password == model.Password)
                    {
                        HttpContext.Session.SetString("UserName", user.FirstName ?? "Kullanıcı"); // UserName olarak düzeltildi
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email adresi boş olamaz.");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Profile()
        {
            SetUserViewBag();
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
                return RedirectToAction("Login");
            }
            var user = _userService.GetByEmailAsync(email).Result;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(User user)
        {
            if (ModelState.IsValid)
            {
                var email = HttpContext.Session.GetString("UserEmail");
                if (email == null)
                {
                    return RedirectToAction("Login");
                }
                await _userService.UpdateUserAsync(email, user);
                return RedirectToAction("Index");
            }
            SetUserViewBag();
            return View(user);
        }

        public IActionResult Splash()
        {
            SetUserViewBag();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SolutionDetails()
        {
            SetUserViewBag();
            var approvedSolutions = await _context.Solutions
                .Where(s => s.Status == "Approved")
                .ToListAsync();
            if (approvedSolutions == null || !approvedSolutions.Any())
            {
                return NotFound();
            }
            return View(approvedSolutions);
        }

        public IActionResult CreateSolution()
        {
            SetUserViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSolution(Solution solution, List<IFormFile> photos, List<string> PhotoDescriptions)
        {
            if (ModelState.IsValid)
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (userEmail == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı oturumu bulunamadı.");
                    return View(solution);
                }
                var user = await _userService.GetByEmailAsync(userEmail);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                    return View(solution);
                }

                solution.UserId = user.Id.ToString(); // Set the UserId

                var existingSolution = await _context.Solutions
                    .FirstOrDefaultAsync(s => s.Id == solution.Id);

                var photoUrls = existingSolution?.Photos ?? new List<string>();
                var photoDescriptions = existingSolution?.PhotoDescriptions ?? new List<string>();

                if (photos != null && photos.Count > 0)
                {
                    foreach (var photo in photos)
                    {
                        if (photo.Length > 0)
                        {
                            using (var stream = photo.OpenReadStream())
                            {
                                var uploadParams = new ImageUploadParams()
                                {
                                    File = new FileDescription(photo.FileName, stream)
                                };

                                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    photoUrls.Add(uploadResult.SecureUrl.AbsoluteUri);
                                }
                            }
                        }
                    }
                }

                if (PhotoDescriptions != null && PhotoDescriptions.Count > 0)
                {
                    photoDescriptions.AddRange(PhotoDescriptions);
                }

                solution.Photos = photoUrls;
                solution.PhotoDescriptions = photoDescriptions;

                // Anahtar kelimeleri çıkar ve çözüm modeline ekle
                solution.Keywords = string.Join(",", ExtractKeywords(solution.SolutionContent));

                if (existingSolution == null)
                {
                    _context.Solutions.Add(solution);
                }
                else
                {
                    existingSolution.Photos = solution.Photos;
                    existingSolution.PhotoDescriptions = solution.PhotoDescriptions;
                    existingSolution.Keywords = solution.Keywords;
                    _context.Update(existingSolution);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(solution);
        }

        public IActionResult Solutions()
        {
            SetUserViewBag();
            return View();
        }

        public async Task<IActionResult> Notifications()
        {
            SetUserViewBag();
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
                return RedirectToAction("Login");
            }
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            var notifications = await _context.Notifications
                                            .Where(n => n.UserId == user.Id.ToString())
                                            .OrderByDescending(n => n.CreatedAt)
                                            .ToListAsync();
            return View(notifications);
        }

        public async Task<IActionResult> MySolutions()
        {
            SetUserViewBag();
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
                Console.WriteLine("No UserEmail in session.");
                return RedirectToAction("Login");
            }
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine("No user found with email: " + email);
                return RedirectToAction("Login");
            }
            var solutions = await _context.Solutions
                                        .Where(s => s.UserId == user.Id.ToString())
                                        .ToListAsync();

            Console.WriteLine("Found " + solutions.Count + " solutions for user with ID: " + user.Id.ToString());
            return View(solutions);
        }

        public async Task<IActionResult> EditSolution(int id)
        {
            var solution = await _solutionService.GetSolutionByIdAsync(id);
            if (solution == null)
            {
                return NotFound();
            }
            return View(solution);
        }

        private string[] ExtractKeywords(string text)
        {
            // Anahtar kelimeleri ayıklamak için basit bir örnek (stop words hariç)
            var stopWords = new HashSet<string> { "bir", "ve", "ile", "bu", "için", "gibi", "çok", "da", "de", "ama", "veya" };
            var words = Regex.Split(text.ToLower(), @"\W+").Where(w => !stopWords.Contains(w) && w.Length > 2);
            return words.Distinct().ToArray();
        }

    }
}
