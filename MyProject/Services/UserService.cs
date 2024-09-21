using Microsoft.EntityFrameworkCore;
using SolutionSharingApp.Data;
using SolutionSharingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace SolutionSharingApp.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public UserService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<List<User>> GetAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email and password cannot be null or empty");
            }

            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
        }

        public async Task CreateUserAsync(User user)
        {
            // Kullanıcıyı kaydet
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Doğrulama kodu oluştur ve gönder
            string verificationCode = GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(10); // 10 dakika geçerli

            // Veritabanına kaydet
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Email gönder
            await _emailService.SendEmailAsync(user.Email, "Doğrulama Kodu", $"Doğrulama kodunuz: {verificationCode}");
        }

        public string GenerateVerificationCode()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999); // Generates a number between 100000 and 999999
            return code.ToString();
        }

        public async Task<bool> ValidateVerificationCodeAsync(string email, string verificationCode)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null && user.VerificationCode == verificationCode && user.VerificationCodeExpiry > DateTime.Now)
            {
                return true;
            }
            return false;
        }

        public async Task SendVerificationCodeAsync(User user)
        {
            // Email gönder
            await _emailService.SendEmailAsync(user.Email, "Doğrulama Kodu", $"Doğrulama kodunuz: {user.VerificationCode}");
        }

        public async Task UpdateUserAsync(string email, User updatedUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null)
            {
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Password = updatedUser.Password;
                user.BirthDate = updatedUser.BirthDate;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
