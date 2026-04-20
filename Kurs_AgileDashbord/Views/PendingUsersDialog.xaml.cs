using System.Windows;
using System.Windows.Controls;
using Kurs_AgileDashbord.Data;
using Microsoft.EntityFrameworkCore;

namespace Kurs_AgileDashbord.Views
{
    public partial class PendingUsersDialog : Window
    {
        public PendingUsersDialog()
        {
            InitializeComponent();
            _ = LoadPendingUsers();
        }

        private async Task LoadPendingUsers()
        {
            using var db = new AgileBoardContext();
            var pending = await db.Users
                .Where(u => u.Status == "Pending")
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            PendingList.ItemsSource = pending;

            SubtitleText.Text = pending.Count == 0
                ? "Новых заявок нет"
                : $"Ожидают подтверждения: {pending.Count}";
        }

        private async void OnApproveClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId)
                await SetUserStatus(userId, "Active", "Заявка одобрена");
        }

        private async void OnRejectClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId)
            {
                var result = MessageBox.Show(
                    "Отклонить заявку? Пользователь не сможет войти в систему.",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    await SetUserStatus(userId, "Rejected", "Заявка отклонена");
            }
        }

        private async Task SetUserStatus(int userId, string status, string message)
        {
            try
            {
                using var db = new AgileBoardContext();
                var user = await db.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Status = status;
                    await db.SaveChangesAsync();
                    MessageBox.Show(message, "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPendingUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
    }
}
