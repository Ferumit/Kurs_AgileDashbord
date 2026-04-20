-- ============================================
-- Курсовая работа: Agile-доска
-- Скрипт 2: Views, Triggers, Stored Procedures
-- ============================================

USE AgileBoardDB;
GO

-- ============================================
-- VIEW: vw_KanbanBoard
-- Канбан-доска: задачи + исполнитель + проект + спринт
-- ============================================
IF OBJECT_ID('vw_KanbanBoard', 'V') IS NOT NULL DROP VIEW vw_KanbanBoard;
GO

CREATE VIEW vw_KanbanBoard AS
SELECT 
    t.TaskID,
    t.Title,
    t.[Description],
    t.[Status],
    t.[Priority],
    t.CreatedAt,
    t.CompletedAt,
    p.ProjectName,
    p.ProjectCode,
    s.SprintName,
    s.IsActive AS SprintIsActive,
    author.FullName  AS AuthorName,
    executor.FullName AS ExecutorName,
    executor.AvatarColor AS ExecutorAvatarColor
FROM Tasks t
    INNER JOIN Projects p   ON t.ProjectID = p.ProjectID
    LEFT JOIN  Sprints s    ON t.SprintID  = s.SprintID
    INNER JOIN Users author ON t.AuthorID  = author.UserID
    LEFT JOIN  Users executor ON t.ExecutorID = executor.UserID;
GO

-- ============================================
-- VIEW: vw_SprintReport
-- Отчёт: количество задач по статусам в каждом спринте
-- ============================================
IF OBJECT_ID('vw_SprintReport', 'V') IS NOT NULL DROP VIEW vw_SprintReport;
GO

CREATE VIEW vw_SprintReport AS
SELECT 
    p.ProjectName,
    s.SprintName,
    s.StartDate,
    s.EndDate,
    s.IsActive,
    COUNT(t.TaskID) AS TotalTasks,
    SUM(CASE WHEN t.[Status] = N'To Do'       THEN 1 ELSE 0 END) AS ToDo,
    SUM(CASE WHEN t.[Status] = N'In Progress'  THEN 1 ELSE 0 END) AS InProgress,
    SUM(CASE WHEN t.[Status] = N'Review'       THEN 1 ELSE 0 END) AS InReview,
    SUM(CASE WHEN t.[Status] = N'Done'         THEN 1 ELSE 0 END) AS Done,
    -- Процент завершения спринта
    CAST(
        CASE 
            WHEN COUNT(t.TaskID) = 0 THEN 0
            ELSE (SUM(CASE WHEN t.[Status] = N'Done' THEN 1 ELSE 0 END) * 100.0 / COUNT(t.TaskID))
        END AS DECIMAL(5,1)
    ) AS CompletionPercent
FROM Sprints s
    INNER JOIN Projects p ON s.ProjectID = p.ProjectID
    LEFT JOIN Tasks t     ON t.SprintID  = s.SprintID
GROUP BY 
    p.ProjectName, s.SprintName, s.StartDate, s.EndDate, s.IsActive;
GO

-- ============================================
-- TRIGGER: trg_TaskStatusChange
-- При изменении статуса:
--   1) Записывает в TaskHistory
--   2) Если новый статус = 'Done', проставляет CompletedAt
--   3) Если статус меняется с 'Done' на другой, очищает CompletedAt
-- ============================================
IF OBJECT_ID('trg_TaskStatusChange', 'TR') IS NOT NULL DROP TRIGGER trg_TaskStatusChange;
GO

CREATE TRIGGER trg_TaskStatusChange
ON Tasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Записываем историю только если статус изменился
    INSERT INTO TaskHistory (TaskID, OldStatus, NewStatus, ChangedByUserID)
    SELECT 
        i.TaskID,
        d.[Status],
        i.[Status],
        i.ExecutorID  -- фиксируем того, кто сейчас назначен исполнителем
    FROM inserted i
    INNER JOIN deleted d ON i.TaskID = d.TaskID
    WHERE i.[Status] <> d.[Status]
      AND i.ExecutorID IS NOT NULL;

    -- Проставляем дату завершения при переходе в 'Done'
    UPDATE t
    SET t.CompletedAt = GETDATE()
    FROM Tasks t
    INNER JOIN inserted i ON t.TaskID = i.TaskID
    INNER JOIN deleted d  ON t.TaskID = d.TaskID
    WHERE i.[Status] = N'Done' AND d.[Status] <> N'Done';

    -- Очищаем дату завершения при переходе из 'Done' обратно
    UPDATE t
    SET t.CompletedAt = NULL
    FROM Tasks t
    INNER JOIN inserted i ON t.TaskID = i.TaskID
    INNER JOIN deleted d  ON t.TaskID = d.TaskID
    WHERE d.[Status] = N'Done' AND i.[Status] <> N'Done';
END
GO

