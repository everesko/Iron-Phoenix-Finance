using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using IFF.Models;
using IFF.Services;
using IFF.Views;

namespace IFF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isBusy;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                if (string.IsNullOrWhiteSpace(_username) && string.IsNullOrWhiteSpace(_password))
                    ErrorMessage = string.Empty; // Очищаємо повідомлення, якщо обидва поля порожні
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                if (string.IsNullOrWhiteSpace(_username) && string.IsNullOrWhiteSpace(_password))
                    ErrorMessage = string.Empty; // Очищаємо повідомлення, якщо обидва поля порожні
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel()
        {
            _databaseService = new DatabaseService();
            LoginCommand = new Command(async () => await LoginAsync());
            GoToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(RegisterPage)}"));
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
        }

        private async Task LoginAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Введіть логін і пароль.";
                    return;
                }

                var user = await _databaseService.GetUserAsync(Username, Password);
                if (user == null)
                {
                    ErrorMessage = "Невірний логін або пароль.";
                    return;
                }

                if (!user.HasAccess)
                {
                    ErrorMessage = "Ваш акаунт не має доступу. Зверніться до адміністратора.";
                    return;
                }

                App.CurrentUser = user;
                ErrorMessage = string.Empty;

                switch (user.Role)
                {
                    case nameof(UserRole.Стрілець):
                        Application.Current.MainPage = new NavigationPage(new WorkerDashboardPage(user));
                        break;
                    case nameof(UserRole.Фінансист):
                        Application.Current.MainPage = new NavigationPage(new FinancierDashboardPage(user));
                        break;
                    case nameof(UserRole.Адміністратор):
                        Application.Current.MainPage = new NavigationPage(new AdminDashboardPage(user));
                        break;
                    default:
                        ErrorMessage = "Невідома роль користувача.";
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Помилка входу: {ex.Message}";
                Console.WriteLine($"LoginAsync Error: {ex}");
            }
        }

    }
}