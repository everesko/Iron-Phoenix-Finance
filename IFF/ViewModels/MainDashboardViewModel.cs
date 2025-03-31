using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFF.ViewModels
{
    internal class MainDashboardViewModel : BaseViewModel
    {
        private string _welcomeMessage;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { _welcomeMessage = value; OnPropertyChanged(); }
        }

        public MainDashboardViewModel()
        {
            WelcomeMessage = "Ласкаво просимо до IFF System!";
        }
    }
}
