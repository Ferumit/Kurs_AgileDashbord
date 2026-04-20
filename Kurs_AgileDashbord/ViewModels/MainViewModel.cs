using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kurs_AgileDashbord.Data;
using Kurs_AgileDashbord.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace Kurs_AgileDashbord.ViewModels
{
    /// <summary>
    /// Главный ViewModel приложения — управляет навигацией, данными и правами доступа
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        // === Текущий пользователь ===
        [ObservableProperty]
        private User _currentUser = null!;

        [ObservableProperty]
        private bool _isAdmin;

        // === Коллекции данных ===
        [ObservableProperty]
        private ObservableCollection<TaskItem> _toDoTasks = new();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _inProgressTasks = new();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _reviewTasks = new();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _doneTasks = new();

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private ObservableCollection<Sprint> _sprints = new();

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        // === Отчёты ===
        [ObservableProperty]
        private ObservableCollection<SprintReportItem> _sprintReports = new();

        [ObservableProperty]
        private ObservableCollection<UserProductivityItem> _userProductivity = new();

        // === Фильтры ===
        [ObservableProperty]
        private Project? _selectedProject;

        [ObservableProperty]
        private Sprint? _selectedSprint;

        [ObservableProperty]
        private User? _selectedExecutorFilter;

        [ObservableProperty]
        private string _searchText = string.Empty;

        // === Навигация ===
        [ObservableProperty]
        private string _currentView = "Kanban";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusBarText = "Готово";

        // === Статистика ===
        [ObservableProperty]
        private int _totalTasksCount;

        [ObservableProperty]
        private int _activeSprintsCount;

        [ObservableProperty]
        private int _teamMembersCount;

        public MainViewModel() { }

        public MainViewModel(User currentUser)
        {
            CurrentUser = currentUser;
            IsAdmin = currentUser.IsAdmin;
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Загрузка всех данных из БД
        /// </summary>
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            StatusBarText = "Загрузка данных...";

            try
            {
                using var db = new AgileBoardContext();

                var projects = await db.Projects.OrderBy(p => p.ProjectName).ToListAsync();
                var users = await db.Users.OrderBy(u => u.FullName).ToListAsync();

                Projects = new ObservableCollection<Project>(projects);
                Users = new ObservableCollection<User>(users);

                TeamMembersCount = users.Count;
                ActiveSprintsCount = await db.Sprints.CountAsync(s => s.IsActive);

                if (SelectedProject == null && Projects.Count > 0)
                    SelectedProject = Projects[0];
                else
                    await LoadSprintsAndTasksAsync();
            }
            catch (Exception ex)
            {
                StatusBarText = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Загрузка спринтов для выбранного проекта
        /// </summary>
        private async Task LoadSprintsAsync()
        {
            if (SelectedProject == null) return;

            using var db = new AgileBoardContext();
            var sprints = await db.Sprints
                .Where(s => s.ProjectID == SelectedProject.ProjectID)
                .OrderByDescending(s => s.IsActive)
                .ThenByDescending(s => s.StartDate)
                .ToListAsync();

            Sprints = new ObservableCollection<Sprint>(sprints);
        }

        /// <summary>
        /// Загрузка спринтов + задач (вызывается при смене проекта)
        /// </summary>
        private async Task LoadSprintsAndTasksAsync()
        {
            await LoadSprintsAsync();
            SelectedSprint = null;
            await LoadTasksAsync();
        }

        /// <summary>
        /// Загрузка задач с учётом фильтров
        /// </summary>
        private async Task LoadTasksAsync()
        {
            if (SelectedProject == null) return;

            IsLoading = true;

            try
            {
                using var db = new AgileBoardContext();

                var query = db.Tasks
                    .Include(t => t.Author)
                    .Include(t => t.Executor)
                    .Include(t => t.Sprint)
                    .Include(t => t.Project)
                    .Include(t => t.Comments)
                    .Where(t => t.ProjectID == SelectedProject.ProjectID);

                // Фильтр по спринту
                if (SelectedSprint != null)
                    query = query.Where(t => t.SprintID == SelectedSprint.SprintID);

                // Фильтр по исполнителю
                if (SelectedExecutorFilter != null)
                    query = query.Where(t => t.ExecutorID == SelectedExecutorFilter.UserID);

                // Для не-админов: показываем только их задачи
                if (!IsAdmin)
                    query = query.Where(t => t.ExecutorID == CurrentUser.UserID || t.AuthorID == CurrentUser.UserID);

                // Поиск по тексту
                if (!string.IsNullOrWhiteSpace(SearchText))
                    query = query.Where(t => t.Title.Contains(SearchText) ||
                                             (t.Description != null && t.Description.Contains(SearchText)));

                var tasks = await query.OrderBy(t => t.CreatedAt).ToListAsync();

                ToDoTasks = new ObservableCollection<TaskItem>(tasks.Where(t => t.Status == "To Do"));
                InProgressTasks = new ObservableCollection<TaskItem>(tasks.Where(t => t.Status == "In Progress"));
                ReviewTasks = new ObservableCollection<TaskItem>(tasks.Where(t => t.Status == "Review"));
                DoneTasks = new ObservableCollection<TaskItem>(tasks.Where(t => t.Status == "Done"));

                TotalTasksCount = tasks.Count;
                StatusBarText = $"Загружено задач: {tasks.Count} | {CurrentUser.FullName} ({(IsAdmin ? "Администратор" : CurrentUser.Role)})";
            }
            catch (Exception ex)
            {
                StatusBarText = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Загрузка данных для отчётов
        /// </summary>
        private async Task LoadReportsAsync()
        {
            if (SelectedProject == null) return;

            IsLoading = true;

            try
            {
                using var db = new AgileBoardContext();

                // Отчёт по спринтам (из View vw_SprintReport)
                var sprintData = await db.Tasks
                    .Include(t => t.Sprint)
                    .Where(t => t.ProjectID == SelectedProject.ProjectID && t.SprintID != null)
                    .GroupBy(t => new { t.Sprint!.SprintName, t.Sprint.StartDate, t.Sprint.EndDate, t.Sprint.IsActive })
                    .Select(g => new SprintReportItem
                    {
                        SprintName = g.Key.SprintName,
                        StartDate = g.Key.StartDate,
                        EndDate = g.Key.EndDate,
                        IsActive = g.Key.IsActive,
                        TotalTasks = g.Count(),
                        ToDo = g.Count(t => t.Status == "To Do"),
                        InProgress = g.Count(t => t.Status == "In Progress"),
                        InReview = g.Count(t => t.Status == "Review"),
                        Done = g.Count(t => t.Status == "Done")
                    })
                    .OrderByDescending(s => s.IsActive)
                    .ThenByDescending(s => s.StartDate)
                    .ToListAsync();

                SprintReports = new ObservableCollection<SprintReportItem>(sprintData);

                // Продуктивность по пользователям
                var userData = await db.Tasks
                    .Include(t => t.Executor)
                    .Where(t => t.ProjectID == SelectedProject.ProjectID && t.ExecutorID != null)
                    .GroupBy(t => new { t.Executor!.FullName, t.Executor.Role, t.Executor.AvatarColor })
                    .Select(g => new UserProductivityItem
                    {
                        FullName = g.Key.FullName,
                        Role = g.Key.Role,
                        AvatarColor = g.Key.AvatarColor,
                        TotalTasks = g.Count(),
                        CompletedTasks = g.Count(t => t.Status == "Done"),
                        InProgressTasks = g.Count(t => t.Status == "In Progress" || t.Status == "Review")
                    })
                    .OrderByDescending(u => u.CompletedTasks)
                    .ToListAsync();

                UserProductivity = new ObservableCollection<UserProductivityItem>(userData);

                StatusBarText = $"Отчёт загружен: {sprintData.Count} спринтов, {userData.Count} участников";
            }
            catch (Exception ex)
            {
                StatusBarText = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // === Обработчики изменения фильтров ===
        partial void OnSelectedProjectChanged(Project? value) => _ = LoadSprintsAndTasksAsync();
        partial void OnSelectedSprintChanged(Sprint? value) => _ = LoadTasksAsync();
        partial void OnSelectedExecutorFilterChanged(User? value) => _ = LoadTasksAsync();
        partial void OnSearchTextChanged(string value) => _ = LoadTasksAsync();

        // === Команды ===

        [RelayCommand]
        private async Task RefreshData() => await LoadDataAsync();

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedSprint = null;
            SelectedExecutorFilter = null;
            SearchText = string.Empty;
        }

        [RelayCommand]
        private async Task MoveTaskStatus(TaskItem? task)
        {
            if (task == null) return;

            // Проверка прав: не-админ может двигать только свои задачи
            if (!IsAdmin && task.ExecutorID != CurrentUser.UserID)
            {
                StatusBarText = "⚠ Вы можете менять статус только своих задач";
                return;
            }

            var nextStatus = task.Status switch
            {
                "To Do" => "In Progress",
                "In Progress" => "Review",
                "Review" => "Done",
                "Done" => "To Do",
                _ => task.Status
            };

            try
            {
                using var db = new AgileBoardContext();
                var dbTask = await db.Tasks.FindAsync(task.TaskID);
                if (dbTask != null)
                {
                    dbTask.Status = nextStatus;
                    await db.SaveChangesAsync();
                    StatusBarText = $"Задача \"{task.Title}\" → {GetStatusDisplayName(nextStatus)}";
                }
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                StatusBarText = $"Ошибка: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task CreateTask(TaskItem? newTask)
        {
            if (newTask == null || string.IsNullOrWhiteSpace(newTask.Title)) return;

            try
            {
                using var db = new AgileBoardContext();
                db.Tasks.Add(newTask);
                await db.SaveChangesAsync();
                StatusBarText = $"Задача \"{newTask.Title}\" создана";
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                StatusBarText = $"Ошибка: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteTask(TaskItem? task)
        {
            if (task == null) return;

            // Только админ может удалять задачи
            if (!IsAdmin)
            {
                StatusBarText = "⚠ Удаление доступно только администратору";
                return;
            }

            try
            {
                using var db = new AgileBoardContext();
                var dbTask = await db.Tasks.FindAsync(task.TaskID);
                if (dbTask != null)
                {
                    db.Tasks.Remove(dbTask);
                    await db.SaveChangesAsync();
                    StatusBarText = $"Задача \"{task.Title}\" удалена";
                }
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                StatusBarText = $"Ошибка: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ShowKanban() => CurrentView = "Kanban";

        [RelayCommand]
        private async Task ShowReports()
        {
            CurrentView = "Reports";
            await LoadReportsAsync();
        }

        private static string GetStatusDisplayName(string status) => status switch
        {
            "To Do" => "К выполнению",
            "In Progress" => "В работе",
            "Review" => "На проверке",
            "Done" => "Готово",
            _ => status
        };
    }

    // === Модели для отчётов ===

    public class SprintReportItem
    {
        public string SprintName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalTasks { get; set; }
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int InReview { get; set; }
        public int Done { get; set; }
        public double CompletionPercent => TotalTasks > 0 ? Math.Round((double)Done / TotalTasks * 100, 1) : 0;
        public string DateRange => $"{StartDate:dd.MM} – {EndDate:dd.MM.yyyy}";
        public string StatusLabel => IsActive ? "🟢 Активный" : "⚫ Завершён";
    }

    public class UserProductivityItem
    {
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public string AvatarColor { get; set; } = "#7C4DFF";
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public double CompletionPercent => TotalTasks > 0 ? Math.Round((double)CompletedTasks / TotalTasks * 100, 1) : 0;
        public string Initials
        {
            get
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}";
                return parts.Length > 0 ? parts[0][0].ToString() : "?";
            }
        }
    }
}
