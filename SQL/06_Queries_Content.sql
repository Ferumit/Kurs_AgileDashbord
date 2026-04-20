-- ============================================================
-- 06_Queries_Content.sql
-- Запросы для просмотра задач, комментариев и истории в SSMS
-- ============================================================

USE AgileBoardDB;
GO

-- 1. Все задачи с полной информацией
SELECT
    t.TaskID,
    p.ProjectName                           AS [Проект],
    s.SprintName                            AS [Спринт],
    t.Title                                 AS [Задача],
    t.Description                           AS [Описание],
    t.Priority                              AS [Приоритет],
    t.Status                                AS [Статус],
    author.FullName                         AS [Автор],
    executor.FullName                       AS [Исполнитель],
    (SELECT COUNT(*) FROM Comments c
     WHERE c.TaskID = t.TaskID)             AS [Комментариев],
    t.CreatedAt                             AS [Создана]
FROM Tasks t
JOIN Projects p ON p.ProjectID = t.ProjectID
LEFT JOIN Sprints s ON s.SprintID = t.SprintID
JOIN Users author ON author.UserID = t.AuthorID
LEFT JOIN Users executor ON executor.UserID = t.ExecutorID
ORDER BY p.ProjectName, t.Status, t.Priority DESC;
GO

-- 2. Итоговая сводка по проектам
SELECT
    p.ProjectName                                               AS [Проект],
    p.ProjectCode                                               AS [Код],
    COUNT(DISTINCT s.SprintID)                                  AS [Спринтов],
    COUNT(t.TaskID)                                             AS [Всего задач],
    SUM(CASE t.Status WHEN 'To Do'       THEN 1 ELSE 0 END)    AS [К выполнению],
    SUM(CASE t.Status WHEN 'In Progress' THEN 1 ELSE 0 END)    AS [В работе],
    SUM(CASE t.Status WHEN 'Review'      THEN 1 ELSE 0 END)    AS [На проверке],
    SUM(CASE t.Status WHEN 'Done'        THEN 1 ELSE 0 END)    AS [Готово],
    CASE
        WHEN COUNT(t.TaskID) = 0 THEN 0
        ELSE CAST(SUM(CASE t.Status WHEN 'Done' THEN 1 ELSE 0 END) * 100.0
             / COUNT(t.TaskID) AS DECIMAL(5,1))
    END                                                         AS [% готовности]
FROM Projects p
LEFT JOIN Sprints s ON s.ProjectID = p.ProjectID
LEFT JOIN Tasks t ON t.ProjectID = p.ProjectID
GROUP BY p.ProjectID, p.ProjectName, p.ProjectCode
ORDER BY [% готовности] DESC;
GO

-- 3. Все комментарии с привязкой к задачам
SELECT
    c.CommentID,
    p.ProjectName   AS [Проект],
    t.Title         AS [Задача],
    u.FullName      AS [Автор комментария],
    c.Text          AS [Текст],
    c.CreatedAt     AS [Дата]
FROM Comments c
JOIN Tasks t ON t.TaskID = c.TaskID
JOIN Projects p ON p.ProjectID = t.ProjectID
JOIN Users u ON u.UserID = c.UserID
ORDER BY c.CreatedAt DESC;
GO

-- 4. История изменений статусов задач
SELECT
    h.HistoryID,
    p.ProjectName   AS [Проект],
    t.Title         AS [Задача],
    u.FullName      AS [Кто изменил],
    h.OldStatus     AS [Был статус],
    h.NewStatus     AS [Стал статус],
    h.ChangedAt     AS [Когда]
FROM TaskHistory h
JOIN Tasks t ON t.TaskID = h.TaskID
JOIN Projects p ON p.ProjectID = t.ProjectID
JOIN Users u ON u.UserID = h.ChangedByUserID
ORDER BY h.ChangedAt DESC;
GO

-- 5. Активные спринты
SELECT
    p.ProjectName                                           AS [Проект],
    s.SprintName                                            AS [Спринт],
    s.StartDate                                             AS [Начало],
    s.EndDate                                               AS [Конец],
    DATEDIFF(DAY, GETDATE(), s.EndDate)                    AS [Дней до конца],
    COUNT(t.TaskID)                                         AS [Задач в спринте],
    SUM(CASE t.Status WHEN 'Done' THEN 1 ELSE 0 END)       AS [Завершено]
FROM Sprints s
JOIN Projects p ON p.ProjectID = s.ProjectID
LEFT JOIN Tasks t ON t.SprintID = s.SprintID
WHERE s.IsActive = 1
GROUP BY s.SprintID, p.ProjectName, s.SprintName, s.StartDate, s.EndDate
ORDER BY s.EndDate;
GO
