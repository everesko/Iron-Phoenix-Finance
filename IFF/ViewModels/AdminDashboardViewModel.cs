using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using IFF.Models;
using IFF.Services;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.IO;
using IFF.Views;
using CommunityToolkit.Maui.Storage;

namespace IFF.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private ObservableCollection<UserWithRoleChange> _users;
        private ObservableCollection<string> _roles;
        private ObservableCollection<Complaint> _complaints;
        private ObservableCollection<Salary> _salaries;
        private Dictionary<int, User> _originalUsers = new Dictionary<int, User>();
        private ObservableCollection<int> _availableYears;
        private int _selectedYear;

        public ObservableCollection<UserWithRoleChange> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Roles
        {
            get => _roles;
            set { _roles = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> AvailableYears
        {
            get => _availableYears;
            set { _availableYears = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Salary> Salaries
        {
            get => _salaries;
            set { _salaries = value; OnPropertyChanged(); }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); LoadSalaryReportAsync(); }
        }

        public ObservableCollection<Complaint> Complaints
        {
            get => _complaints;
            set { _complaints = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand DeleteComplaintCommand { get; }

        public AdminDashboardViewModel(User currentUser)
        {
            _databaseService = new DatabaseService();
            _currentUser = currentUser;
            Users = new ObservableCollection<UserWithRoleChange>();
            Roles = new ObservableCollection<string> { nameof(UserRole.Стрілець), nameof(UserRole.Фінансист), nameof(UserRole.Адміністратор) };
            Complaints = new ObservableCollection<Complaint>();
            Salaries = new ObservableCollection<Salary>();

            int currentYear = DateTime.Now.Year;
            AvailableYears = new ObservableCollection<int>(Enumerable.Range(currentYear - 5, 7));
            SelectedYear = currentYear;

            SaveCommand = new Command<UserWithRoleChange>(async (user) => await SaveUserAsync(user));
            DeleteCommand = new Command<UserWithRoleChange>(async (user) => await DeleteUserAsync(user));
            CancelCommand = new Command<UserWithRoleChange>(CancelChanges);
            LogoutCommand = new Command(async () => await LogoutAsync());
            GenerateReportCommand = new Command(async () => await GenerateReportAsync());
            DeleteComplaintCommand = new Command<Complaint>(async (complaint) => await DeleteComplaintAsync(complaint));
        }

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
            await LoadSalaryReportAsync();
            await LoadComplaintsAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                await _databaseService.InitializeAsync();
                Users.Clear();
                _originalUsers.Clear();
                var users = await _databaseService.GetUsersAsync();
                if (users == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося завантажити користувачів: список порожній.", "OK");
                    return;
                }
                foreach (var user in users.OrderBy(u => u.FullName))
                {
                    var userCopy = new User
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Password = user.Password,
                        Role = user.Role,
                        FullName = user.FullName,
                        HireDate = user.HireDate,
                        IsApproved = user.IsApproved,
                        HasAccess = user.HasAccess
                    };
                    Users.Add(new UserWithRoleChange(userCopy));
                    _originalUsers[user.Id] = userCopy;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося завантажити користувачів: {ex.Message}", "OK");
            }
        }

        private async Task SaveUserAsync(UserWithRoleChange user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.Role) && user.HireDate.HasValue)
                    user.OriginalUser.IsApproved = true;
                user.OriginalUser.HasAccess = user.HasAccess; // Явно оновлюємо HasAccess
                await _databaseService.UpdateUserAsync(user.OriginalUser);
                _originalUsers[user.Id] = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    Role = user.Role,
                    FullName = user.FullName,
                    HireDate = user.HireDate,
                    IsApproved = user.IsApproved,
                    HasAccess = user.HasAccess
                };
                user.IsChanged = false;
                await Application.Current.MainPage.DisplayAlert("Успіх", "Дані збережено", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося зберегти користувача: {ex.Message}", "OK");
            }
        }

        private async Task DeleteUserAsync(UserWithRoleChange user)
        {
            var confirm = await Application.Current.MainPage.DisplayAlert("Підтвердження", "Ви впевнені, що бажаєте видалити цей обліковий запис?", "Так", "Ні");
            if (!confirm) return;

            try
            {
                await _databaseService.DeleteUserAsync(user.OriginalUser);
                Users.Remove(user);
                _originalUsers.Remove(user.Id);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося видалити користувача: {ex.Message}", "OK");
            }
        }

        private void CancelChanges(UserWithRoleChange user)
        {
            var original = _originalUsers[user.Id];
            user.FullName = original.FullName;
            user.Role = original.Role;
            user.HireDate = original.HireDate;
            user.HasAccess = original.HasAccess;
            user.IsChanged = false;
        }

        private async Task LogoutAsync()
        {
            App.CurrentUser = null;
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        private async Task LoadSalaryReportAsync()
        {
            try
            {
                // Очищаємо колекцію перед завантаженням
                Salaries.Clear();

                // Отримуємо існуючі зарплати з бази для поточного користувача та року
                var existingSalaries = await _databaseService.GetSalariesByUserAndYearAsync(_currentUser.Id, SelectedYear);

                // Завантажуємо по одному запису для кожного місяця (1–12)
                for (int month = 1; month <= 12; month++)
                {
                    // Перевіряємо, чи вже є запис для цього місяця в колекції
                    if (Salaries.Any(s => s.Month == month))
                    {
                        continue; // Пропускаємо, якщо місяць уже доданий
                    }

                    // Беремо існуючий запис із бази або створюємо новий
                    var salary = existingSalaries.FirstOrDefault(s => s.Month == month);
                    if (salary == null)
                    {
                        salary = new Salary
                        {
                            UserId = _currentUser.Id,
                            Year = SelectedYear,
                            Month = month,
                            AccrualDate = new DateTime(SelectedYear, month, 1),
                            Accrued = 0m,
                            Pdfo = 0m,
                            Pension = 0m,
                            Military = 0m,
                            ZfFund = 0m,
                            Bonus = 0m,
                            Received = 0m
                        };
                    }
                    Salaries.Add(salary);
                }

                Console.WriteLine($"Loaded {Salaries.Count} unique salaries for CurrentUser in year {SelectedYear}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadSalaryReportAsync Error: {ex}");
                throw;
            }
        }

        private async Task GenerateReportAsync()
        {
            var folder = await CommunityToolkit.Maui.Storage.FolderPicker.Default.PickAsync();
            if (folder == null || !folder.IsSuccessful) return;

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);

            gfx.DrawString($"Звіт по зарплаті {_currentUser.FullName} за {SelectedYear}", font, XBrushes.Black, new XRect(0, 0, page.Width, 20), XStringFormats.Center);
            int y = 40;
            gfx.DrawString("Місяць | Дата | Нараховано | ПДФО | ПЗ | ВЗ | ФЗФ | Премія | Отримано", font, XBrushes.Black, new XRect(20, y, page.Width, 20), XStringFormats.TopLeft);
            y += 20;
            foreach (var salary in Salaries)
            {
                var line = $"{GetMonthName(salary.Month)} | {salary.AccrualDate:dd.MM.yyyy} | {salary.Accrued:F2} | {salary.Pdfo:F2} | {salary.Pension:F2} | {salary.Military:F2} | {salary.ZfFund:F2} | {salary.Bonus:F2} | {salary.Received:F2}";
                gfx.DrawString(line, font, XBrushes.Black, new XRect(20, y, page.Width, 20), XStringFormats.TopLeft);
                y += 20;
            }

            string filePath = Path.Combine(folder.Folder.Path, $"Report_{_currentUser.Username}_{SelectedYear}.pdf");
            document.Save(filePath);
            document.Close();

            await Application.Current.MainPage.DisplayAlert("Звіт", $"Звіт збережено: {filePath}", "OK");
        }

        private async Task LoadComplaintsAsync()
        {
            Complaints.Clear();
            var complaints = await _databaseService.GetComplaintsAsync();
            foreach (var complaint in complaints.OrderByDescending(c => c.Date))
            {
                Complaints.Add(complaint);
            }
        }

        private async Task DeleteComplaintAsync(Complaint complaint)
        {
            var confirm = await Application.Current.MainPage.DisplayAlert("Підтвердження", "Видалити скаргу?", "Так", "Ні");
            if (confirm)
            {
                await _databaseService.DeleteComplaintAsync(complaint);
                Complaints.Remove(complaint);
            }
        }

        private string GetMonthName(int month) => new[] { "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень" }[month - 1];
    }

    public class UserWithRoleChange : BaseViewModel
    {
        private User _user;
        private bool _isChanged;

        public UserWithRoleChange(User user) => _user = user;

        public User OriginalUser => _user;

        public int Id => _user.Id;

        public string Username => _user.Username;

        public string Password => _user.Password;

        public string FullName
        {
            get => _user.FullName;
            set { _user.FullName = value; OnPropertyChanged(); _isChanged = true; }
        }

        public string Role
        {
            get => _user.Role;
            set { _user.Role = value; OnPropertyChanged(); _isChanged = true; }
        }

        public DateTime? HireDate
        {
            get => _user.HireDate;
            set { _user.HireDate = value; OnPropertyChanged(); _isChanged = true; }
        }

        public bool IsApproved
        {
            get => _user.IsApproved;
            set { _user.IsApproved = value; OnPropertyChanged(); }
        }

        public bool HasAccess
        {
            get => _user.HasAccess;
            set { _user.HasAccess = value; OnPropertyChanged(); _isChanged = true; }
        }

        public bool IsChanged
        {
            get => _isChanged;
            set { _isChanged = value; OnPropertyChanged(); }
        }
    }
}