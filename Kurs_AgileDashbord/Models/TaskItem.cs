namespace Kurs_AgileDashbord.Models
{
    /// <summary>
    /// Задача на Agile-доске — основная сущность приложения.
    /// Имя "TaskItem" вместо "Task" чтобы не конфликтовать с System.Threading.Tasks.Task.
    /// </summary>
    public class TaskItem
    {
        public int TaskID { get; set; }
        public int ProjectID { get; set; }
        public int? SprintID { get; set; }
        public int AuthorID { get; set; }
        public int? ExecutorID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "To Do";
        public string Priority { get; set; } = "Medium";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        // Навигационные свойства
        public Project Project { get; set; } = null!;
        public Sprint? Sprint { get; set; }
        public User Author { get; set; } = null!;
        public User? Executor { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();

        /// <summary>
        /// Локализованный статус для отображения в UI
        /// </summary>
        public string StatusDisplayName => Status switch
        {
            "To Do" => "К выполнению",
            "In Progress" => "В работе",
            "Review" => "На проверке",
            "Done" => "Готово",
            _ => Status
        };

        /// <summary>
        /// Локализованный приоритет для отображения в UI
        /// </summary>
        public string PriorityDisplayName => Priority switch
        {
            "Low" => "Низкий",
            "Medium" => "Средний",
            "High" => "Высокий",
            "Critical" => "Критический",
            _ => Priority
        };

        public override string ToString() => Title;
    }
}
