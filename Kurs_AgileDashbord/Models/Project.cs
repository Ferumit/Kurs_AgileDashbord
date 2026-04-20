namespace Kurs_AgileDashbord.Models
{
    /// <summary>
    /// Проект (например, "Веб-портал компании")
    /// </summary>
    public class Project
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ProjectCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public override string ToString() => $"[{ProjectCode}] {ProjectName}";
    }
}
