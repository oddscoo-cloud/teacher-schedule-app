# Промпт 4 — Календарь, повторения и статусы

## Цель
Реализовать календарь с видами день/неделя/месяц, создание разовых и еженедельно повторяющихся занятий, управление статусами (проведено/отменено/перенесено) и интеграцию с финансовой системой из задачи 3.

## Зависит от
- Задача 1 (завершена): аутентификация и рабочие пространства
- Задача 2 (завершена): модели учеников и курсов
- Задача 3 (завершена): оплаты и расчёт балансов

## Работы

### Данные и модели

1. **LessonSeriesEntity** — шаблон/серия еженедельных повторений:
   - `Id` (GUID)
   - `TeacherId` (FK)
   - `StudentId` (FK)
   - `WorkspaceId` (FK)
   - `CourseId` (FK на CourseEntity, опционально)
   - `DayOfWeek` (enum: Monday, Tuesday, ..., Friday — из требований вроде Пн–Пт)
   - `StartTime` (TimeSpan HH:mm)
   - `Duration` (TimeSpan — длительность занятия)
   - `Title` (string — название занятия)
   - `Description` (string, опционально)
   - `StartDate` (DateTime — первое занятие серии)
   - `EndDate` (DateTime или NULL — если серия активна неопределённо долго)
   - `Timezone` (string — часовой пояс, напр. "Europe/Kiev", по умолчанию школьный)
   - `CreatedAt` (DateTime)
   - `IsActive` (bool — мягкое удаление для истории)

2. **LessonEntity** — конкретное занятие (разовое или из серии):
   - `Id` (GUID)
   - `SeriesId` (FK на LessonSeriesEntity, NULL если разовое)
   - `TeacherId` (FK)
   - `StudentId` (FK)
   - `WorkspaceId` (FK)
   - `CourseId` (FK)
   - `ScheduledDate` (DateTime — точная дата+время занятия)
   - `Title` (string)
   - `Description` (string, опционально)
   - `Status` (enum: "Scheduled", "Completed", "Cancelled", "Rescheduled")
   - `Duration` (TimeSpan)
   - `Timezone` (string)
   - `Notes` (string — заметки о занятии)
   - `CreatedAt` (DateTime)
   - `ModifiedAt` (DateTime)
   - `ModifiedBy` (UserId для аудита)
   - `RescheduledFrom` (GUID FK на другой LessonEntity, если это перенесённое занятие)
   - `SeriesInstanceIndex` (int — порядковый номер в серии, для редактирования/отмены)

3. **LessonRescheduleEntity** — журнал переносов (для аудита):
   - `Id` (GUID)
   - `OriginalLessonId` (FK)
   - `NewLessonId` (FK)
   - `RescheduleReason` (string)
   - `RescheduleDate` (DateTime)
   - `RescheduleBy` (UserId)

4. **Миграция EF Core:**
   - Создать миграцию `AddLessonSystem`
   - Добавить индексы на (TeacherId, StudentId, ScheduledDate)
   - Добавить индекс на (SeriesId, Status)
   - Обеспечить целостность FK с каскадным удалением для истории переносов

### API (Backend)

1. **LessonsController** (требует авторизации):
   - `GET /api/lessons?from=&to=&studentId=&courseId=&status=` — список занятий за период (фильтр по дате, студенту, курсу, статусу)
   - `GET /api/lessons/{id}` — деталь одного занятия
   - `POST /api/lessons` — создать разовое занятие
   - `POST /api/lessons/series` — создать серию еженедельных повторений
   - `PUT /api/lessons/{id}` — изменить одно занятие (разовое или отдельный экземпляр серии)
   - `PUT /api/lessons/series/{seriesId}` — изменить всю серию (все будущие экземпляры)
   - `PUT /api/lessons/{id}/status` — изменить статус (Scheduled → Completed, Cancelled, Rescheduled)
   - `DELETE /api/lessons/{id}` — отменить занятие (мягкое удаление через Status)
   - `POST /api/lessons/{id}/reschedule` — перенести занятие на новое время

2. **LessonService** (Backend сервис):
   - `CreateLesson(...)` → LessonEntity
   - `CreateSeries(...)` → LessonSeriesEntity (генерирует экземпляры на N недель вперёд)
   - `GenerateSeriesInstances(seriesId, fromDate, toDate)` → List<LessonEntity> (на лету для календаря)
   - `UpdateLesson(id, ...)` — обновить одно занятие
   - `UpdateSeries(seriesId, ...)` — обновить серию и все будущие экземпляры
   - `ChangeStatus(lessonId, newStatus, reason, cancelledAt)` → bool (вызывает BalanceCalculationService.ChargeLesson)
   - `RescheduleLesson(lessonId, newDate)` → LessonEntity (не должно создавать дубликатные начисления)
   - `GetLessonsInRange(teacherId, fromDate, toDate, studentId?, courseId?)` → List<LessonEntity>
   - Проверка прав доступа: преподаватель видит только своих учеников, администратор видит всех

