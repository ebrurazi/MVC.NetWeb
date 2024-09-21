using Microsoft.EntityFrameworkCore;
using SolutionSharingApp.Data;
using SolutionSharingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolutionSharingApp.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<List<Admin>> GetAsync()
        {
            return await _context.Admins.ToListAsync();
        }

        public async Task<Admin?> GetAsync(string email, string password)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
            Console.WriteLine($"Admin fetched: {admin != null}");
            return admin;
        }


        
        public async Task CreateAdminAsync(Admin admin)
        {
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
        }

        public async Task<Admin?> GetByCategoryAsync(string category)
        {
            return await _context.Admins.FirstOrDefaultAsync(x => x.Subject == category);
        }
       
        public async Task<Admin?> GetByEmailAsync(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(x => x.Email == email);
        }
    }
}
