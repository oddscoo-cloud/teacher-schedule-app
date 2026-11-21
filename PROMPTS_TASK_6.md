# Промпт 6 — Дашборд «Сегодня» и финансовые предупреждения

## Цель
Реализовать главный экран дашборда, показывающий сегодняшние занятия с быстрой сменой статуса, очередь проверки домашних заданий и мини-отчёт о финансовом состоянии учеников в рамках разрешений текущего пользователя.

## Зависит от
- Задача 1 (завершена): аутентификация и рабочие пространства
- Задача 2 (завершена): модели учеников
- Задача 3 (завершена): оплаты и расчёт балансов
- Задача 4 (завершена): календарь и занятия
- Задача 5 (завершена): домашние задания

## Работы

### API (Backend)

1. **DashboardController** (требует авторизации):
   - `GET /api/dashboard/today` — данные для главного экрана (занятия, очередь проверки, финансовый отчёт)
   - Возвращает JSON со структурой:
     ```json
     {
       "todaysLessons": [
         {
           "id": "lesson-id",
           "studentName": "Иван Петров",
           "startTime": "2024-11-18T14:00:00Z",
           "duration": "PT1H",
           "status": "Scheduled",
           "courseTitle": "Математика"
         }
       ],
       "submissionsToReview": [
         {
           "id": "submission-id",
           "studentName": "Иван Петров",
           "homeworkTitle": "Задача №1",
           "submittedAt": "2024-11-18T10:00:00Z",
           "isLate": false
         }
       ],
       "studentFinancialReport": [
         {
           "studentId": "student-id",
           "studentName": "Иван Петров",
           "lessonBalance": 5,
           "moneyBalance": -1500.0,
           "warningLevel": "none" | "low_balance" | "high_debt"
         }
       ],
       "stats": {
         "todaysLessonCount": 3,
         "submissionsCount": 2,
         "lowBalanceCount": 1,
         "highDebtCount": 0
       }
     }
     ```

2. **DashboardService** (Backend сервис):
   - `GetTodaysDashboard(userId, workspaceId)` → DashboardViewModel
   - Агрегирует данные из LessonService, HomeworkService и BalanceCalculationService
   - Преподаватель видит только своих учеников
   - Администратор видит всех учеников школы
   - Оптимизирует запросы (использует включение (Include) для избежания N+1)

### Фронтенд

1. **DashboardPage** — главная страница после входа:
   - Показывает вкладки/секции для быстрого навигации
   - Мобильная адаптивность (Stack на мобильных, Columns на десктопе)

2. **Компоненты:**

   **TodaysLessons** — сегодняшние занятия:
   - Список/таблица занятий на сегодня, отсортировано по времени (от ранних к поздним)
   - Колонки: время, студент, курс, статус
   - Быстрые кнопки действий: "Провести ✓", "Отменить ✗", "Перенести →" (вызывают RescheduleModal)
   - Статус отображается цветом (Scheduled=серый, Completed=зелёный, Cancelled=красный)
   - Пустое состояние: "Нет занятий на сегодня"
   - Клик на строку открывает детали занятия (опционально)

   **SubmissionsQueue** — очередь проверки:
   - Таблица с новыми отправками (отсортировано по дате, новые первыми)
   - Колонки: студент, задание, время сдачи, статус (отправлена / просмотрена / оценена)
   - Индикатор просрочки (красный значок, если отправлено после срока)
   - Быстрая кнопка "Проверить" → переход в SubmissionDetails для оценивания
   - Счётчик в заголовке "Ожидает проверки: 5"
   - Пустое состояние: "Нет работ на проверку"
   - Опционально: фильтр по студенту/курсу

   **FinancialReport** — финансовый мини-отчёт:
   - Таблица студентов с колонками: студент, остаток занятий, денежный баланс, статус
   - Цветовой код: зелёный (всё ок), жёлтый (низкий остаток), красный (есть долг)
   - Параметры предупреждения (настраиваются):
     - `LowBalanceThreshold` = 2 (жёлтый, если остаток ≤ 2)
     - `HighDebtThreshold` = 2000 (красный, если долг ≥ 2000 UAH)
   - Быстрая кнопка "Показать платежи" → переход на страницу платежей студента
   - Пустое состояние: "Нет студентов"

   **StatsPanel** — сводка статистики:
   - Четыре карточки: "Занятий сегодня", "На проверку", "Низкий остаток", "Есть долг"
   - Числовые значения и мини-графики (опционально)
   - Клик на карточку прокручивает к соответствующей секции

   **LoadingAndErrors**:
   - Spinner при загрузке
   - Сообщение об ошибке с кнопкой "Перезагрузить"
   - Пустые состояния во всех секциях

3. **Маршруты:**
   - `/dashboard` или `/` — главный экран дашборда
   - Доступен для преподавателя (видит своих студентов) и администратора (видит всех)

4. **API-клиент:**
   - `fetchDashboard()` → DashboardViewModel

5. **Параметры и конфигурация (добавить в appsettings.json):**
   ```json
   "Dashboard": {
     "LowBalanceThreshold": 2,
     "HighDebtThreshold": 2000,
     "RefreshIntervalSeconds": 30
   }
   ```

6. **Auto-refresh (опционально):**
   - Дашборд может автоматически обновляться каждые 30 сек (параметр выше)
   - Не блокирует UI; новые данные загружаются в фоне
   - Пользователь видит индикатор синхронизации ("Обновлено 2 мин назад")

## Критерии готовности

