using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolutionSharingApp.Data;
using SolutionSharingApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionSharingApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Toplam kullanıcı sayısını al
            var totalUsers = await _context.Users.CountAsync();
            
            // Toplam çözüm önerisi sayısını al
            var totalSolutions = await _context.Solutions.CountAsync();
            
            // Onay bekleyen çözümleri say
            var pendingSolutions = await _context.Solutions.CountAsync(s => s.Status == "Pending");

            // Son 10 aktiviteyi al
            var recentActivities = await _context.Solutions
                .OrderByDescending(s => s.DateCreated)
                .Take(10)
                .ToListAsync();

            // Aylık çözüm önerisi sayısını gruplandırarak al
            var solutionCountByMonth = await _context.Solutions
                .GroupBy(s => new { s.DateCreated.Year, s.DateCreated.Month })
                .Select(g => new { Date = g.Key.Year + "-" + g.Key.Month, Count = g.Count() })
                .ToDictionaryAsync(k => k.Date, v => v.Count);

            // Aylık kullanıcı aktivitesini gruplandırarak al
            var userActivityByMonth = await _context.Users
                .GroupBy(u => new { u.DateCreated.Year, u.DateCreated.Month })
                .Select(g => new { Date = g.Key.Year + "-" + g.Key.Month, Count = g.Count() })
                .ToDictionaryAsync(k => k.Date, v => v.Count);

            var viewModel = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalSolutions = totalSolutions,
                PendingSolutions = pendingSolutions,
                RecentActivities = recentActivities,
                SolutionCountByMonth = solutionCountByMonth,
                UserActivityByMonth = userActivityByMonth
            };

            return View(viewModel);
        }
    }
}
