namespace Kurs_AgileDashbord.Models
{
    /// <summary>
    /// Комментарий к задаче
    /// </summary>
    public class Comment
    {
        public int CommentID { get; set; }
        public int TaskID { get; set; }
        public int UserID { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        public TaskItem Task { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
