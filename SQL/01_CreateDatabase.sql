-- ============================================
-- Курсовая работа: Agile-доска
-- Скрипт 1: Создание базы данных и таблиц
-- ============================================

-- Создаём базу данных
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AgileBoardDB')
BEGIN
    CREATE DATABASE AgileBoardDB;
END
GO

USE AgileBoardDB;
GO

-- ============================================
-- Таблица: Users (Пользователи)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID      INT PRIMARY KEY IDENTITY(1,1),
        FullName    NVARCHAR(100) NOT NULL,
        Email       NVARCHAR(150) NOT NULL UNIQUE,
        [Role]      NVARCHAR(50)  NOT NULL DEFAULT N'Developer'
            CHECK ([Role] IN (N'Developer', N'QA', N'PM', N'Designer', N'Team Lead')),
        AvatarColor NVARCHAR(7)   DEFAULT N'#7C4DFF',  -- цвет для аватарки в UI
        CreatedAt   DATETIME      DEFAULT GETDATE()
    );
END
GO

-- ============================================
-- Таблица: Projects (Проекты)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Projects')
BEGIN
    CREATE TABLE Projects (
        ProjectID    INT PRIMARY KEY IDENTITY(1,1),
        ProjectName  NVARCHAR(150) NOT NULL,
        [Description] NVARCHAR(MAX),
        ProjectCode  NVARCHAR(10) NOT NULL UNIQUE,  -- короткий код: "WEB", "MOB"
        CreatedAt    DATETIME DEFAULT GETDATE()
    );
END
GO

-- ============================================
-- Таблица: Sprints (Спринты)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sprints')
BEGIN
    CREATE TABLE Sprints (
        SprintID    INT PRIMARY KEY IDENTITY(1,1),
        ProjectID   INT NOT NULL,
        SprintName  NVARCHAR(100) NOT NULL,
        StartDate   DATE NOT NULL,
        EndDate     DATE NOT NULL,
        IsActive    BIT DEFAULT 1,
        CONSTRAINT FK_Sprints_Projects FOREIGN KEY (ProjectID) REFERENCES Projects(ProjectID),
        CONSTRAINT CK_Sprints_Dates CHECK (EndDate >= StartDate)
    );
END
GO

-- ============================================
-- Таблица: Tasks (Задачи) — основная сущность
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE Tasks (
        TaskID       INT PRIMARY KEY IDENTITY(1,1),
        ProjectID    INT NOT NULL,
        SprintID     INT NULL,           -- NULL = бэклог (не назначен спринту)
        AuthorID     INT NOT NULL,
        ExecutorID   INT NULL,           -- NULL = не назначен
        Title        NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX),
        [Status]     NVARCHAR(20) NOT NULL DEFAULT N'To Do'
            CHECK ([Status] IN (N'To Do', N'In Progress', N'Review', N'Done')),
        [Priority]   NVARCHAR(20) NOT NULL DEFAULT N'Medium'
            CHECK ([Priority] IN (N'Low', N'Medium', N'High', N'Critical')),
        CreatedAt    DATETIME DEFAULT GETDATE(),
        CompletedAt  DATETIME NULL,

        CONSTRAINT FK_Tasks_Projects FOREIGN KEY (ProjectID)  REFERENCES Projects(ProjectID),
        CONSTRAINT FK_Tasks_Sprints  FOREIGN KEY (SprintID)   REFERENCES Sprints(SprintID),
        CONSTRAINT FK_Tasks_Author   FOREIGN KEY (AuthorID)   REFERENCES Users(UserID),
        CONSTRAINT FK_Tasks_Executor FOREIGN KEY (ExecutorID)  REFERENCES Users(UserID)
    );
END
GO

-- ============================================
-- Таблица: Comments (Комментарии к задачам)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments')
BEGIN
    CREATE TABLE Comments (
        CommentID   INT PRIMARY KEY IDENTITY(1,1),
        TaskID      INT NOT NULL,
        UserID      INT NOT NULL,
        [Text]      NVARCHAR(MAX) NOT NULL,
        CreatedAt   DATETIME DEFAULT GETDATE(),

        CONSTRAINT FK_Comments_Tasks FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE,
        CONSTRAINT FK_Comments_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- ============================================
-- Таблица: TaskHistory (История изменений статуса)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskHistory')
BEGIN
    CREATE TABLE TaskHistory (
        HistoryID       INT PRIMARY KEY IDENTITY(1,1),
        TaskID          INT NOT NULL,
        OldStatus       NVARCHAR(20) NOT NULL,
        NewStatus       NVARCHAR(20) NOT NULL,
        ChangedAt       DATETIME DEFAULT GETDATE(),
        ChangedByUserID INT NOT NULL,

        CONSTRAINT FK_TaskHistory_Tasks FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE,
        CONSTRAINT FK_TaskHistory_Users FOREIGN KEY (ChangedByUserID) REFERENCES Users(UserID)
    );
END
GO

-- ============================================
-- Индексы для быстрой фильтрации
-- ============================================
CREATE NONCLUSTERED INDEX IX_Tasks_Status     ON Tasks([Status]);
CREATE NONCLUSTERED INDEX IX_Tasks_ProjectID  ON Tasks(ProjectID);
CREATE NONCLUSTERED INDEX IX_Tasks_SprintID   ON Tasks(SprintID);
CREATE NONCLUSTERED INDEX IX_Tasks_ExecutorID ON Tasks(ExecutorID);
CREATE NONCLUSTERED INDEX IX_Tasks_Priority   ON Tasks([Priority]);
GO

PRINT N'✅ База данных AgileBoardDB и все таблицы успешно созданы!';
GO