✅ API:
- Endpoint `/api/dashboard/today` реализован
- Возвращает корректные данные для преподавателя (только своих студентов) и администратора (всех)
- Данные согласованы с LessonService, HomeworkService и BalanceCalculationService
- Ошибки обработаны (нет студентов, нет занятий)
- Запрос оптимизирован (Include для связей, WHERE для фильтра по дате)

✅ Фронтенд:
- Дашборд открывается после входа без ошибок
- Все секции (занятия, очередь, финансы) загружаются и отображаются
- Быстрые кнопки действий работают (провести, отменить, перенести, проверить)
- Цветовые коды и предупреждения видны
- Переход на детали занятия/отправки работает
- Пустые состояния обработаны
- Мобильная адаптивность проверена (стек на мобильных)

✅ Параметры и настройка:
- LowBalanceThreshold и HighDebtThreshold читаются из appsettings.json
- Визуальный пороги срабатывают корректно
- Auto-refresh (если реализован) работает без блокировки UI

✅ Безопасность и авторизация:
- Преподаватель видит только своих учеников
- Администратор видит всех
- Нет утечки данных между пространствами
- Серверная проверка прав на каждый запрос

✅ Тесты:
- Юнит-тесты для `DashboardService` (агрегация данных, фильтрация по преподавателю/администратору)
- Интеграционные тесты для DashboardController (доступ, авторизация, пустые состояния)
- Граничные тесты: преподаватель без студентов, студент с нулевым балансом, нет занятий на сегодня
- Тесты доступа: студент не может видеть дашборд, преподаватель не видит других преподавателей
- Все тесты проходят: `dotnet test backend.sln`

## Обязательные коммиты

Формат:
```
task/6-dashboard-and-warnings: <описание>

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Рекомендуемый порядок коммитов:

1. `task/6-dashboard-and-warnings: Create DashboardViewModel and DTO structures`
2. `task/6-dashboard-and-warnings: Implement DashboardService with data aggregation and filtering`
3. `task/6-dashboard-and-warnings: Create DashboardController with /api/dashboard/today endpoint`
4. `task/6-dashboard-and-warnings: Add dashboard configuration to appsettings.json`
5. `task/6-dashboard-and-warnings: Add unit tests for DashboardService and boundary cases`
6. `task/6-dashboard-and-warnings: Add integration tests for DashboardController and authorization`
7. `task/6-dashboard-and-warnings: Implement DashboardPage component with main layout`
8. `task/6-dashboard-and-warnings: Implement TodaysLessons component with quick action buttons`
9. `task/6-dashboard-and-warnings: Implement SubmissionsQueue component with sorting and indicators`
10. `task/6-dashboard-and-warnings: Implement FinancialReport component with color coding and thresholds`
11. `task/6-dashboard-and-warnings: Implement StatsPanel with summary cards`
12. `task/6-dashboard-and-warnings: Add error handling and empty state components`
13. `task/6-dashboard-and-warnings: Implement auto-refresh with configurable interval (optional)`
14. `task/6-dashboard-and-warnings: Create API client function fetchDashboard()`
15. `task/6-dashboard-and-warnings: Set DashboardPage as default route after login`
16. `task/6-dashboard-and-warnings: Add mobile responsiveness and adaptive layout`
17. `task/6-dashboard-and-warnings: Update README with dashboard features and warning thresholds`

## Что проверить перед финализацией

```powershell
# Сборка без ошибок
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\backend'
dotnet build backend.sln --no-restore

# Все тесты
dotnet test backend.sln --no-restore --filter "Category=DashboardTests"

# Frontend собирается
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\frontend'
npm run build

# Docker Compose и логин
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app'
docker compose up --build
# Проверить:
# - Вход в приложение работает
# - Дашборд открывается после входа
# - Все секции загружаются без ошибок
docker compose down
```

## Обновить IMPLEMENTATION_PLAN.md

```
| 6. Дашборд и предупреждения | Завершена | **Выполнено:** Реализован DashboardService с агрегацией данных для занятий, очереди проверки и финансового отчёта. DashboardController с endpoint /api/dashboard/today, фильтрация по ролям и пространствам. Параметры LowBalanceThreshold и HighDebtThreshold в appsettings.json. Фронтенд: компоненты DashboardPage, TodaysLessons (с быстрыми кнопками), SubmissionsQueue (сортировка, индикаторы), FinancialReport (цветовые коды), StatsPanel (сводка). Auto-refresh с дебаунсингом. Маршрут / (или /dashboard) для главного экрана. Юнит-тесты для DashboardService с граничными случаями (нет студентов, нет занятий). Тесты доступа и авторизации. Мобильная адаптивность. Все тесты пройдены. **Файлы:** Core/Services/DashboardService.cs, Presentation/Controllers/DashboardController.cs, Presentation/ViewModels/DashboardViewModel.cs, frontend/src/pages/DashboardPage.js, frontend/src/components/Dashboard/*.js, appsettings.json (обновлен). **Ветка:** task/6-dashboard-and-warnings с 17 коммитами. |
```

## Критический момент: производительность

- **Не используйте N+1 запросы:** явно включайте связи в запросе (`Include()`)
- **Кэшируйте недорогие данные:** если дашборд часто обновляется, добавьте кэш на уровне сервиса (напр. MemoryCache на 1 минуту)
- **Пагинируйте большие списки:** если студентов > 100, покажите первых 10 и дайте ссылку "Показать все"
- **Оптимизируйте COUNT запросы:** используйте индексы на часто фильтруемых полях
- Проверьте время ответа при 1000+ учеников на одного преподавателя