3. **Часовой пояс (важно):**
   - Одна школа имеет один часовой пояс (или каждое пространство имеет свой)
   - Преобразование между UTC и локальным временем при сохранении/чтении
   - Переходы на летнее/зимнее время (DST) обрабатываются корректно
   - Используйте `NodaTime` для .NET или встроенные TimeZoneInfo для работы с часовыми поясами

4. **Идемпотентность переносов:**
   - Перенос одного занятия не должен повлиять на остаток занятий и денежный долг
   - Перенос не удаляет и не создаёт новое начисление
   - Для редактирования всей серии: все будущие экземпляры обновляются, прошедшие остаются неизменены

### Фронтенд

1. **Компоненты:**
   - `Calendar` — главный компонент с переключателем день/неделя/месяц
   - `DayView` — отображение одного дня, сетка по времени с занятиями
   - `WeekView` — неделя (Пн–Пт по требованиям), сетка неделя×время
   - `MonthView` — месячная сетка, точки для занятий
   - `LessonCard` — карточка занятия в календаре, быстрое изменение статуса (кнопки: "Провести", "Отменить", "Перенести")
   - `LessonForm` — форма создания/редактирования занятия (разовое или серия)
   - `LessonDetails` — детальное окно/модалка с полной информацией, заметками, историей
   - `SeriesForm` — форма для создания еженедельной серии
   - `RescheduleModal` — модалка для выбора новой даты/времени при переносе

2. **Маршруты:**
   - `/teacher/calendar` — календарь преподавателя (день/неделя/месяц)
   - `/teacher/calendar?view=week&student=:studentId` — фильтр по студенту
   - `/teacher/lessons/new` — создание занятия
   - `/teacher/lessons/:id` — деталь занятия
   - `/admin/school/calendar` — календарь школы (все преподаватели, администратор видит всех)

3. **API-клиент:**
   - Расширить `frontend/src/api.js`: `fetchLessons`, `createLesson`, `createSeries`, `updateLesson`, `updateLessonStatus`, `rescheduleLesson`, `fetchLessonDetails`

4. **Состояния и визуализация:**
   - Scheduled — серый/обычный
   - Completed — зелёный (галочка)
   - Cancelled — красный (крест)
   - Rescheduled → новое занятие (стрелка)
   - Пустые слоты/дни без занятий должны быть видны

## Критерии готовности

✅ Модели и миграции:
- LessonSeriesEntity, LessonEntity, LessonRescheduleEntity созданы
- Миграция `AddLessonSystem` применяется успешно
- Индексы добавлены
- FK целостность обеспечена

✅ Логика создания и редактирования:
- Создание разовое занятие работает
- Создание серии еженедельных повторений генерирует корректные даты
- Редактирование одного занятия не влияет на серию (если это экземпляр серии)
- Редактирование серии обновляет все будущие экземпляры, прошедшие остаются неизменены
- Перенос занятия не создаёт дубликатные начисления и не теряет историю

✅ Интеграция с финансами (задача 3):
- Статус "Completed" вызывает ChargeLesson (списание 1 занятия, начисление суммы)
- Статус "Cancelled" в течение 5 часов до начала: списание 1 занятия + начисление суммы
- Статус "Cancelled" за 5+ часов: ничего не начисляется
- Перенос не вызывает начисление/списание
- BalanceCalculationService согласована с LessonService

✅ Часовой пояс:
- Всё время хранится в UTC в БД
- Преобразование в локальный часовой пояс при чтении
- DST переходы обработаны корректно
- Часовой пояс явно настраивается (не угадывается)

✅ API:
- Все endpoints реализованы
- Авторизация: преподаватель видит только своих учеников, администратор видит всех
- Фильтрация по дате, студенту, курсу, статусу работает без SQL-injection
- Пагинация, если нужна

✅ Фронтенд:
- Три вида (день/неделя/месяц) работают без ошибок
- Переключение между видами плавное
- Быстрое изменение статуса (кнопка на карточке) работает
- Перенос занятия через модалку работает
- История переносов/отмен видна в деталях занятия
- Пустые состояния обработаны (нет занятий в период)

