using System;
using System.Collections.Generic;

namespace SolutionSharingApp.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSolutions { get; set; }
        public int PendingSolutions { get; set; }
        public List<Solution> RecentActivities { get; set; } = new List<Solution>();
        public Dictionary<string, int> SolutionCountByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UserActivityByMonth { get; set; } = new Dictionary<string, int>();
    }
}
