using System.Windows;
using System.Windows.Controls;
using Kurs_AgileDashbord.Data;
using Kurs_AgileDashbord.Models;

namespace Kurs_AgileDashbord.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private async void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            // Простая валидация полей
            if (string.IsNullOrWhiteSpace(FullNameBox.Text))
            { ShowError("Введите полное имя"); return; }

            if (string.IsNullOrWhiteSpace(EmailBox.Text) || !EmailBox.Text.Contains('@'))
            { ShowError("Введите корректный email"); return; }

            if (PasswordBox.Password.Length < 4)
            { ShowError("Пароль должен быть не менее 4 символов"); return; }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            { ShowError("Пароли не совпадают"); return; }

            var role = (RoleCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Developer";

            try
            {
                using var db = new AgileBoardContext();

                // Проверяем что такой email не занят
                var existingUser = db.Users.FirstOrDefault(u => u.Email == EmailBox.Text.Trim());
                if (existingUser != null)
                {
                    if (existingUser.Status == "Pending")
                        ShowError("Заявка с этим email уже отправлена и ожидает подтверждения");
                    else
                        ShowError("Пользователь с таким email уже зарегистрирован");
                    return;
                }

                // Создаём пользователя со статусом Pending
                var newUser = new User
                {
                    FullName = FullNameBox.Text.Trim(),
                    Email = EmailBox.Text.Trim().ToLower(),
                    PasswordHash = PasswordBox.Password,
                    Role = role,
                    AvatarColor = GetRandomAvatarColor(),
                    Status = "Pending",
                    IsAdmin = false
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                MessageBox.Show(
                    $"Заявка отправлена!\n\nАдминистратор рассмотрит вашу заявку и подтвердит аккаунт.\nКак только это произойдёт — вы сможете войти.",
                    "Заявка принята", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при создании заявки: {ex.Message}");
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        // Случайный цвет аватара из набора
        private static string GetRandomAvatarColor()
        {
            var colors = new[] { "#7C4DFF", "#00BCD4", "#FF5722", "#4CAF50", "#FF9800", "#E91E63", "#9C27B0", "#2196F3" };
            return colors[new Random().Next(colors.Length)];
        }
    }
}
