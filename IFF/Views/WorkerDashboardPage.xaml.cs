using IFF.ViewModels;
using IFF.Models;

namespace IFF.Views
{
    public partial class WorkerDashboardPage : TabbedPage
    {
        private readonly WorkerDashboardViewModel _viewModel;

        public WorkerDashboardPage(User currentUser)
        {
            InitializeComponent();
            _viewModel = new WorkerDashboardViewModel(currentUser);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync(); // Якщо є асинхронна ініціалізація
        }
    }
}