-- ============================================
-- Курсовая работа: Agile-доска
-- Скрипт 3: Тестовые данные (DML) — исправленный
-- ============================================

USE AgileBoardDB;
GO

-- Устанавливаем формат дат для совместимости
SET DATEFORMAT ymd;
GO

-- Очищаем существующие данные (если есть)
DELETE FROM Comments;
DELETE FROM TaskHistory;
DELETE FROM Tasks;
DELETE FROM Sprints;
DELETE FROM Projects;
DELETE FROM Users;
GO

-- ============================================
-- Пользователи (7 человек)
-- ============================================
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserID, FullName, Email, [Role], AvatarColor) VALUES
(1, N'Иванов Алексей',     N'ivanov@company.kz',    N'Team Lead',  N'#7C4DFF'),
(2, N'Петрова Мария',      N'petrova@company.kz',   N'Developer',  N'#00BCD4'),
(3, N'Сидоров Дмитрий',    N'sidorov@company.kz',   N'Developer',  N'#FF5722'),
(4, N'Касымова Айгерим',   N'kasymova@company.kz',  N'QA',         N'#4CAF50'),
(5, N'Нурланов Ерболат',   N'nurlanov@company.kz',  N'Developer',  N'#FF9800'),
(6, N'Волкова Елена',      N'volkova@company.kz',    N'Designer',   N'#E91E63'),
(7, N'Ахметов Тимур',      N'akhmetov@company.kz',  N'PM',         N'#3F51B5');
SET IDENTITY_INSERT Users OFF;
GO

-- ============================================
-- Проекты (3 проекта)
-- ============================================
SET IDENTITY_INSERT Projects ON;
INSERT INTO Projects (ProjectID, ProjectName, [Description], ProjectCode) VALUES
(1, N'Веб-портал компании',     N'Разработка корпоративного веб-портала для внутренних нужд',     N'WEB'),
(2, N'Мобильное приложение',     N'Кроссплатформенное приложение для клиентов',                   N'MOB'),
(3, N'Система аналитики',        N'Дашборд для анализа ключевых метрик бизнеса',                  N'ANL');
SET IDENTITY_INSERT Projects OFF;
GO

-- ============================================
-- Спринты (5 спринтов)
-- ============================================
SET IDENTITY_INSERT Sprints ON;
INSERT INTO Sprints (SprintID, ProjectID, SprintName, StartDate, EndDate, IsActive) VALUES
(1, 1, N'Спринт 1 - Основа',           CAST('2026-03-01' AS DATE), CAST('2026-03-14' AS DATE), 0),
(2, 1, N'Спринт 2 - Функционал',       CAST('2026-03-15' AS DATE), CAST('2026-03-28' AS DATE), 0),
(3, 1, N'Спринт 3 - Тестирование',     CAST('2026-03-29' AS DATE), CAST('2026-04-11' AS DATE), 1),
(4, 2, N'Спринт 1 - MVP',              CAST('2026-03-10' AS DATE), CAST('2026-03-24' AS DATE), 0),
(5, 2, N'Спринт 2 - Улучшения',        CAST('2026-03-25' AS DATE), CAST('2026-04-07' AS DATE), 1);
SET IDENTITY_INSERT Sprints OFF;
GO

-- ============================================
-- Задачи (25 задач)
-- Триггер отключаем чтобы не засорять историю при загрузке
-- ============================================
DISABLE TRIGGER trg_TaskStatusChange ON Tasks;
GO

SET IDENTITY_INSERT Tasks ON;

-- Проект WEB, Спринт 1 (завершён)
INSERT INTO Tasks (TaskID, ProjectID, SprintID, AuthorID, ExecutorID, Title, [Description], [Status], [Priority], CreatedAt, CompletedAt) VALUES
(1,  1, 1, 7, 2, N'Настройка CI/CD пайплайна',           N'Настроить GitHub Actions для автоматической сборки и деплоя',    N'Done',        N'High',     CAST('2026-03-01' AS DATETIME), CAST('2026-03-05' AS DATETIME)),
(2,  1, 1, 7, 3, N'Создание макета главной страницы',     N'Разработать структуру лендинга с адаптивной версткой',           N'Done',        N'High',     CAST('2026-03-01' AS DATETIME), CAST('2026-03-07' AS DATETIME)),
(3,  1, 1, 1, 5, N'Настройка базы данных PostgreSQL',     N'Развернуть сервер БД и настроить миграции',                      N'Done',        N'Critical', CAST('2026-03-02' AS DATETIME), CAST('2026-03-06' AS DATETIME)),
(4,  1, 1, 1, 4, N'Написание тест-кейсов',               N'Покрыть основные пользовательские сценарии тестами',             N'Done',        N'Medium',   CAST('2026-03-03' AS DATETIME), CAST('2026-03-13' AS DATETIME));

