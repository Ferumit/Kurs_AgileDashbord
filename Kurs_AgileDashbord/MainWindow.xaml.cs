using System.Windows;
using System.Windows.Input;
using Kurs_AgileDashbord.Models;
using Kurs_AgileDashbord.ViewModels;
using Kurs_AgileDashbord.Views;

namespace Kurs_AgileDashbord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(User currentUser)
        {
            InitializeComponent();
            DataContext = new MainViewModel(currentUser);
        }

        /// <summary>
        /// Открытие диалога создания задачи
        /// </summary>
        private void OnCreateTaskClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;

            var dialog = new TaskDialog(vm.Projects.ToList(), vm.Users.ToList(), vm.Sprints.ToList(), vm.SelectedProject, vm.CurrentUser);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && dialog.CreatedTask != null)
            {
                vm.CreateTaskCommand.Execute(dialog.CreatedTask);
            }
        }

        /// <summary>
        /// Клик по карточке задачи — открытие детального просмотра
        /// </summary>
        private void OnTaskCardClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TaskItem task)
            {
                var vm = DataContext as MainViewModel;
                if (vm == null) return;

                var dialog = new TaskDetailDialog(task, vm.CurrentUser);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    vm.RefreshDataCommand.Execute(null);
                }
            }
        }
        // Открывает всплывающее меню профиля
        private void OnUserAvatarClick(object sender, RoutedEventArgs e)
        {
            UserPopup.IsOpen = true;
        }

        // Выход из аккаунта — закрываем главное окно и показываем экран входа заново
        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            UserPopup.IsOpen = false;

            // Переключаем режим, чтобы приложение не закрылось вместе с главным окном
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginWindow = new LoginWindow();
            this.Close(); // закрываем текущий сеанс (окно)

            // Важно использовать ShowDialog(), так как внутри окна делается DialogResult = true
            if (loginWindow.ShowDialog() == true && loginWindow.LoggedInUser != null)
            {
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                var mainWindow = new MainWindow(loginWindow.LoggedInUser);
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                // Если пользователь просто закрыл окно входа (крестиком)
                Application.Current.Shutdown();
            }
        }
    }
}