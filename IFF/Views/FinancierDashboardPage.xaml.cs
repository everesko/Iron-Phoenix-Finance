using IFF.ViewModels;
using IFF.Models;

namespace IFF.Views
{
    public partial class FinancierDashboardPage : TabbedPage
    {
        private readonly FinancierDashboardViewModel _viewModel;

        public FinancierDashboardPage(User currentUser)
        {
            try
            {
                InitializeComponent();
                _viewModel = new FinancierDashboardViewModel(currentUser);
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FinancierDashboardPage Constructor Error: {ex}");
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Помилка при завантаженні сторінки: {ex.Message}", "OK");
                Console.WriteLine($"OnAppearing Error: {ex}");
            }
        }
    }
}