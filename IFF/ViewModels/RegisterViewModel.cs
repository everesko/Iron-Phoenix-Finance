using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using IFF.Services;
using IFF.Models;
using IFF.Views;

namespace IFF.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;
        private string _usernameStatus;
        private string _fullName;
        private bool _isTermsAccepted;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                UpdateUsernameStatus();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value?.Replace(" ", ""); // Видаляємо пробіли
                OnPropertyChanged();
                UpdatePasswordValidation();
                CheckPasswordsMatch();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                CheckPasswordsMatch();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string UsernameStatus
        {
            get => _usernameStatus;
            set { _usernameStatus = value; OnPropertyChanged(); }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public bool IsTermsAccepted
        {
            get => _isTermsAccepted;
            set { _isTermsAccepted = value; OnPropertyChanged(); }
        }

        public bool IsLengthValid => !string.IsNullOrEmpty(Password) && Password.Length > 6;
        public bool IsLatinOnly => !string.IsNullOrEmpty(Password) && Regex.IsMatch(Password, @"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]*$");
        public bool HasDigit => !string.IsNullOrEmpty(Password) && Regex.IsMatch(Password, @"\d");
        public bool HasSpecialChar => !string.IsNullOrEmpty(Password) && Regex.IsMatch(Password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]");
        public bool PasswordsMatch => string.IsNullOrEmpty(ConfirmPassword) || Password == ConfirmPassword;

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }
        public ICommand ShowTermsCommand { get; }
        public ICommand AcceptTermsCommand { get; } // Нова команда

        public RegisterViewModel()
        {
            _databaseService = new DatabaseService();
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(LoginPage)}"));
            ShowTermsCommand = new Command(async () => await Shell.Current.Navigation.PushAsync(new TermsPage(this)));
            AcceptTermsCommand = new Command(() => IsTermsAccepted = true); // Активуємо згоду
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FullName = string.Empty;
            ErrorMessage = string.Empty;
            UsernameStatus = string.Empty;
        }

        private async void UpdateUsernameStatus()
        {
            if (string.IsNullOrEmpty(Username))
                UsernameStatus = string.Empty;
            else if (Username.Length < 3)
                UsernameStatus = "Логін має бути довше 3 символів";
            else
            {
                var user = await _databaseService.GetUserByUsernameAsync(Username);
                UsernameStatus = user == null ? "Логін доступний" : "Логін уже є в базі";
            }
        }

        private void UpdatePasswordValidation()
        {
            OnPropertyChanged(nameof(IsLengthValid));
            OnPropertyChanged(nameof(IsLatinOnly));
            OnPropertyChanged(nameof(HasDigit));
            OnPropertyChanged(nameof(HasSpecialChar));
        }

        private void CheckPasswordsMatch()
        {
            OnPropertyChanged(nameof(PasswordsMatch));
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Заповніть усі поля.";
                return;
            }

            if (Username.Length < 3 || (await _databaseService.GetUserByUsernameAsync(Username)) != null)
            {
                ErrorMessage = "Логін недоступний або закороткий.";
                return;
            }

            if (!IsLengthValid || !IsLatinOnly || !HasDigit || !HasSpecialChar)
            {
                ErrorMessage = "Пароль не відповідає вимогам.";
                return;
            }

            if (!PasswordsMatch)
            {
                ErrorMessage = "Паролі не співпадають.";
                return;
            }

            if (!IsTermsAccepted)
            {
                ErrorMessage = "Прийміть умовну згоду.";
                return;
            }

            var newUser = new User
            {
                Username = Username,
                Password = BCrypt.Net.BCrypt.HashPassword(Password),
                FullName = FullName,
                Role = nameof(UserRole.Стрілець),
                HasAccess = false
            };

            await _databaseService.SaveUserAsync(newUser);
            ErrorMessage = string.Empty;
            await Shell.Current.DisplayAlert("Успіх", "Реєстрація успішна! Чекайте схвалення адміністратора.", "OK");
            await Shell.Current.GoToAsync($"///{nameof(LoginPage)}");
        }

        public void AcceptTerms()
        {
            IsTermsAccepted = true;
        }
    }
}