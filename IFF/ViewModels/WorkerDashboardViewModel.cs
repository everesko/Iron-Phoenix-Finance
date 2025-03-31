using System;
using System.Collections.ObjectModel;
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
    public class WorkerDashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private ObservableCollection<int> _availableYears;
        private int _selectedYear;
        private ObservableCollection<Salary> _salaries;
        private string _complaintText;
        private ObservableCollection<Complaint> _complaints;

        public ObservableCollection<int> AvailableYears
        {
            get => _availableYears;
            set { _availableYears = value; OnPropertyChanged(); }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); LoadSalaryReportAsync(); }
        }

        public ObservableCollection<Salary> Salaries
        {
            get => _salaries;
            set { _salaries = value; OnPropertyChanged(); }
        }

        public string ComplaintText
        {
            get => _complaintText;
            set { _complaintText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Complaint> Complaints
        {
            get => _complaints;
            set { _complaints = value; OnPropertyChanged(); }
        }

        public ICommand LogoutCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand SubmitComplaintCommand { get; }
        public ICommand RequestRoleChangeCommand { get; }

        public WorkerDashboardViewModel(User currentUser)
        {
            _databaseService = new DatabaseService();
            _currentUser = currentUser;
            Salaries = new ObservableCollection<Salary>();
            Complaints = new ObservableCollection<Complaint>();

            var hireDate = _currentUser.HireDate ?? DateTime.Now;
            int currentYear = DateTime.Now.Year;
            AvailableYears = new ObservableCollection<int>();
            for (int year = hireDate.Year; year <= currentYear + 1; year++)
                AvailableYears.Add(year);
            SelectedYear = currentYear;

            LogoutCommand = new Command(async () => await LogoutAsync());
            GenerateReportCommand = new Command(async () => await GenerateReportAsync());
            SubmitComplaintCommand = new Command(async () => await SubmitComplaintAsync());
            RequestRoleChangeCommand = new Command(async () => await RequestRoleChangeAsync());

            LoadSalaryReportAsync();
            LoadComplaintsAsync();
        }

        public async Task InitializeAsync()
        {
            await LoadSalaryReportAsync();
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
            Complaints.Add(complaint); // Додаємо до локального списку
            await Application.Current.MainPage.DisplayAlert("Успіх", "Скаргу надіслано.", "OK");
            ComplaintText = string.Empty;
        }

        private async Task RequestRoleChangeAsync()
        {
            if (string.IsNullOrWhiteSpace(ComplaintText))
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Введіть запит на зміну посади.", "OK");
                return;
            }
            var complaint = new Complaint
            {
                UserId = _currentUser.Id,
                Username = _currentUser.Username,
                Text = ComplaintText,
                Date = DateTime.Now,
                IsRoleChangeRequest = true
            };
            await _databaseService.SaveComplaintAsync(complaint);
            Complaints.Add(complaint); // Додаємо до локального списку
            await Application.Current.MainPage.DisplayAlert("Успіх", "Заявку на зміну посади надіслано.", "OK");
            ComplaintText = string.Empty;
        }

        private async void LoadComplaintsAsync()
        {
            var complaints = await _databaseService.GetComplaintsAsync();
            Complaints.Clear();
            foreach (var complaint in complaints.Where(c => c.UserId == _currentUser.Id))
                Complaints.Add(complaint);
        }

        private string GetMonthName(int month) => new[] { "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень" }[month - 1];
    }
}