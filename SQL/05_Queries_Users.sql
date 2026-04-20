-- ============================================================
-- 05_Queries_Users.sql
-- Запросы для просмотра данных о пользователях в SSMS
-- ============================================================

USE AgileBoardDB;
GO

-- 1. Все пользователи с полной информацией
SELECT
    u.UserID,
    u.FullName          AS [Полное имя],
    u.Email,
    u.Role              AS [Роль],
    u.Status            AS [Статус аккаунта],
    CASE u.IsAdmin WHEN 1 THEN 'Да' ELSE 'Нет' END AS [Администратор],
    u.AvatarColor       AS [Цвет аватара],
    u.CreatedAt         AS [Дата регистрации]
FROM Users u
ORDER BY u.IsAdmin DESC, u.FullName;
GO

-- 2. Статистика задач по каждому пользователю
SELECT
    u.FullName                                              AS [Пользователь],
    u.Role                                                  AS [Роль],
    COUNT(t.TaskID)                                         AS [Всего задач],
    SUM(CASE t.Status WHEN 'To Do'       THEN 1 ELSE 0 END) AS [К выполнению],
    SUM(CASE t.Status WHEN 'In Progress' THEN 1 ELSE 0 END) AS [В работе],
    SUM(CASE t.Status WHEN 'Review'      THEN 1 ELSE 0 END) AS [На проверке],
    SUM(CASE t.Status WHEN 'Done'        THEN 1 ELSE 0 END) AS [Готово],
    CASE
        WHEN COUNT(t.TaskID) = 0 THEN 0
        ELSE CAST(SUM(CASE t.Status WHEN 'Done' THEN 1 ELSE 0 END) * 100.0
             / COUNT(t.TaskID) AS DECIMAL(5,1))
    END                                                     AS [% выполнения]
FROM Users u
LEFT JOIN Tasks t ON t.ExecutorID = u.UserID
WHERE u.Status = 'Active'
GROUP BY u.UserID, u.FullName, u.Role
ORDER BY [% выполнения] DESC, [Всего задач] DESC;
GO

-- 3. Ожидающие подтверждения заявки на регистрацию
SELECT
    u.UserID,
    u.FullName  AS [Имя],
    u.Email,
    u.Role      AS [Запрошенная роль],
    u.CreatedAt AS [Дата заявки]
FROM Users u
WHERE u.Status = 'Pending'
ORDER BY u.CreatedAt;
GO

-- 4. Список администраторов
SELECT
    u.FullName  AS [Администратор],
    u.Email,
    u.CreatedAt AS [В системе с]
FROM Users u
WHERE u.IsAdmin = 1 AND u.Status = 'Active';
GO
