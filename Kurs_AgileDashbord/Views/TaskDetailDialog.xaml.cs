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
        private readonly TaskItem _task;
        private readonly User _currentUser;

        public TaskDetailDialog(TaskItem task, User currentUser)
        {
            InitializeComponent();
            _task = task;
            _currentUser = currentUser;

            // Скрываем кнопку удаления, если не автор и не админ
            if (!_currentUser.IsAdmin && _currentUser.UserID != _task.AuthorID)
            {
                DeleteTaskButton.Visibility = Visibility.Collapsed;
            }

            LoadTaskDetails();
        }

        private async void LoadTaskDetails()
        {
            // Заголовок
            TitleBlock.Text = _task.Title;
            DescriptionBlock.Text = _task.Description ?? "Нет описания";

            // Приоритет
            PriorityText.Text = _task.PriorityDisplayName;
            PriorityBadge.Background = new SolidColorBrush(GetPriorityColor(_task.Priority));

            // Статус
            StatusText.Text = _task.StatusDisplayName;
            StatusBadge.Background = new SolidColorBrush(GetStatusColor(_task.Status));

            // Информация
            ProjectBlock.Text = _task.Project?.ToString() ?? "—";
            SprintBlock.Text = _task.Sprint?.SprintName ?? "Бэклог";
            AuthorBlock.Text = _task.Author?.FullName ?? "—";
            ExecutorBlock.Text = _task.Executor?.FullName ?? "Не назначен";
            CreatedAtBlock.Text = _task.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            CompletedAtBlock.Text = _task.CompletedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—";

            await RefreshCommentsAsync();
        }

        private async Task RefreshCommentsAsync()
        {
            try
            {
                using var db = new AgileBoardContext();
                var comments = await db.Comments
                    .Include(c => c.User)
                    .Where(c => c.TaskID == _task.TaskID)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                if (comments.Count > 0)
                {
                    CommentsPanel.ItemsSource = comments;
                    NoCommentsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CommentsPanel.ItemsSource = null;
                    NoCommentsText.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                NoCommentsText.Text = "Ошибка загрузки комментариев";
                NoCommentsText.Visibility = Visibility.Visible;
            }
        }

        private async void OnAddCommentClick(object sender, RoutedEventArgs e)
        {
            var text = CommentTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                using var db = new AgileBoardContext();
                var comment = new Comment
                {
                    TaskID = _task.TaskID,
                    UserID = _currentUser.UserID,
                    Text = text,
                    CreatedAt = DateTime.Now
                };

                db.Comments.Add(comment);
                await db.SaveChangesAsync();

                CommentTextBox.Text = string.Empty;
                await RefreshCommentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnDeleteTaskClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить задачу \"{_task.Title}\"?", 
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var db = new AgileBoardContext();
                    var taskToDelete = await db.Tasks.FindAsync(_task.TaskID);
                    if (taskToDelete != null)
                    {
                        db.Tasks.Remove(taskToDelete);
                        await db.SaveChangesAsync();

                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
