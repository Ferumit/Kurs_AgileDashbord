using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;


namespace Kurs_AgileDashbord.Data
{
    // Отвечает за первый запуск: находит SQL Server, создаёт базу данных и заполняет тестовыми данными.
    // При успешном подключении сохраняет адрес сервера в appsettings.json — следующий старт будет быстрее.
    public static class DatabaseInitializer
    {
        private const string DbName = "AgileBoardDB";
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        // Порядок поиска SQL Server: сначала LocalDB (есть на любом ПК с Visual Studio),
        // потом обычный localhost, потом Express-версия
        private static readonly string[] FallbackServers =
        [
            @"(localdb)\MSSQLLocalDB",
            @"localhost",
            @".\SQLEXPRESS",
        ];

        // Возвращает рабочую строку подключения.
        // Если база не существует — создаёт её и наполняет тестовыми данными.
        public static string InitializeAndGetConnectionString()
        {
            // Сначала пробуем сервер из appsettings.json (запомненный с прошлого запуска)
            var savedServer = ReadServerFromSettings();
            if (!string.IsNullOrWhiteSpace(savedServer))
            {
                var savedConn = BuildConnectionString(savedServer);
                if (TryInitialize(savedConn))
                    return savedConn;
                // Если сохранённый сервер вдруг перестал работать — идём дальше
            }

            // Перебираем стандартные варианты
            foreach (var server in FallbackServers)
            {
                var connStr = BuildConnectionString(server);
                if (TryInitialize(connStr))
                {
                    SaveServerToSettings(server); // запоминаем для следующего запуска
                    return connStr;
                }
            }

            throw new InvalidOperationException(
                "Не удалось подключиться к SQL Server.\n\n" +
                "Поддерживаемые варианты:\n" +
                "• SQL Server LocalDB — скачать: https://aka.ms/sqllocaldb\n" +
                "• SQL Server Express\n" +
                "• SQL Server (localhost)\n\n" +
                "Можно указать адрес сервера вручную в следующем окне.");
        }

        // Позволяет явно задать сервер (вызывается из диалога подключения)
        public static string InitializeWithServer(string server)
        {
            var connStr = BuildConnectionString(server);
            if (TryInitialize(connStr))
            {
                SaveServerToSettings(server);
                return connStr;
            }
            throw new InvalidOperationException($"Не удалось подключиться к серверу: {server}");
        }

        private static string BuildConnectionString(string server) =>
            $"Server={server};Database={DbName};Integrated Security=True;TrustServerCertificate=True;";

