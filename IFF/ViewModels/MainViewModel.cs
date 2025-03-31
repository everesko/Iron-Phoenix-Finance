using System.Windows.Input;
using Microsoft.Maui.Controls;
using IFF.Views;

namespace IFF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public MainViewModel()
        {
            LoginCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(LoginPage)}"));
            RegisterCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(RegisterPage)}"));
        }
    }
}
