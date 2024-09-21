using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolutionSharingApp.Data;
using SolutionSharingApp.Models;
using SolutionSharingApp.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SolutionSharingApp.Controllers
{
    public class SolutionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly Cloudinary _cloudinary;
        private readonly SolutionService _solutionService;
        private readonly NLPService _nlpService;
        private readonly UserService _userService;

        public SolutionsController(ApplicationDbContext context, NotificationService notificationService, Cloudinary cloudinary, SolutionService solutionService, NLPService nlpService, UserService userService)
        {
            _context = context;
            _notificationService = notificationService;
            _cloudinary = cloudinary;
            _solutionService = solutionService;
            _nlpService = nlpService;
            _userService = userService;
        }

        private void SetUserViewBag()
        {
            var user = HttpContext.Session.GetString("User");
            ViewBag.User = user;
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
                solution.CreatedAt = DateTime.Now; // Ensure CreatedAt is set correctly

                var photoUrls = new List<string>();

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

                solution.Photos = photoUrls;
                solution.PhotoDescriptions = PhotoDescriptions;
                _context.Solutions.Add(solution);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(solution);
        }

        private async Task<ImageUploadResult> UploadPhoto(IFormFile photo)
        {
            using (var stream = photo.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(photo.FileName, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult;
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SolutionDet(int id)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution == null)
            {
                return NotFound();
            }

            solution.ClickCount++;
            await _context.SaveChangesAsync();

            return View("/Views/Second/SolutionDet.cshtml", solution);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution != null)
            {
                solution.Status = "Approved";
                _context.Solutions.Update(solution);
                await _context.SaveChangesAsync();

                // Bildirim oluşturma
                if (!string.IsNullOrEmpty(solution.UserId))
                {
                    await _notificationService.CreateNotificationAsync(solution.UserId, $"Çözümünüz onaylandı: {solution.Title}");
                }
                else
                {
                    // userId null ise loglama veya hata mesajı
                    // Logger veya başka bir işlem yapabilirsiniz.
                    Console.WriteLine("UserId is null. Notification not created.");
                }

                return Json(new { success = true, message = "Başarıyla onaylandı", solution });
            }
            return Json(new { success = false, message = "Onaylama başarısız" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string rejectReason)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution != null)
            {
                solution.Status = "Rejected";
                solution.RejectReason = rejectReason;
                _context.Solutions.Update(solution);
                await _context.SaveChangesAsync();

                // Bildirim oluşturma
                if (!string.IsNullOrEmpty(solution.UserId))
                {
                    await _notificationService.CreateNotificationAsync(solution.UserId, $"Çözümünüz reddedildi: {rejectReason}");
                }
                else
                {
                    // userId null ise loglama veya hata mesajı
                    // Logger veya başka bir işlem yapabilirsiniz.
                    Console.WriteLine("UserId is null. Notification not created.");
                }

                return RedirectToAction("SolutionAdmin");
            }
            return Json(new { success = false, message = "Reddetme başarısız" });
        }

        [HttpPost]
        public async Task<IActionResult> Return(int id, string returnReason)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution != null)
            {
                solution.Status = "Returned";
                solution.ReturnReason = returnReason;
                _context.Solutions.Update(solution);
                await _context.SaveChangesAsync();

                // Bildirim oluşturma
                if (!string.IsNullOrEmpty(solution.UserId))
                {
                    await _notificationService.CreateNotificationAsync(solution.UserId, $"Çözümünüz geri gönderildi: {returnReason}");
                }
                else
                {
                    // userId null ise loglama veya hata mesajı
                    // Logger veya başka bir işlem yapabilirsiniz.
                    Console.WriteLine("UserId is null. Notification not created.");
                }

                return RedirectToAction("SolutionAdmin");
            }
            return Json(new { success = false, message = "Geri gönderme başarısız" });
        }

        [HttpGet]
        public async Task<IActionResult> SolutionAdmin()
        {
            var solutions = await _context.Solutions.ToListAsync();
            return View("/Views/Second/SolutionAdmin.cshtml", solutions);
        }

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

        [HttpGet]
        public async Task<IActionResult> GetSolutionDetails(int id)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution == null)
            {
                return NotFound();
            }

            return Json(new
            {
                title = solution.Title,
                author = solution.Author,
                category = solution.Category,
                problem = solution.Problem,
                solutionContent = solution.SolutionContent,
                photos = solution.Photos,
                photoDescriptions = solution.PhotoDescriptions
            });
        }
        
    }
}
