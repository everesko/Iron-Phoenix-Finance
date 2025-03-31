using IFF.Views;

namespace IFF
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Реєстрація маршрутів
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(EditSalaryPage), typeof(EditSalaryPage));
            Routing.RegisterRoute(nameof(TermsPage), typeof(TermsPage));
        }
    }
}