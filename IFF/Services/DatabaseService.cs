using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IFF.Models;

namespace IFF.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;

        public DatabaseService()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IFF");
            Directory.CreateDirectory(folderPath); // Створюємо папку, якщо її немає
            string dbPath = Path.Combine(folderPath, "users.db3");
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<Salary>();
                await _database.CreateTableAsync<Complaint>();

                // Тестові профілі
                var testUsers = new[]
                {
                    new User { Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin"), FullName = "Адмін Тест", Role = nameof(UserRole.Адміністратор), HireDate = DateTime.Now, IsApproved = true, HasAccess = true },
                    new User { Username = "financer", Password = BCrypt.Net.BCrypt.HashPassword("financer"), FullName = "Фінансист Тест", Role = nameof(UserRole.Фінансист), HireDate = DateTime.Now, IsApproved = true, HasAccess = true },
                    new User { Username = "worker", Password = BCrypt.Net.BCrypt.HashPassword("worker"), FullName = "Стрілець Тест", Role = nameof(UserRole.Стрілець), HireDate = DateTime.Now, IsApproved = true, HasAccess = true }
                };

                foreach (var testUser in testUsers)
                {
                    var existingUser = await _database.Table<User>().Where(u => u.Username == testUser.Username).FirstOrDefaultAsync();
                    if (existingUser == null)
                        await _database.InsertAsync(testUser);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка SQLite при ініціалізації: {ex.Message}", ex);
            }
        }

        // Отримання користувача за логіном і паролем (з перевіркою хешу)
        public async Task<User> GetUserAsync(string username, string password)
        {
            await InitializeAsync(); // Переконаємося, що база ініціалізована
            var user = await _database.Table<User>()
                                     .Where(u => u.Username == username)
                                     .FirstOrDefaultAsync();
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                return user;
            return null;
        }

        // Отримання всіх користувачів
        public async Task<List<User>> GetUsersAsync()
        {
            await InitializeAsync();
            return await _database.Table<User>().ToListAsync();
        }

        // Отримання користувача за логіном
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            await InitializeAsync();
            return await _database.Table<User>().Where(u => u.Username == username).FirstOrDefaultAsync();
        }

        // Збереження користувача (нова версія для сумісності)
        public async Task<int> SaveUserAsync(User user)
        {
            await InitializeAsync();
            if (user.Id == 0)
                return await _database.InsertAsync(user);
            return await _database.UpdateAsync(user);
        }

        // Оновлення користувача
        public async Task UpdateUserAsync(User user)
        {
            await InitializeAsync();
            await _database.UpdateAsync(user);
        }

        // Видалення користувача
        public async Task DeleteUserAsync(User user)
        {
            await InitializeAsync();
            await _database.DeleteAsync(user);
        }

        // Отримання зарплат за користувачем і роком
        public async Task<List<Salary>> GetSalariesByUserAndYearAsync(int userId, int year)
        {
            await InitializeAsync();
            return await _database.Table<Salary>()
                                 .Where(s => s.UserId == userId && s.Year == year)
                                 .ToListAsync();
        }

        // Збереження зарплати
        public async Task<int> SaveSalaryAsync(Salary salary)
        {
            await InitializeAsync();
            if (salary.Id == 0)
                return await _database.InsertAsync(salary);
            return await _database.UpdateAsync(salary);
        }

        // Збереження скарги
        public async Task<int> SaveComplaintAsync(Complaint complaint)
        {
            await InitializeAsync();
            if (complaint.Id == 0)
                return await _database.InsertAsync(complaint);
            return await _database.UpdateAsync(complaint);
        }

        // Отримання всіх скарг
        public async Task<List<Complaint>> GetComplaintsAsync()
        {
            await InitializeAsync();
            return await _database.Table<Complaint>().ToListAsync();
        }

        // Видалення скарги
        public async Task<int> DeleteComplaintAsync(Complaint complaint)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(complaint);
        }
    }
}