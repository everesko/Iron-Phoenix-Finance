using IFF.ViewModels;
using IFF.Models;

namespace IFF.Views
{
    public partial class AdminDashboardPage : TabbedPage
    {
        private readonly AdminDashboardViewModel _viewModel;

        public AdminDashboardPage(User currentUser)
        {
            InitializeComponent();
            _viewModel = new AdminDashboardViewModel(currentUser); // �������� User � ViewModel
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}