✅ Тесты:
- Юнит-тесты для `LessonService` (создание, редактирование, статусы, граничные случаи)
- Юнит-тесты для генерации серий (корректные даты, обработка конца серии)
- Юнит-тесты для переносов (идемпотентность, отсутствие дубликатных начислений)
- Юнит-тесты для часовых поясов (преобразование в UTC, обратно в локальный, DST)
- Интеграционные тесты для LessonsController (доступ, статусы, фильтрация)
- Граничные тесты: отмена точно в 5 часов, отмена за 1 сек до 5 часов, перенос на день недели вне рабочего диапазона (если применимо)
- Все тесты проходят: `dotnet test backend.sln`

## Обязательные коммиты

Формат:
```
task/4-calendar-and-lessons: <описание>

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Рекомендуемый порядок коммитов:

1. `task/4-calendar-and-lessons: Create LessonSeriesEntity, LessonEntity and LessonRescheduleEntity models`
2. `task/4-calendar-and-lessons: Create EF Core migration AddLessonSystem with indices and FK constraints`
3. `task/4-calendar-and-lessons: Implement LessonService with series generation and status transitions`
4. `task/4-calendar-and-lessons: Implement timezone handling with NodaTime or TimeZoneInfo`
5. `task/4-calendar-and-lessons: Implement lesson creation, editing and rescheduling logic with idempotency`
6. `task/4-calendar-and-lessons: Integrate LessonService with BalanceCalculationService for charge logic`
7. `task/4-calendar-and-lessons: Create LessonsController with CRUD and filtering endpoints`
8. `task/4-calendar-and-lessons: Add unit tests for LessonService and timezone transformations`
9. `task/4-calendar-and-lessons: Add unit tests for financial charge logic and boundary cases (5-hour rule)`
10. `task/4-calendar-and-lessons: Add integration tests for LessonsController and authorization`
11. `task/4-calendar-and-lessons: Implement Calendar, DayView, WeekView and MonthView components`
12. `task/4-calendar-and-lessons: Implement LessonCard, LessonDetails and quick status change buttons`
13. `task/4-calendar-and-lessons: Implement LessonForm, SeriesForm and RescheduleModal`
14. `task/4-calendar-and-lessons: Create API client functions for lessons, series and rescheduling`
15. `task/4-calendar-and-lessons: Add calendar routes and filtering by student/course to frontend`
16. `task/4-calendar-and-lessons: Update README with calendar, timezone settings and lesson management`

## Что проверить перед финализацией

```powershell
# Сборка без ошибок
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\backend'
dotnet build backend.sln --no-restore

# Тесты, включая граничные случаи на 5 часов
dotnet test backend.sln --no-restore --filter "Category=LessonTests or Category=TimezoneTests"

# Frontend собирается
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\frontend'
npm run build

# Docker Compose и миграции
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app'
docker compose up --build
# Проверить в логах: "Migration 'AddLessonSystem' applied"
docker compose down
```

## Обновить IMPLEMENTATION_PLAN.md

```
| 4. Календарь и повторения | Завершена | **Выполнено:** Созданы сущности LessonSeriesEntity, LessonEntity, LessonRescheduleEntity. EF Core миграция AddLessonSystem. Реализован LessonService с генерацией еженедельных серий, редактированием одного/всех занятий, переносами без дубликатных начислений. Часовой пояс (NodaTime или TimeZoneInfo) с корректной обработкой UTC и DST. Интеграция с BalanceCalculationService для финансовых начислений. LessonsController с endpoints фильтрации, создания, редактирования и изменения статуса. Фронтенд: компоненты Calendar (день/неделя/месяц), DayView, WeekView, MonthView, LessonCard с быстрой сменой статуса, LessonForm, SeriesForm, RescheduleModal, LessonDetails. Маршруты /teacher/calendar, /teacher/lessons/new, /admin/school/calendar. Юнит-тесты для генерации серий, переносов, 5-часового правила. Граничные тесты для отмены ровно в 5 часов и за 1 сек до лимита. Все тесты пройдены. **Файлы:** Core/Entities/Lesson*.cs, Core/Services/LessonService.cs, Infrastructure/Services/TimezoneService.cs, Presentation/Controllers/LessonsController.cs, frontend/src/components/Calendar/*.js, frontend/src/pages/Calendar.js. **Миграции:** AddLessonSystem. **Ветка:** task/4-calendar-and-lessons с 16 коммитами. |
```

## Критический момент: DST и часовые пояса

- Всегда храните время в UTC в БД
- Используйте специализированную библиотеку (NodaTime рекомендуется) или встроенные TimeZoneInfo
- Не доверяйте системному часовому поясу сервера; явно указывайте зону (напр. "Europe/Kiev")
- Проверьте DST переходы: 25 марта 2024 (на летнее время) и 27 октября 2024 (на зимнее время)
- Тестируйте граничные случаи вблизи переходов

