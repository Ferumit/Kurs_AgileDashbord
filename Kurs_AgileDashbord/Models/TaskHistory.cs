namespace Kurs_AgileDashbord.Models
{
    /// <summary>
    /// История изменений статуса задачи (заполняется триггером в БД)
    /// </summary>
    public class TaskHistory
    {
        public int HistoryID { get; set; }
        public int TaskID { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public int ChangedByUserID { get; set; }

        // Навигационные свойства
        public TaskItem Task { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
