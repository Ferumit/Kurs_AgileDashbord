using Microsoft.EntityFrameworkCore;
using Kurs_AgileDashbord.Models;

namespace Kurs_AgileDashbord.Data
{
    /// <summary>
    /// Контекст базы данных Entity Framework Core.
    /// Подключение к MS SQL Server (AgileBoardDB).
    /// </summary>
    public class AgileBoardContext : DbContext
    {
        private static string? _connectionString;

        /// <summary>
        /// Установить строку подключения (вызывается из DatabaseInitializer при старте app)
        /// </summary>
        public static void SetConnectionString(string connStr) => _connectionString = connStr;

        public AgileBoardContext() { }

        public AgileBoardContext(DbContextOptions<AgileBoardContext> options) : base(options) { }
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
                var connStr = _connectionString
                    ?? @"Server=(localdb)\MSSQLLocalDB;Database=AgileBoardDB;Integrated Security=True;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connStr, options => options.EnableRetryOnFailure());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === Users ===
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
            });

            // === Projects ===
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.HasKey(e => e.ProjectID);
                entity.Property(e => e.ProjectName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.ProjectCode).HasMaxLength(10).IsRequired();
            });

            // === Sprints ===
            modelBuilder.Entity<Sprint>(entity =>
            {
                entity.ToTable("Sprints");
                entity.HasKey(e => e.SprintID);
                entity.Property(e => e.SprintName).HasMaxLength(100).IsRequired();

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Sprints)
                      .HasForeignKey(e => e.ProjectID);
            });

            // === Tasks (TaskItem) ===
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

                entity.HasOne(e => e.Author)
                      .WithMany(u => u.AuthoredTasks)
                      .HasForeignKey(e => e.AuthorID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Executor)
                      .WithMany(u => u.AssignedTasks)
                      .HasForeignKey(e => e.ExecutorID)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === Comments ===
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

            // === TaskHistory ===
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
