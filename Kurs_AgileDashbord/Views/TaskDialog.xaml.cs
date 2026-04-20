using System.Windows;
using System.Windows.Controls;
using Kurs_AgileDashbord.Data;
using Kurs_AgileDashbord.Models;
using Microsoft.EntityFrameworkCore;

namespace Kurs_AgileDashbord.Views
{
    /// <summary>
    /// Диалог создания новой задачи
    /// </summary>
    public partial class TaskDialog : Window
    {
        private readonly List<Project> _projects;
        private readonly List<User> _users;
        private readonly User _currentUser;

        public TaskItem? CreatedTask { get; private set; }

        public TaskDialog(List<Project> projects, List<User> users, List<Sprint> sprints, 
                          Project? selectedProject = null, User? currentUser = null)
        {
            InitializeComponent();

            _projects = projects;
            _users = users;
            _currentUser = currentUser ?? users.First();

            ProjectCombo.ItemsSource = _projects;
            ExecutorCombo.ItemsSource = _users;

            if (selectedProject != null)
            {
                ProjectCombo.SelectedItem = _projects.FirstOrDefault(p => p.ProjectID == selectedProject.ProjectID);
            }
        }

        /// <summary>
        /// При смене проекта — загрузить спринты этого проекта из БД
        /// </summary>
        private async void OnProjectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedItem is Project project)
            {
                try
                {
                    using var db = new AgileBoardContext();
                    var sprints = await db.Sprints
                        .Where(s => s.ProjectID == project.ProjectID && s.IsActive)
                        .OrderBy(s => s.SprintName)
                        .ToListAsync();
                    
                    SprintCombo.ItemsSource = sprints;
                }
                catch
                {
                    SprintCombo.ItemsSource = null;
                }
            }
        }

        private void OnCreateClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProjectCombo.SelectedItem is not Project project)
            {
                MessageBox.Show("Выберите проект", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var priority = "Medium";
            if (PriorityCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                priority = tag;

            CreatedTask = new TaskItem
            {
                Title = TitleBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim(),
                ProjectID = project.ProjectID,
                SprintID = (SprintCombo.SelectedItem as Sprint)?.SprintID,
                AuthorID = _currentUser.UserID,
                ExecutorID = (ExecutorCombo.SelectedItem as User)?.UserID,
                Priority = priority,
                Status = "To Do"
            };

            DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
