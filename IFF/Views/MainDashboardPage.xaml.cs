using IFF.ViewModels;

namespace IFF.Views;

public partial class MainDashboardPage : ContentPage
{
    public MainDashboardPage()
    {
        InitializeComponent();
        BindingContext = new MainDashboardViewModel();
    }
}