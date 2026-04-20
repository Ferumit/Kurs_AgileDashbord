using Microsoft.EntityFrameworkCore;
using Kurs_AgileDashbord.Models;

namespace Kurs_AgileDashbord.Data
{
    // Главный класс для работы с базой данных через Entity Framework.
    // Хранит все таблицы и описывает их структуру.
    public class AgileBoardContext : DbContext
    {
        // Строка подключения устанавливается один раз при старте приложения
        // через DatabaseInitializer, который сам находит подходящий SQL Server
        private static string? _connectionString;

        public static void SetConnectionString(string connStr) => _connectionString = connStr;

        public AgileBoardContext() { }

        public AgileBoardContext(DbContextOptions<AgileBoardContext> options) : base(options) { }

        // Таблицы базы данных
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TaskHistory> TaskHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Если строка подключения не задана явно — используем LocalDB по умолчанию
                var connStr = _connectionString
                    ?? @"Server=(localdb)\MSSQLLocalDB;Database=AgileBoardDB;Integrated Security=True;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connStr, options => options.EnableRetryOnFailure());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Пользователи — хранит данные аккаунтов, роли и пароли
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.Property(e => e.AvatarColor).HasMaxLength(7);
                entity.Property(e => e.PasswordHash).HasMaxLength(256);
                entity.Property(e => e.IsAdmin).HasDefaultValue(false);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            });

            // Проекты — верхний уровень иерархии, группирует спринты и задачи
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.HasKey(e => e.ProjectID);
                entity.Property(e => e.ProjectName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.ProjectCode).HasMaxLength(10).IsRequired();
            });

            // Спринты — итерации внутри проекта с датами начала и конца
            modelBuilder.Entity<Sprint>(entity =>
            {
                entity.ToTable("Sprints");
                entity.HasKey(e => e.SprintID);
                entity.Property(e => e.SprintName).HasMaxLength(100).IsRequired();

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Sprints)
                      .HasForeignKey(e => e.ProjectID);
            });

            // Задачи — основная сущность Kanban-доски
            // Связана с проектом, спринтом, автором и исполнителем
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("Tasks");
                entity.HasKey(e => e.TaskID);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Priority).HasMaxLength(20).IsRequired();

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Tasks)
                      .HasForeignKey(e => e.ProjectID);

                entity.HasOne(e => e.Sprint)
                      .WithMany(s => s.Tasks)
                      .HasForeignKey(e => e.SprintID)
                      .IsRequired(false);

                // Автор — кто создал задачу (при удалении пользователя задача остаётся)
                entity.HasOne(e => e.Author)
                      .WithMany(u => u.AuthoredTasks)
                      .HasForeignKey(e => e.AuthorID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Исполнитель — кто выполняет (может быть не назначен)
                entity.HasOne(e => e.Executor)
                      .WithMany(u => u.AssignedTasks)
                      .HasForeignKey(e => e.ExecutorID)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Комментарии к задачам — при удалении задачи комментарии тоже удаляются
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comments");
                entity.HasKey(e => e.CommentID);
                entity.Property(e => e.Text).IsRequired();

                entity.HasOne(e => e.Task)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(e => e.TaskID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // История изменений статусов — заполняется через триггер в БД
            modelBuilder.Entity<TaskHistory>(entity =>
            {
                entity.ToTable("TaskHistory");
                entity.HasKey(e => e.HistoryID);
                entity.Property(e => e.OldStatus).HasMaxLength(20).IsRequired();
                entity.Property(e => e.NewStatus).HasMaxLength(20).IsRequired();

                entity.HasOne(e => e.Task)
                      .WithMany(t => t.History)
                      .HasForeignKey(e => e.TaskID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ChangedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ChangedByUserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
