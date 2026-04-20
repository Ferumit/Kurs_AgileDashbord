using System.Windows;
using Kurs_AgileDashbord.Data;
using Kurs_AgileDashbord.Views;

namespace Kurs_AgileDashbord
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Не закрывать приложение когда LoginWindow закрывается
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Ошибка: {args.Exception.Message}\n\n{args.Exception.InnerException?.Message}",
                    "AgileBoard — Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            // === Инициализация базы данных ===
            try
            {
                var connStr = DatabaseInitializer.InitializeAndGetConnectionString();
                AgileBoardContext.SetConnectionString(connStr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AgileBoard — Нет подключения к БД",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // === Авторизация ===
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true && loginWindow.LoggedInUser != null)
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                var mainWindow = new MainWindow(loginWindow.LoggedInUser);
                MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}
