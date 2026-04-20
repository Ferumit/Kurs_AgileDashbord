# AgileBoard — Agile Task Management System

<div align="center">

![AgileBoard Logo](https://img.shields.io/badge/AgileBoard-v1.0-7C4DFF?style=for-the-badge&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Material%20Design-7C4DFF?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**Курсовая работа | Управление проектами по методологии Agile/Scrum**

[📥 Скачать приложение](#-установка) • [🚀 Быстрый старт](#-быстрый-старт) • [📖 Документация](#-архитектура)

</div>

---

## 📋 О проекте

**AgileBoard** — десктопное приложение для управления задачами по методологии Agile/Scrum с Kanban-доской. Разработано как курсовая работа по дисциплине «Базы данных и информационные системы».

### Возможности

| Функция | Описание |
|---------|----------|
| 🔐 **Авторизация** | Система аккаунтов с ролями (Администратор / Пользователь) |
| 📋 **Kanban-доска** | 4 колонки: К выполнению → В работе → На проверке → Готово |
| 🎯 **Задачи** | Создание, просмотр, перемещение по статусам, комментарии |
| 🔍 **Фильтры** | По проекту, спринту, исполнителю, текстовый поиск |
| 📊 **Отчёты** | Статистика по спринтам и продуктивность команды |
| 🗄️ **Auto-DB** | Автоматическое создание и заполнение базы данных |

---

## 💻 Технологический стек

```
Язык:        C# 13
Платформа:   .NET 9.0 Windows
UI:          WPF + Material Design 3 (тёмная тема)
Паттерн:     MVVM (CommunityToolkit.Mvvm)
БД:          MS SQL Server / LocalDB
ORM:         Entity Framework Core 9
```

### NuGet пакеты

| Пакет | Версия | Назначение |
|-------|--------|-----------|
| `MaterialDesignThemes` | 5.x | Тема Material Design 3 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.x | EF Core для SQL Server |
| `CommunityToolkit.Mvvm` | 8.x | MVVM утилиты |

---

## 📥 Установка

### Вариант 1: Готовая сборка (рекомендуется)

1. Перейти в папку [`Release/`](./Release/)
2. Скачать `AgileBoard_v1.0_Release.zip`
3. Распаковать в любую папку
4. Установить **SQL Server LocalDB** если не установлен:
   ```
   https://aka.ms/sqllocaldb
   ```
5. Запустить `Kurs_AgileDashbord.exe`

> 💡 LocalDB входит в состав Visual Studio — если VS установлена, дополнительная установка не нужна.

### Вариант 2: Сборка из исходников

```bash
# Требования: .NET 9.0 SDK, SQL Server или LocalDB
git clone https://github.com/YOUR_USERNAME/Kurs_AgileDashbord.git
cd Kurs_AgileDashbord/Kurs_AgileDashbord
dotnet run
```

---

## 🚀 Быстрый старт

При первом запуске приложение **автоматически**:
1. Найдёт SQL Server (LocalDB / Express / localhost)
2. Создаст базу данных `AgileBoardDB`
3. Заполнит тестовыми данными (7 пользователей, 3 проекта, 5 спринтов, 16 задач)

### Тестовые аккаунты

| Роль | Email | Пароль | Права |
|------|-------|--------|-------|
| 👑 Администратор | `ivanov@company.kz` | `admin123` | Все задачи, удаление |
| 👤 Разработчик | `petrova@company.kz` | `user123` | Только свои задачи |
| 👤 Разработчик | `sidorov@company.kz` | `user123` | Только свои задачи |

---

## 🏗️ Архитектура

### Структура проекта

```
Kurs_AgileDashbord/
├── 📁 Data/
│   ├── AgileBoardContext.cs      # EF Core DbContext
│   └── DatabaseInitializer.cs   # Авто-создание и seed БД
├── 📁 Models/
│   ├── User.cs                  # Пользователь + роль + авторизация
│   ├── Project.cs               # Проект
│   ├── Sprint.cs                # Спринт
│   ├── TaskItem.cs              # Задача (Kanban-карточка)
│   ├── Comment.cs               # Комментарий
│   └── TaskHistory.cs           # История изменений
├── 📁 ViewModels/
│   └── MainViewModel.cs         # Главный VM: фильтры, Kanban, отчёты
├── 📁 Views/
│   ├── LoginWindow.xaml         # Окно авторизации
│   ├── TaskDialog.xaml          # Диалог создания задачи
│   └── TaskDetailDialog.xaml    # Детали задачи + комментарии
├── 📁 Converters/               # IValueConverter для UI
├── 📁 SQL/                      # SQL скрипты (для ознакомления)
│   ├── 01_CreateDatabase.sql    # Схема БД
│   ├── 02_ViewsTriggersProcs.sql # Views, Triggers, Procedures
│   ├── 03_TestData.sql          # Тестовые данные
│   └── 04_AddAuth.sql           # Поля авторизации
├── MainWindow.xaml              # Главное окно (Kanban + Отчёты)
└── App.xaml.cs                  # Startup: DB init → Login → Main
```

### База данных

```sql
Users          -- Пользователи системы (роль, email, пароль, IsAdmin)
Projects       -- Проекты (название, код)
Sprints        -- Спринты (даты, IsActive)
Tasks          -- Задачи (статус, приоритет, исполнитель)
Comments       -- Комментарии к задачам
TaskHistory    -- Автоматическая история изменений (Trigger)
```

### Ролевая модель

```
Администратор:
  ✅ Видит все задачи всех пользователей
  ✅ Создаёт/удаляет задачи
  ✅ Меняет статус любой задачи
  ✅ Полный доступ к отчётам

Пользователь:
  ✅ Видит только свои задачи (ExecutorID = UserID)
  ✅ Меняет статус только своих задач
  ❌ Не может удалять задачи
```

---

## 🗄️ Поддерживаемые БД

Приложение автоматически пробует подключиться в порядке:

1. `(localdb)\MSSQLLocalDB` — SQL Server LocalDB (Visual Studio)
2. `localhost` — полный SQL Server
3. `.\SQLEXPRESS` — SQL Server Express

---

## 📸 Скриншоты

### Авторизация
![Login](docs/screenshots/login.png)

### Kanban-доска
![Board](docs/screenshots/board.png)

### Отчёты
![Reports](docs/screenshots/reports.png)

---

## 👨‍💻 Автор

**Курсовая работа**
- Дисциплина: Базы данных и информационные системы
- Стек: C# / WPF / EF Core / SQL Server
- Год: 2026

---

## 📄 Лицензия

MIT License — используйте свободно для учебных целей.
