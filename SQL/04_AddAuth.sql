USE AgileBoardDB;
GO

-- Добавляем поля авторизации
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Users ADD PasswordHash NVARCHAR(256) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsAdmin')
BEGIN
    ALTER TABLE Users ADD IsAdmin BIT DEFAULT 0;
END
GO

-- Устанавливаем пароли: Иванов Алексей (Team Lead) = администратор
UPDATE Users SET PasswordHash = N'admin123', IsAdmin = 1 WHERE UserID = 1;
UPDATE Users SET PasswordHash = N'user123', IsAdmin = 0 WHERE UserID = 2;
UPDATE Users SET PasswordHash = N'user123', IsAdmin = 0 WHERE UserID = 3;
UPDATE Users SET PasswordHash = N'user123', IsAdmin = 0 WHERE UserID = 4;
UPDATE Users SET PasswordHash = N'user123', IsAdmin = 0 WHERE UserID = 5;
UPDATE Users SET PasswordHash = N'user123', IsAdmin = 0 WHERE UserID = 6;
UPDATE Users SET PasswordHash = N'user123', IsAdmin = 0 WHERE UserID = 7;
GO

PRINT N'✅ Поля авторизации добавлены!';
GO
