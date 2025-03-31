using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using IFF.Models;
using IFF.Services;

namespace IFF.ViewModels
{
    public class EditSalaryViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private Salary _salary;

        public DateTime AccrualDate
        {
            get => _salary.AccrualDate;
            set
            {
                _salary.AccrualDate = value;
                _salary.Year = value.Year;
                _salary.Month = value.Month;
                OnPropertyChanged();
                Calculate();
            }
        }

        public string FullName => _currentUser.FullName;
        public string Role => _currentUser.Role;

        public decimal BaseSalary
        {
            get => _salary.Accrued; // Початковий оклад
            set
            {
                _salary.Accrued = value;
                OnPropertyChanged();
                Calculate();
            }
        }

        public int WorkDays
        {
            get => _salary.WorkDays ?? 0;
            set
            {
                _salary.WorkDays = value;
                OnPropertyChanged();
                Calculate();
            }
        }

        public int VacationDays
        {
            get => _salary.VacationDays ?? 0;
            set
            {
                _salary.VacationDays = value;
                OnPropertyChanged();
                Calculate();
            }
        }

        public int SickDays
        {
            get => _salary.SickDays ?? 0;
            set
            {
                _salary.SickDays = value;
                OnPropertyChanged();
                Calculate();
            }
        }

        // Властивості з результатами розрахунку
        public decimal Accrued => _salary.Accrued; // Залишаємо як базовий оклад, але не перезаписуємо в Calculate
        public decimal Pdfo => _salary.Pdfo;
        public decimal Pension => _salary.Pension;
        public decimal Military => _salary.Military;
        public decimal ZfFund => _salary.ZfFund;
        public decimal Bonus => _salary.Bonus;
        public decimal Received => _salary.Received;

        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CancelCommand { get; }

        public EditSalaryViewModel(User currentUser, Salary salary, DatabaseService databaseService)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _salary = salary ?? new Salary
            {
                UserId = currentUser.Id,
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                AccrualDate = DateTime.Now
            };
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

            SaveCommand = new Command(async () => await SaveAsync());
            ClearCommand = new Command(Clear);
            CancelCommand = new Command(async () => await Application.Current.MainPage.Navigation.PopAsync());
        }

        private void Calculate()
        {
            int daysInMonth = DateTime.DaysInMonth(_salary.Year, _salary.Month);
            decimal dailyRate = BaseSalary / daysInMonth;

            // Перевіряємо, щоб кількість днів не перевищувала днів у місяці
            int totalDays = WorkDays + VacationDays + SickDays;
            if (totalDays > daysInMonth)
            {
                WorkDays = daysInMonth;
                VacationDays = 0;
                SickDays = 0;
            }

            // Розрахунок нарахованої суми з урахуванням відпрацьованих, відпускних і лікарняних днів
            decimal accrued = (dailyRate * WorkDays) + (dailyRate * 0.75m * VacationDays) + (dailyRate * 0.5m * SickDays);
            _salary.Accrued = accrued;

            _salary.Pdfo = accrued * -0.05m;       // НДФЛ -5%
            _salary.Pension = accrued * -0.07m;    // ПЗ -7%
            _salary.Military = accrued * -0.03m;   // ВЗ -3%
            _salary.ZfFund = accrued * -0.03m;     // ФЗФ -3%
            _salary.Bonus = accrued * 0.25m;       // Премія +25%
            _salary.Received = accrued + _salary.Pdfo + _salary.Pension + _salary.Military + _salary.ZfFund + _salary.Bonus;

            // Оновлюємо UI для всіх залежних властивостей
            OnPropertyChanged(nameof(Accrued));
            OnPropertyChanged(nameof(Pdfo));
            OnPropertyChanged(nameof(Pension));
            OnPropertyChanged(nameof(Military));
            OnPropertyChanged(nameof(ZfFund));
            OnPropertyChanged(nameof(Bonus));
            OnPropertyChanged(nameof(Received));
            OnPropertyChanged(nameof(WorkDays));
            OnPropertyChanged(nameof(VacationDays));
            OnPropertyChanged(nameof(SickDays));
        }

        private async Task SaveAsync()
        {
            try
            {
                await _databaseService.SaveSalaryAsync(_salary);
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося зберегти зарплату: {ex.Message}", "OK");
                Console.WriteLine($"SaveAsync Error: {ex}");
            }
        }

        private void Clear()
        {
            _salary.Accrued = 0;
            _salary.WorkDays = 0;
            _salary.VacationDays = 0;
            _salary.SickDays = 0;
            _salary.Pdfo = 0;
            _salary.Pension = 0;
            _salary.Military = 0;
            _salary.ZfFund = 0;
            _salary.Bonus = 0;
            _salary.Received = 0;

            OnPropertyChanged(nameof(BaseSalary));
            OnPropertyChanged(nameof(WorkDays));
            OnPropertyChanged(nameof(VacationDays));
            OnPropertyChanged(nameof(SickDays));
            OnPropertyChanged(nameof(Accrued));
            OnPropertyChanged(nameof(Pdfo));
            OnPropertyChanged(nameof(Pension));
            OnPropertyChanged(nameof(Military));
            OnPropertyChanged(nameof(ZfFund));
            OnPropertyChanged(nameof(Bonus));
            OnPropertyChanged(nameof(Received));
        }
    }
}