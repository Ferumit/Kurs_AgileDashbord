using System.Windows;
using Kurs_AgileDashbord.Data;
using Kurs_AgileDashbord.Models;
using Microsoft.EntityFrameworkCore;

namespace Kurs_AgileDashbord.Views
{
    public partial class LoginWindow : Window
    {
        public User? LoggedInUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите email и пароль");
                return;
            }

            ErrorText.Visibility = Visibility.Collapsed;

            try
            {
                using var db = new AgileBoardContext();
                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);

                if (user == null)
                {
                    ShowError("Неверный email или пароль");
                    return;
                }

                // Проверяем статус аккаунта
                if (user.Status == "Pending")
                {
                    ShowError("Ваша заявка ещё не подтверждена администратором");
                    return;
                }

                if (user.Status == "Rejected")
                {
                    ShowError("Ваша заявка отклонена. Обратитесь к администратору.");
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

        private void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow { Owner = this };
            registerWindow.ShowDialog();
            // После закрытия окна регистрации — просто возвращаемся к форме входа
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

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
