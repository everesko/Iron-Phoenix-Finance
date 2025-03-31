using IFF.ViewModels;

namespace IFF.Views
{
    public partial class TermsPage : ContentPage
    {
        private readonly RegisterViewModel _viewModel;

        public TermsPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnAcceptClicked(object sender, EventArgs e)
        {
            _viewModel.AcceptTerms();
            await Navigation.PopAsync();
        }
    }
}