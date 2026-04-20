namespace Kurs_AgileDashbord.Models
{
    /// <summary>
    /// Пользователь системы (разработчик, QA, PM и т.д.)
    /// </summary>
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Developer";
        public string AvatarColor { get; set; } = "#7C4DFF";
        public string? PasswordHash { get; set; }
        public bool IsAdmin { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        public ICollection<TaskItem> AuthoredTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>
        /// Инициалы для аватарки (например, "ИА" для "Иванов Алексей")
        /// </summary>
        public string Initials
        {
            get
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}";
                return parts.Length > 0 ? parts[0][0].ToString() : "?";
            }
        }

        public override string ToString() => FullName;
    }
}