-- Проект WEB, Спринт 2 (завершён)
INSERT INTO Tasks (TaskID, ProjectID, SprintID, AuthorID, ExecutorID, Title, [Description], [Status], [Priority], CreatedAt, CompletedAt) VALUES
(5,  1, 2, 7, 2, N'Авторизация через OAuth 2.0',         N'Реализовать вход через Google и Яндекс',                        N'Done',        N'Critical', CAST('2026-03-15' AS DATETIME), CAST('2026-03-22' AS DATETIME)),
(6,  1, 2, 1, 3, N'REST API для профиля',                 N'Эндпоинты CRUD для управления пользователями',                  N'Done',        N'High',     CAST('2026-03-15' AS DATETIME), CAST('2026-03-20' AS DATETIME)),
(7,  1, 2, 7, 6, N'Дизайн страницы настроек',            N'UI/UX для страницы профиля и настроек уведомлений',              N'Done',        N'Medium',   CAST('2026-03-16' AS DATETIME), CAST('2026-03-25' AS DATETIME)),
(8,  1, 2, 1, 5, N'Оптимизация SQL-запросов',             N'Ускорить тяжёлые запросы на главной странице',                   N'Done',        N'High',     CAST('2026-03-17' AS DATETIME), CAST('2026-03-26' AS DATETIME));

-- Проект WEB, Спринт 3 (активный)
INSERT INTO Tasks (TaskID, ProjectID, SprintID, AuthorID, ExecutorID, Title, [Description], [Status], [Priority], CreatedAt, CompletedAt) VALUES
(9,  1, 3, 7, 2, N'Система уведомлений',                 N'Push-уведомления и email-рассылка по событиям',                N'In Progress', N'High',     CAST('2026-03-29' AS DATETIME), NULL),
(10, 1, 3, 1, 3, N'Интеграция с Telegram Bot',           N'Создать бота для отправки уведомлений в рабочий чат',           N'To Do',       N'Medium',   CAST('2026-03-29' AS DATETIME), NULL),
(11, 1, 3, 7, 4, N'Регрессионное тестирование',           N'Полный цикл тестов перед релизом v2.0',                        N'Review',      N'Critical', CAST('2026-03-30' AS DATETIME), NULL),
(12, 1, 3, 1, 5, N'Документация API (Swagger)',           N'Настроить автогенерацию документации Swagger',                 N'In Progress', N'Medium',   CAST('2026-03-30' AS DATETIME), NULL),
(13, 1, 3, 7, 6, N'Редизайн формы обратной связи',       N'Обновить UI формы и добавить валидацию полей',                  N'To Do',       N'Low',      CAST('2026-04-01' AS DATETIME), NULL),
(14, 1, 3, 1, 2, N'Кэширование Redis',                   N'Внедрить Redis для кэша сессий и частых запросов',              N'To Do',       N'High',     CAST('2026-04-02' AS DATETIME), NULL);

-- Проект WEB, Бэклог (без спринта)
INSERT INTO Tasks (TaskID, ProjectID, SprintID, AuthorID, ExecutorID, Title, [Description], [Status], [Priority], CreatedAt) VALUES
(15, 1, NULL, 7, NULL, N'Dark mode для портала',              N'Реализовать переключение между светлой и тёмной темой',    N'To Do',  N'Low',    CAST('2026-04-05' AS DATETIME)),
(16, 1, NULL, 1, NULL, N'Экспорт отчётов в PDF',              N'Формирование красиво оформленных отчётов в PDF',           N'To Do',  N'Medium', CAST('2026-04-06' AS DATETIME));

-- Проект MOB, Спринт 1 (завершён)
INSERT INTO Tasks (TaskID, ProjectID, SprintID, AuthorID, ExecutorID, Title, [Description], [Status], [Priority], CreatedAt, CompletedAt) VALUES
(17, 2, 4, 7, 5, N'Настройка Flutter-проекта',            N'Инициализация проекта и настройка архитектуры приложения',     N'Done',        N'Critical', CAST('2026-03-10' AS DATETIME), CAST('2026-03-14' AS DATETIME)),
(18, 2, 4, 7, 6, N'Дизайн экранов авторизации',           N'Figma-макеты для Login/Register/Forgot password',             N'Done',        N'High',     CAST('2026-03-10' AS DATETIME), CAST('2026-03-17' AS DATETIME)),
(19, 2, 4, 1, 5, N'Экран списка товаров',                  N'Верстка каталога с фильтрацией и поиском',                   N'Done',        N'High',     CAST('2026-03-12' AS DATETIME), CAST('2026-03-22' AS DATETIME));

