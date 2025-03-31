using IFF.Services;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using IFF.Models;

namespace IFF
{
    public partial class App : Application
    {
        private readonly DatabaseService _databaseService;
        public static User CurrentUser { get; set; }

        public App()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            MainPage = new AppShell();

            // Асинхронна ініціалізація бази даних
            InitializeDatabaseAsync();
        }

        protected override void OnStart()
        {
            CurrentUser = null; // Очищаємо при старті
        }

        private async void InitializeDatabaseAsync()
        {
            try
            {
                await _databaseService.InitializeAsync();
            }
            catch (Exception ex)
            {
                await MainPage.DisplayAlert("Помилка", $"Не вдалося ініціалізувати базу даних: {ex.Message}", "OK");
            }
        }
    }
}