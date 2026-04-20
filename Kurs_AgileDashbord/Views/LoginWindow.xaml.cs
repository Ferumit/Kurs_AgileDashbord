using System.Windows;
using Kurs_AgileDashbord.Data;
using Kurs_AgileDashbord.Models;
using Microsoft.EntityFrameworkCore;

namespace Kurs_AgileDashbord.Views
{
    /// <summary>
    /// Окно авторизации — проверяет email и пароль из БД
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Авторизованный пользователь (после успешного входа)
        /// </summary>
        public User? LoggedInUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            EmailBox.Focus();
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text?.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите email и пароль");
                return;
            }

            try
            {
                using var db = new AgileBoardContext();
                var user = await db.Users.FirstOrDefaultAsync(u =>
                    u.Email == email && u.PasswordHash == password);

                if (user == null)
                {
                    ShowError("Неверный email или пароль");
                    return;
                }

                LoggedInUser = user;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения: {ex.Message}");
            }
        }

        private void OnQuickLoginAdmin(object sender, RoutedEventArgs e)
        {
            EmailBox.Text = "ivanov@company.kz";
            PasswordBox.Password = "admin123";
        }

        private void OnQuickLoginUser(object sender, RoutedEventArgs e)
        {
            EmailBox.Text = "petrova@company.kz";
            PasswordBox.Password = "user123";
        }

        private void ShowError(string text)
        {
            ErrorText.Text = text;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
