namespace Kurs_AgileDashbord.Models
{
    /// <summary>
    /// Спринт — итерация разработки (обычно 2 недели)
    /// </summary>
    public class Sprint
    {
        public int SprintID { get; set; }
        public int ProjectID { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public Project Project { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public override string ToString() => SprintName;
    }
}