-- Проект MOB, Спринт 2 (активный)
INSERT INTO Tasks (TaskID, ProjectID, SprintID, AuthorID, ExecutorID, Title, [Description], [Status], [Priority], CreatedAt) VALUES
(20, 2, 5, 7, 5, N'Оффлайн-режим',                       N'Кэширование данных для работы без интернета',                  N'In Progress', N'High',     CAST('2026-03-25' AS DATETIME)),
(21, 2, 5, 1, 3, N'Push-уведомления (Firebase)',          N'Интеграция FCM для мобильных уведомлений',                     N'To Do',       N'Medium',   CAST('2026-03-26' AS DATETIME)),
(22, 2, 5, 7, 4, N'Тестирование на Android 14+',          N'Проверка совместимости на новых версиях Android',              N'Review',      N'High',     CAST('2026-03-27' AS DATETIME)),
(23, 2, 5, 1, 6, N'Анимации переходов',                   N'Плавные переходы между экранами (Hero animations)',            N'In Progress', N'Low',      CAST('2026-03-28' AS DATETIME));

-- Проект ANL, Бэклог
INSERT INTO Tasks (TaskID, ProjectID, SprintID, AuthorID, ExecutorID, Title, [Description], [Status], [Priority], CreatedAt) VALUES
(24, 3, NULL, 7, NULL, N'Прототип дашборда аналитики',     N'Прототип основного экрана с графиками и KPI',                 N'To Do',  N'High',   CAST('2026-04-10' AS DATETIME)),
(25, 3, NULL, 1, NULL, N'Интеграция с API маркетплейсов',  N'Подключение Kaspi/Wildberries для сбора данных',              N'To Do',  N'Medium', CAST('2026-04-12' AS DATETIME));

SET IDENTITY_INSERT Tasks OFF;
GO

-- Включаем триггер обратно
ENABLE TRIGGER trg_TaskStatusChange ON Tasks;
GO

-- ============================================
-- Комментарии (15 комментариев)
-- ============================================
INSERT INTO Comments (TaskID, UserID, [Text], CreatedAt) VALUES
(1,  2, N'Пайплайн настроен. Сборка на каждый push в main.',                          CAST('2026-03-04' AS DATETIME)),
(1,  7, N'Отлично! Добавь ещё деплой на staging при PR.',                               CAST('2026-03-04' AS DATETIME)),
(5,  2, N'OAuth работает стабильно. Google прошёл ревью.',                              CAST('2026-03-20' AS DATETIME)),
(5,  7, N'Яндекс ещё на тесте, но Google уже можно мержить.',                          CAST('2026-03-21' AS DATETIME)),
(9,  2, N'Начал работу. Использую SignalR для real-time.',                              CAST('2026-03-30' AS DATETIME)),
(9,  1, N'Хорошо. Не забудь про fallback на polling для старых браузеров.',             CAST('2026-03-31' AS DATETIME)),
(11, 4, N'Нашла 3 бага в модуле авторизации. Заведу отдельные задачи.',                 CAST('2026-04-02' AS DATETIME)),
(11, 7, N'Критичные? Нужно понять, блокируют ли они релиз.',                            CAST('2026-04-02' AS DATETIME)),
(11, 4, N'Один критичный (сброс пароля не работает), два минорных.',                    CAST('2026-04-03' AS DATETIME)),
(17, 5, N'Flutter 3.25 установлен. Архитектура Clean Architecture + BLoC.',             CAST('2026-03-12' AS DATETIME)),
(20, 5, N'Использую Hive для локального кэша. Работает быстро.',                        CAST('2026-03-27' AS DATETIME)),
(20, 7, N'А что с синхронизацией при возврате онлайн?',                                  CAST('2026-03-28' AS DATETIME)),
(20, 5, N'Реализую очередь операций. При подключении — автосинхр.',                     CAST('2026-03-28' AS DATETIME)),
(22, 4, N'На Pixel 8 всё ок. Samsung S24 — мелкие проблемы с версткой.',               CAST('2026-04-01' AS DATETIME)),
(14, 1, N'Нужно использовать Redis 7.x. Старые версии не поддерживают streams.',        CAST('2026-04-03' AS DATETIME));
GO

PRINT N'✅ Тестовые данные успешно загружены!';
GO
