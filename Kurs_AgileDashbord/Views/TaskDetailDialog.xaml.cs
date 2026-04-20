using System.Windows;
using System.Windows.Media;
using Kurs_AgileDashbord.Data;
using Kurs_AgileDashbord.Models;
using Microsoft.EntityFrameworkCore;

namespace Kurs_AgileDashbord.Views
{
    /// <summary>
    /// Диалог с детальной информацией о задаче и комментариями
    /// </summary>
    public partial class TaskDetailDialog : Window
    {
        public TaskDetailDialog(TaskItem task)
        {
            InitializeComponent();
            LoadTaskDetails(task);
        }

        private async void LoadTaskDetails(TaskItem task)
        {
            // Заголовок
            TitleBlock.Text = task.Title;
            DescriptionBlock.Text = task.Description ?? "Нет описания";

            // Приоритет
            PriorityText.Text = task.PriorityDisplayName;
            PriorityBadge.Background = new SolidColorBrush(GetPriorityColor(task.Priority));

            // Статус
            StatusText.Text = task.StatusDisplayName;
            StatusBadge.Background = new SolidColorBrush(GetStatusColor(task.Status));

            // Информация
            ProjectBlock.Text = task.Project?.ToString() ?? "—";
            SprintBlock.Text = task.Sprint?.SprintName ?? "Бэклог";
            AuthorBlock.Text = task.Author?.FullName ?? "—";
            ExecutorBlock.Text = task.Executor?.FullName ?? "Не назначен";
            CreatedAtBlock.Text = task.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            CompletedAtBlock.Text = task.CompletedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—";

            // Загружаем комментарии из БД
            try
            {
                using var db = new AgileBoardContext();
                var comments = await db.Comments
                    .Include(c => c.User)
                    .Where(c => c.TaskID == task.TaskID)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                if (comments.Count > 0)
                {
                    CommentsPanel.ItemsSource = comments;
                    NoCommentsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoCommentsText.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                NoCommentsText.Text = "Ошибка загрузки комментариев";
                NoCommentsText.Visibility = Visibility.Visible;
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private static Color GetPriorityColor(string priority) => priority switch
        {
            "Low" => (Color)ColorConverter.ConvertFromString("#78909C"),
            "Medium" => (Color)ColorConverter.ConvertFromString("#42A5F5"),
            "High" => (Color)ColorConverter.ConvertFromString("#FFA726"),
            "Critical" => (Color)ColorConverter.ConvertFromString("#EF5350"),
            _ => Colors.Gray
        };

        private static Color GetStatusColor(string status) => status switch
        {
            "To Do" => (Color)ColorConverter.ConvertFromString("#78909C"),
            "In Progress" => (Color)ColorConverter.ConvertFromString("#42A5F5"),
            "Review" => (Color)ColorConverter.ConvertFromString("#FFA726"),
            "Done" => (Color)ColorConverter.ConvertFromString("#66BB6A"),
            _ => Colors.Gray
        };
    }
}
