using IFF.ViewModels;

namespace IFF.Views
{
    public partial class EditSalaryPage : ContentPage
    {
        public EditSalaryPage(EditSalaryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}