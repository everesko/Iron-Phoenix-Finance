using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class FinancierDashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private ObservableCollection<User> _users;
        private User _selectedUser;
        private ObservableCollection<Salary> _salariesForSelectedUser;
        private ObservableCollection<Salary> _salariesForCurrentUser;
        private ObservableCollection<int> _availableYears;
        private int _selectedYear;
        private decimal _totalReceived;
        private string _complaintText;
        private ObservableCollection<Complaint> _complaints;

        public ObservableCollection<User> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                _ = LoadSalariesAsync(); // Асинхронно завантажуємо зарплати для нового користувача
            }
        }

        public ObservableCollection<Salary> SalariesForSelectedUser
        {
            get => _salariesForSelectedUser;
            set { _salariesForSelectedUser = value; OnPropertyChanged(); CalculateTotal(); }
        }

        public ObservableCollection<Salary> SalariesForCurrentUser
        {
            get => _salariesForCurrentUser;
            set { _salariesForCurrentUser = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> AvailableYears
        {
            get => _availableYears;
            set { _availableYears = value; OnPropertyChanged(); }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
                _ = LoadSalariesAsync(); // Оновлюємо зарплати для обраного року
                _ = LoadSalaryReportAsync(); // Оновлюємо звіт для поточного користувача
            }
        }

        public decimal TotalReceived
        {
            get => _totalReceived;
            set { _totalReceived = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Complaint> Complaints
        {
            get => _complaints;
            set { _complaints = value; OnPropertyChanged(); }
        }

        public string ComplaintText
        {
            get => _complaintText;
            set { _complaintText = value; OnPropertyChanged(); }
        }

        public string FullName => _currentUser.FullName;

        public ICommand EditSalaryCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SubmitComplaintCommand { get; }

        public FinancierDashboardViewModel(User currentUser)
        {
            _databaseService = new DatabaseService();
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            Users = new ObservableCollection<User>();
            SalariesForSelectedUser = new ObservableCollection<Salary>();
            SalariesForCurrentUser = new ObservableCollection<Salary>();
            Complaints = new ObservableCollection<Complaint>();

            var hireDate = _currentUser.HireDate ?? DateTime.Now;
            int currentYear = DateTime.Now.Year;
            AvailableYears = new ObservableCollection<int>();
            for (int year = hireDate.Year; year <= currentYear + 1; year++)
                AvailableYears.Add(year);
            SelectedYear = currentYear;

            EditSalaryCommand = new Command<Salary>(async (salary) => await EditSalaryAsync(salary));
            GenerateReportCommand = new Command(async () => await GenerateReportAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            SubmitComplaintCommand = new Command(async () => await SubmitComplaintAsync());
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _databaseService.InitializeAsync();
                await LoadUsersAsync();
                await LoadSalaryReportAsync();
                await LoadSalariesAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося ініціалізувати дані фінансіста: {ex.Message}", "OK");
                Console.WriteLine($"InitializeAsync Error: {ex}");
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _databaseService.GetUsersAsync();
                Users.Clear();
                foreach (var user in users.OrderBy(u => u.FullName))
                    Users.Add(user);
                Console.WriteLine($"Loaded {Users.Count} users");
                SelectedUser = Users.FirstOrDefault(u => u.Id == _currentUser.Id) ?? Users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadUsersAsync Error: {ex}");
                throw;
            }
        }

        private async Task LoadSalariesAsync()
        {
            if (SelectedUser == null)
            {
                SalariesForSelectedUser.Clear();
                return;
            }

            try
            {
                // Очищаємо колекцію перед завантаженням
                SalariesForSelectedUser.Clear();
                var existingSalaries = await _databaseService.GetSalariesByUserAndYearAsync(SelectedUser.Id, SelectedYear);

                // Додаємо записи лише для 12 місяців
                for (int month = 1; month <= 12; month++)
                {
                    var salary = existingSalaries.FirstOrDefault(s => s.Month == month);
                    if (salary == null)
                    {
                        salary = new Salary
                        {
                            UserId = SelectedUser.Id,
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
                    SalariesForSelectedUser.Add(salary);
                }
                CalculateTotal();
                Console.WriteLine($"Loaded {SalariesForSelectedUser.Count} salaries for SelectedUser");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadSalariesAsync Error: {ex}");
                throw;
            }
        }

        private async Task LoadSalaryReportAsync()
        {
            try
            {
                // Очищаємо колекцію перед завантаженням
                SalariesForCurrentUser.Clear();

                // Отримуємо існуючі зарплати з бази для поточного користувача та року
                var existingSalaries = await _databaseService.GetSalariesByUserAndYearAsync(_currentUser.Id, SelectedYear);

                // Завантажуємо по одному запису для кожного місяця (1–12)
                for (int month = 1; month <= 12; month++)
                {
                    // Перевіряємо, чи вже є запис для цього місяця в колекції
                    if (SalariesForCurrentUser.Any(s => s.Month == month))
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
                    SalariesForCurrentUser.Add(salary);
                }

                Console.WriteLine($"Loaded {SalariesForCurrentUser.Count} unique salaries for CurrentUser in year {SelectedYear}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadSalaryReportAsync Error: {ex}");
                throw;
            }
        }

        private void CalculateTotal()
        {
            TotalReceived = SalariesForSelectedUser.Sum(s => s.Received);
        }

        private async Task EditSalaryAsync(Salary salary)
        {
            if (SelectedUser == null)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Оберіть користувача перед нарахуванням.", "OK");
                Console.WriteLine("EditSalaryAsync: SelectedUser is null");
                return;
            }
            if (salary == null)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося отримати дані про зарплату.", "OK");
                Console.WriteLine("EditSalaryAsync: Salary is null");
                return;
            }

            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new EditSalaryPage(new EditSalaryViewModel(SelectedUser, salary, _databaseService)));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося відкрити сторінку нарахування: {ex.Message}", "OK");
                Console.WriteLine($"EditSalaryAsync Error: {ex}");
            }
        }

        private async Task GenerateReportAsync()
        {
            var folder = await FolderPicker.Default.PickAsync();
            if (folder == null || !folder.IsSuccessful) return;

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);

            gfx.DrawString($"Звіт по зарплаті {_currentUser.FullName} за {SelectedYear}", font, XBrushes.Black, new XRect(0, 0, page.Width, 20), XStringFormats.Center);
            int y = 40;
            gfx.DrawString("Місяць | Дата | Нараховано | ПДФО | ПЗ | ВЗ | ФЗФ | Премія | Отримано", font, XBrushes.Black, new XRect(20, y, page.Width, 20), XStringFormats.TopLeft);
            y += 20;
            foreach (var salary in SalariesForCurrentUser)
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

        private async Task LogoutAsync()
        {
            App.CurrentUser = null;
            await Shell.Current.Navigation.PopToRootAsync();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        private string GetMonthName(int month) => new[] { "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень" }[month - 1];

        private async Task SubmitComplaintAsync()
        {
            if (string.IsNullOrWhiteSpace(ComplaintText))
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Введіть текст скарги.", "OK");
                return;
            }
            var complaint = new Complaint
            {
                UserId = _currentUser.Id,
                Username = _currentUser.Username,
                Text = ComplaintText,
                Date = DateTime.Now,
                IsRoleChangeRequest = false
            };
            await _databaseService.SaveComplaintAsync(complaint);
            await Application.Current.MainPage.DisplayAlert("Успіх", "Скаргу надіслано.", "OK");
            ComplaintText = string.Empty;
        }
    }
}