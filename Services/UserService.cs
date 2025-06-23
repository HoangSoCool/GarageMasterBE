using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace GarageMasterBE.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        // Tạo hash mật khẩu (dùng SHA256 làm ví dụ đơn giản)
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        // Kiểm tra mật khẩu so với hash lưu trong DB
        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == passwordHash;
        }

        // Tìm user theo email
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        // Tạo user mới (chưa xác thực email)
        public async Task<User> CreateUserAsync(string email, string password, string role = "Customer")
        {
            var user = new User
            {
                Email = email,
                PasswordHash = HashPassword(password),
                EmailConfirmed = false,
                EmailConfirmationCode = GenerateConfirmationCode(),
                EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(1),
                Role = role // <-- thêm dòng này
            };

            await _users.InsertOneAsync(user);
            return user;
        }

        // Sinh mã xác nhận 6 chữ số
        private string GenerateConfirmationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // Xác thực email
        public async Task<bool> ConfirmEmailAsync(string email, string code)
        {
            var user = await GetByEmailAsync(email);
            if (user == null)
                return false;

            if (user.EmailConfirmed)
                return true; // Đã xác thực rồi

            if (user.EmailConfirmationCode == code && user.EmailConfirmationCodeExpiry > DateTime.UtcNow)
            {
                var update = Builders<User>.Update
                    .Set(u => u.EmailConfirmed, true)
                    .Unset(u => u.EmailConfirmationCode)
                    .Unset(u => u.EmailConfirmationCodeExpiry);

                await _users.UpdateOneAsync(u => u.Id == user.Id, update);
                return true;
            }

            return false;
        }

        // Kiểm tra đăng nhập, trả về user nếu đúng
        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return null;

            if (!user.EmailConfirmed) return null;

            if (VerifyPassword(password, user.PasswordHash))
                return user;

            return null;
        }

        // Đặt lại mật khẩu qua email
        public async Task<bool> ForgotPasswordAsync(string email, EmailService emailService)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return true; // Không tiết lộ

            // Sinh mật khẩu mới random
            var newPassword = GenerateRandomPassword(10); // 10 ký tự gồm chữ + số
            var newHash = HashPassword(newPassword);

            // Cập nhật mật khẩu mới
            var update = Builders<User>.Update.Set(u => u.PasswordHash, newHash);
            await _users.UpdateOneAsync(u => u.Id == user.Id, update);

            // Gửi email mật khẩu mới
            await emailService.SendNewPasswordEmailAsync(email, newPassword);

            return true;
        }

        // Hàm sinh mật khẩu random
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Đặt lại mật khẩu với mật khẩu hiện tại
        public async Task<bool> ResetPasswordWithCurrentAsync(string email, string currentPassword, string newPassword)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return false;
            if (!VerifyPassword(currentPassword, user.PasswordHash)) return false;

            var newHash = HashPassword(newPassword);
            var update = Builders<User>.Update.Set(u => u.PasswordHash, newHash);
            await _users.UpdateOneAsync(u => u.Id == user.Id, update);

            return true;
        }

        public async Task<bool> DeleteByIdAsync(string userId)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == userId);
            return result.DeletedCount > 0;
        }
    }
}