        private static bool TryInitialize(string connStr)
        {
            try
            {
                // Проверяем подключение к master и создаём БД если нужно
                var masterConn = connStr.Replace(DbName, "master");
                using var conn = new SqlConnection(masterConn);
                conn.Open();

                bool dbExists;
                using (var cmd = new SqlCommand(
                    $"SELECT COUNT(1) FROM sys.databases WHERE name = '{DbName}'", conn))
                    dbExists = (int)cmd.ExecuteScalar() > 0;

                if (!dbExists)
                {
                    using var cmd = new SqlCommand($"CREATE DATABASE [{DbName}]", conn);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();

                // Создаём таблицы и заполняем данными
                var opts = new DbContextOptionsBuilder<AgileBoardContext>()
                    .UseSqlServer(connStr, o => o.EnableRetryOnFailure(1))
                    .Options;
                using var db = new AgileBoardContext(opts);
                db.Database.EnsureCreated();

                if (!db.Users.Any())
                    SeedData(db);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string? ReadServerFromSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return null;
                var json = File.ReadAllText(SettingsPath);
                var doc = JsonDocument.Parse(json);
                var server = doc.RootElement
                    .GetProperty("Database")
                    .GetProperty("Server")
                    .GetString();
                return string.IsNullOrWhiteSpace(server) ? null : server;
            }
            catch { return null; }
        }

        private static void SaveServerToSettings(string server)
        {
            try
            {
                var settings = new { Database = new { Server = server, DatabaseName = DbName } };
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { /* не критично, просто не сохраним */ }
        }

        // Начальные данные: 7 пользователей, 3 проекта, 5 спринтов, 16 задач, 3 комментария
        private static void SeedData(AgileBoardContext db)
        {
            var users = new[]
            {
                new Models.User { FullName = "Иванов Алексей",   Email = "ivanov@company.kz",    Role = "Team Lead",   AvatarColor = "#7C4DFF", PasswordHash = "admin123", IsAdmin = true,  Status = "Active" },
                new Models.User { FullName = "Петрова Мария",    Email = "petrova@company.kz",   Role = "Developer",   AvatarColor = "#00BCD4", PasswordHash = "user123",  Status = "Active" },
                new Models.User { FullName = "Сидоров Дмитрий", Email = "sidorov@company.kz",   Role = "Developer",   AvatarColor = "#FF5722", PasswordHash = "user123",  Status = "Active" },
                new Models.User { FullName = "Волкова Елена",    Email = "volkova@company.kz",   Role = "QA Engineer", AvatarColor = "#4CAF50", PasswordHash = "user123",  Status = "Active" },
                new Models.User { FullName = "Нурланов Ерболат", Email = "nurlanov@company.kz",  Role = "Frontend Dev",AvatarColor = "#FF9800", PasswordHash = "user123",  Status = "Active" },
                new Models.User { FullName = "Касымова Айгерим", Email = "kassymova@company.kz", Role = "Backend Dev", AvatarColor = "#E91E63", PasswordHash = "user123",  Status = "Active" },
                new Models.User { FullName = "Жумабеков Асан",   Email = "zhumabekov@company.kz",Role = "DevOps",      AvatarColor = "#9C27B0", PasswordHash = "user123",  Status = "Active" },
            };
            db.Users.AddRange(users);
            db.SaveChanges();

            var projects = new[]
            {
                new Models.Project { ProjectName = "Веб-портал компании", ProjectCode = "WEB", Description = "Корпоративный портал" },
                new Models.Project { ProjectName = "Мобильное приложение", ProjectCode = "MOB", Description = "iOS/Android приложение" },
                new Models.Project { ProjectName = "API Gateway",          ProjectCode = "API", Description = "Микросервисный шлюз" },
            };
            db.Projects.AddRange(projects);
            db.SaveChanges();

            var sprints = new[]
            {
                new Models.Sprint { ProjectID = projects[0].ProjectID, SprintName = "Спринт 1 - Основа",       StartDate = DateTime.Now.AddDays(-60), EndDate = DateTime.Now.AddDays(-30), IsActive = false },
                new Models.Sprint { ProjectID = projects[0].ProjectID, SprintName = "Спринт 2 - Функционал",   StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now.AddDays(-1),  IsActive = false },
                new Models.Sprint { ProjectID = projects[0].ProjectID, SprintName = "Спринт 3 - Тестирование", StartDate = DateTime.Now,              EndDate = DateTime.Now.AddDays(14),  IsActive = true  },
                new Models.Sprint { ProjectID = projects[1].ProjectID, SprintName = "Спринт 1 - MVP",          StartDate = DateTime.Now.AddDays(-20), EndDate = DateTime.Now.AddDays(-5),  IsActive = false },
                new Models.Sprint { ProjectID = projects[1].ProjectID, SprintName = "Спринт 2 - Улучшения",    StartDate = DateTime.Now,              EndDate = DateTime.Now.AddDays(21),  IsActive = true  },
            };
            db.Sprints.AddRange(sprints);
            db.SaveChanges();

            var tasks = new Models.TaskItem[]
            {
                new() { Title = "Интеграция с Telegram Bot",          ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[2].UserID, Priority = "Medium",   Status = "To Do"      },
                new() { Title = "Система уведомлений email",           ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[1].UserID, Priority = "High",     Status = "In Progress" },
                new() { Title = "Регрессионное тестирование",          ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[5].UserID, Priority = "Critical", Status = "Review"     },
                new() { Title = "Настройка CI/CD пайплайна",           ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[1].UserID, Priority = "High",     Status = "Done"       },
                new() { Title = "Редизайн формы обратной связи",       ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[3].UserID, Priority = "Low",      Status = "To Do"      },
                new() { Title = "Документация API (Swagger)",          ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[4].UserID, Priority = "Medium",   Status = "In Progress" },
                new() { Title = "Кэширование Redis",                   ProjectID = projects[0].ProjectID, SprintID = sprints[1].SprintID, AuthorID = users[0].UserID, ExecutorID = users[1].UserID, Priority = "High",     Status = "Done"       },
                new() { Title = "Авторизация через OAuth 2.0",         ProjectID = projects[0].ProjectID, SprintID = sprints[1].SprintID, AuthorID = users[0].UserID, ExecutorID = users[1].UserID, Priority = "Critical", Status = "Done"       },
                new() { Title = "Написание тест-кейсов",               ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[5].UserID, Priority = "Medium",   Status = "Done"       },
                new() { Title = "REST API для профиля",                ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[2].UserID, Priority = "High",     Status = "Done"       },
                new() { Title = "Dark mode для портала",               ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = null,            Priority = "Low",      Status = "To Do"      },
                new() { Title = "Экспорт отчётов в PDF",               ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = null,            Priority = "Medium",   Status = "To Do"      },
                new() { Title = "Оптимизация SQL-запросов",            ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[4].UserID, Priority = "High",     Status = "Done"       },
                new() { Title = "Создание макета главной страницы",    ProjectID = projects[0].ProjectID, SprintID = sprints[0].SprintID, AuthorID = users[0].UserID, ExecutorID = users[2].UserID, Priority = "High",     Status = "Done"       },
                new() { Title = "Настройка базы данных PostgreSQL",    ProjectID = projects[0].ProjectID, SprintID = sprints[0].SprintID, AuthorID = users[0].UserID, ExecutorID = users[4].UserID, Priority = "Critical", Status = "Done"       },
                new() { Title = "Дизайн страницы настроек",            ProjectID = projects[0].ProjectID, SprintID = sprints[2].SprintID, AuthorID = users[0].UserID, ExecutorID = users[3].UserID, Priority = "Medium",   Status = "Done"       },
            };
            db.Tasks.AddRange(tasks);
            db.SaveChanges();

            var comments = new Models.Comment[]
            {
                new() { TaskID = tasks[0].TaskID, UserID = users[0].UserID, Text = "Нужен токен бота от @BotFather" },
                new() { TaskID = tasks[1].TaskID, UserID = users[1].UserID, Text = "Используем SMTP через Mailjet" },
                new() { TaskID = tasks[2].TaskID, UserID = users[3].UserID, Text = "Покрытие тестами — 85% готово" },
            };
            db.Comments.AddRange(comments);
            db.SaveChanges();
        }
    }
}