-- ============================================
-- STORED PROCEDURE: sp_CreateTaskWithValidation
-- Создаёт задачу с проверками
-- ============================================
IF OBJECT_ID('sp_CreateTaskWithValidation', 'P') IS NOT NULL DROP PROCEDURE sp_CreateTaskWithValidation;
GO

CREATE PROCEDURE sp_CreateTaskWithValidation
    @Title       NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @ProjectID   INT,
    @SprintID    INT = NULL,
    @AuthorID    INT,
    @ExecutorID  INT = NULL,
    @Priority    NVARCHAR(20) = N'Medium',
    @NewTaskID   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Проверка: существует ли проект
    IF NOT EXISTS (SELECT 1 FROM Projects WHERE ProjectID = @ProjectID)
    BEGIN
        RAISERROR(N'Ошибка: Проект с ID=%d не найден.', 16, 1, @ProjectID);
        RETURN;
    END

    -- Проверка: существует ли автор
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @AuthorID)
    BEGIN
        RAISERROR(N'Ошибка: Пользователь-автор с ID=%d не найден.', 16, 1, @AuthorID);
        RETURN;
    END

    -- Проверка: существует ли исполнитель (если указан)
    IF @ExecutorID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @ExecutorID)
    BEGIN
        RAISERROR(N'Ошибка: Исполнитель с ID=%d не найден.', 16, 1, @ExecutorID);
        RETURN;
    END

    -- Проверка: существует ли спринт (если указан) и принадлежит ли он проекту
    IF @SprintID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Sprints WHERE SprintID = @SprintID AND ProjectID = @ProjectID)
        BEGIN
            RAISERROR(N'Ошибка: Спринт с ID=%d не найден или не принадлежит проекту.', 16, 1, @SprintID);
            RETURN;
        END
    END

    -- Проверка: валидность приоритета
    IF @Priority NOT IN (N'Low', N'Medium', N'High', N'Critical')
    BEGIN
        RAISERROR(N'Ошибка: Недопустимый приоритет "%s".', 16, 1, @Priority);
        RETURN;
    END

    -- Вставка задачи
    INSERT INTO Tasks (Title, [Description], ProjectID, SprintID, AuthorID, ExecutorID, [Priority])
    VALUES (@Title, @Description, @ProjectID, @SprintID, @AuthorID, @ExecutorID, @Priority);

    SET @NewTaskID = SCOPE_IDENTITY();

    PRINT N'✅ Задача "' + @Title + N'" успешно создана с ID=' + CAST(@NewTaskID AS NVARCHAR(10));
END
GO

-- ============================================
-- STORED PROCEDURE: sp_CloseSprintAndMoveBacklog
-- Закрывает спринт и переносит незавершённые задачи
-- ============================================
IF OBJECT_ID('sp_CloseSprintAndMoveBacklog', 'P') IS NOT NULL DROP PROCEDURE sp_CloseSprintAndMoveBacklog;
GO

CREATE PROCEDURE sp_CloseSprintAndMoveBacklog
    @SprintID      INT,
    @NextSprintID  INT = NULL   -- NULL = перенести в бэклог
AS
BEGIN
    SET NOCOUNT ON;

    -- Проверка: существует ли спринт
    IF NOT EXISTS (SELECT 1 FROM Sprints WHERE SprintID = @SprintID)
    BEGIN
        RAISERROR(N'Ошибка: Спринт с ID=%d не найден.', 16, 1, @SprintID);
        RETURN;
    END

    -- Проверка: если указан следующий спринт, существует ли он
    IF @NextSprintID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Sprints WHERE SprintID = @NextSprintID)
    BEGIN
        RAISERROR(N'Ошибка: Следующий спринт с ID=%d не найден.', 16, 1, @NextSprintID);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Подсчитываем незавершённые задачи
        DECLARE @UnfinishedCount INT;
        SELECT @UnfinishedCount = COUNT(*) 
        FROM Tasks 
        WHERE SprintID = @SprintID AND [Status] <> N'Done';

        -- Переносим незавершённые задачи
        UPDATE Tasks
        SET SprintID = @NextSprintID  -- NULL = бэклог
        WHERE SprintID = @SprintID AND [Status] <> N'Done';

        -- Деактивируем спринт
        UPDATE Sprints
        SET IsActive = 0
        WHERE SprintID = @SprintID;

        COMMIT TRANSACTION;

        IF @NextSprintID IS NOT NULL
            PRINT N'✅ Спринт закрыт. Перенесено задач: ' + CAST(@UnfinishedCount AS NVARCHAR(10)) + N' в спринт ID=' + CAST(@NextSprintID AS NVARCHAR(10));
        ELSE
            PRINT N'✅ Спринт закрыт. Перенесено задач в бэклог: ' + CAST(@UnfinishedCount AS NVARCHAR(10));

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT N'✅ Views, Triggers и Stored Procedures успешно созданы!';
GO
