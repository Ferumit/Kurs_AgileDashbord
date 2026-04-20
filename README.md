# AgileBoard

Десктопное приложение для управления задачами по методологии Agile/Scrum. Разработано в рамках курсовой работы.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/UI-WPF%20%2B%20Material%20Design-7C4DFF?style=flat-square)
![SQL Server](https://img.shields.io/badge/БД-MS%20SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver)

---

## Что умеет

- **Kanban-доска** с четырьмя колонками: К выполнению → В работе → На проверке → Готово
- **Авторизация** с ролями — администратор видит все задачи, обычный пользователь только свои
- **Фильтры** по проекту, спринту, исполнителю и текстовый поиск
- **Отчёты** — статистика по спринтам и продуктивность участников команды
- **Автоматическая БД** — при первом запуске сама находит SQL Server, создаёт базу и заполняет тестовыми данными

## Стек технологий

| | |
|---|---|
| Язык | C# 13 |
| Платформа | .NET 9.0 Windows |
| Интерфейс | WPF + Material Design 3 (тёмная тема) |
| Паттерн | MVVM (CommunityToolkit.Mvvm) |
| База данных | MS SQL Server / LocalDB |
| ORM | Entity Framework Core 9 |

## Установка и запуск

### Готовая сборка

1. Скачать архив из [Releases](https://github.com/Ferumit/Kurs_AgileDashbord/releases)
2. Установить **SQL Server LocalDB** если не установлен — [скачать](https://aka.ms/sqllocaldb) *(не нужно если установлена Visual Studio)*
3. Распаковать и запустить `Kurs_AgileDashbord.exe`

База данных создастся автоматически при первом запуске.

### Сборка из исходников

```bash
git clone https://github.com/Ferumit/Kurs_AgileDashbord.git
cd Kurs_AgileDashbord/Kurs_AgileDashbord
dotnet run
```

Требования: .NET 9.0 SDK + SQL Server или LocalDB.

## Тестовые аккаунты

| Роль | Email | Пароль |
|------|-------|--------|
| Администратор | ivanov@company.kz | admin123 |
| Пользователь | petrova@company.kz | user123 |
| Пользователь | sidorov@company.kz | user123 |

## Структура проекта

```
Kurs_AgileDashbord/
├── Data/
│   ├── AgileBoardContext.cs       # EF Core контекст — все таблицы и связи
│   └── DatabaseInitializer.cs    # Авто-поиск SQL Server, создание БД, seed данные
├── Models/                        # User, Project, Sprint, TaskItem, Comment, TaskHistory
├── ViewModels/
│   └── MainViewModel.cs          # Вся логика: фильтры, Kanban, отчёты
├── Views/
│   ├── LoginWindow.xaml          # Окно входа
│   ├── TaskDialog.xaml           # Создание задачи
│   └── TaskDetailDialog.xaml     # Просмотр задачи и комментарии
├── Converters/                    # Конвертеры для привязки данных в XAML
├── SQL/                           # SQL-скрипты для изучения схемы БД
└── MainWindow.xaml                # Главное окно: Kanban + Отчёты
```

## База данных

Схема в виде SQL-скриптов лежит в папке `SQL/`:

| Файл | Содержимое |
|------|-----------|
| `01_CreateDatabase.sql` | Схема: 6 таблиц с ключами и ограничениями |
| `02_ViewsTriggersProcs.sql` | 2 представления, 1 триггер, 2 хранимые процедуры |
| `03_TestData.sql` | Тестовые данные |
| `04_AddAuth.sql` | Добавление полей авторизации |

## Роли пользователей

**Администратор:**
- Видит задачи всех участников
- Создаёт и удаляет задачи
- Меняет статус любой задачи

**Пользователь:**
- Видит только задачи назначенные на него
- Может менять статус своих задач
