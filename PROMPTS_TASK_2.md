# Промпт 2 — Школа, ученики и курсы

## Цель
Реализовать модели и API для управления школьной базой учеников, курсами/категориями и назначениями преподавателей. Разделить школьных учеников от standalone-учеников преподавателя.

## Зависит от
- Задача 1 (завершена): аутентификация, рабочие пространства и роли

## Работы

### Данные и модели
1. **Расширить сущности:**
   - `StudentEntity` — базовые поля: фамилия, имя, отчество, email, телефон, дата рождения
   - `ContactEntity` — родители/опекуны с телефоном и email
   - `StudentContactEntity` — связь многие-ко-многим
   - `CourseEntity` — категория/курс (название, описание, школа)
   - `StudentAssignmentEntity` — назначение ученика одному или нескольким преподавателям (с флагом школьный/standalone)
   - `StudentNoteEntity` — личные заметки преподавателя (цели, прогресс) с указанием автора; видны только автору и администратору школы

2. **Миграции EF Core:**
   - Создать миграцию `AddStudentManagement` (или аналогичное имя)
   - Добавить индексы на email, FK школа-ученик, преподаватель-назначение
   - Обеспечить целостность FK и предусмотреть backfill для старых юзеров (если есть)
   - Миграция должна быть idempotent; выполниться на чистой и существующей БД без потери данных

### API (Backend)
1. **StudentsController** (требует авторизации):
   - `GET /api/students` — список учеников текущего преподавателя (школьных или standalone)
   - `GET /api/students/{id}` — карточка ученика (полные контакты, заметки, история занятий)
   - `POST /api/students` — создать ученика (только администратор или преподаватель в standalone)
   - `PUT /api/students/{id}` — обновить ученика (администратор или владелец-преподаватель)
   - `GET /api/students?search=&course=&status=` — фильтрация по имени, курсу, статусу подтверждения

2. **CoursesController** (требует авторизации):
   - `GET /api/courses` — список всех курсов школы (доступен для школьных пользователей и администратора)
   - `POST /api/courses` — создать курс (только администратор школы)
   - `PUT /api/courses/{id}` — обновить курс (только администратор)
   - `DELETE /api/courses/{id}` — удалить курс (только администратор)

3. **StudentAssignmentsController** (требует авторизации):
   - `POST /api/students/{studentId}/assign-teacher/{teacherId}` — назначить ученика преподавателю (администратор)
   - `DELETE /api/students/{studentId}/assign-teacher/{teacherId}` — отозвать назначение
   - `GET /api/students/{studentId}/assignments` — список назначений ученика

4. **StudentNotesController** (требует авторизации):
   - `GET /api/students/{id}/notes` — заметки ученика (видны только автору и администратору)
   - `POST /api/students/{id}/notes` — создать заметку
   - `PUT /api/students/{id}/notes/{noteId}` — обновить заметку (только автор)
   - `DELETE /api/students/{id}/notes/{noteId}` — удалить заметку (только автор)

### Авторизация и безопасность
- Преподаватель видит только своих назначенных учеников
- Администратор видит всех школьных учеников
- Standalone-ученики видны только своему преподавателю
- Заметки видны только автору-преподавателю и администратору
- Удалить/обновить ученика может только администратор или владелец-преподаватель
- Все endpoints имеют атрибут `[Authorize]`
- Серверная проверка по WorkspaceId, StudentAssignment и роли — не доверять клиентской проверке

### Фронтенд
1. **Компоненты:**
   - `StudentList` — таблица/список учеников с поиском, фильтрацией по курсу и статусу
   - `StudentCard` — полная карточка ученика (контакты, родители, заметки, история занятий)
   - `StudentForm` — форма создания/редактирования ученика
   - `CourseFilter` — выпадающий список курсов для фильтрации
   - `StudentNotesList` — список заметок с возможностью добавления и редактирования

2. **Маршруты:**
   - `/teacher/students` — список учеников
   - `/teacher/students/:id` — карточка ученика
   - `/teacher/students/new` — форма создания
   - `/admin/school/students` — управление школьной базой (администратор)
   - `/admin/school/courses` — управление курсами (администратор)

3. **API-клиент:**
   - Расширить `frontend/src/api.js` функциями `fetchStudents`, `fetchStudent`, `createStudent`, `updateStudent`, `deleteStudent`, `fetchCourses`, `fetchNotes`, `createNote`, `updateNote`, `deleteNote`

## Критерии готовности

✅ Модели и миграции:
- Все сущности созданы и связаны через FK
- Миграция применяется на чистой и существующей БД без ошибок
- Индексы добавлены для поиска по email и наименованию

✅ API:
- Все endpoints реализованы и возвращают корректные данные
- Серверная авторизация проверяет WorkspaceId, роль и назначение на каждом запросе
- Фильтрация и поиск работают без SQL-injection

✅ Данные:
- Преподаватель видит только своих назначенных учеников
- Администратор видит всех школьных учеников
- Standalone-данные не видны в школьном режиме и наоборот
- Личные заметки видны только автору и администратору

✅ Фронтенд:
- Список, карточка, форма создания/редактирования работают без ошибок
- Поиск и фильтрация отображают корректные результаты
- Пустые состояния обработаны (нет учеников, нет заметок)

✅ Тесты:
- Юнит-тесты для `StudentService` (получение, создание, фильтрация, авторизация)
- Юнит-тесты для `CourseService` (создание, список, удаление, права администратора)
- Интеграционные тесты для endpoints (проверка ролей, доступа и данных)
- Все тесты проходят: `dotnet test backend.sln`

## Обязательные коммиты

Выполняй коммиты после каждого логичного блока работы. Используй названия в формате:

```
task/2-school-and-students: <описание>

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Рекомендуемый порядок коммитов:

1. `task/2-school-and-students: Create StudentEntity, ContactEntity and CourseEntity models`
2. `task/2-school-and-students: Create EF Core migration AddStudentManagement`
3. `task/2-school-and-students: Implement StudentService with authorization checks`
4. `task/2-school-and-students: Implement CourseService and StudentAssignmentService`
5. `task/2-school-and-students: Create StudentsController with CRUD endpoints`
6. `task/2-school-and-students: Create CoursesController and StudentAssignmentsController`
7. `task/2-school-and-students: Create StudentNotesController and StudentNoteService`
8. `task/2-school-and-students: Add unit tests for StudentService and CourseService`
9. `task/2-school-and-students: Add integration tests for Students and Courses endpoints`
10. `task/2-school-and-students: Implement StudentList, StudentCard and StudentForm components`
11. `task/2-school-and-students: Implement CourseFilter and StudentNotesList components`
12. `task/2-school-and-students: Create API client functions for Students, Courses and Notes`
13. `task/2-school-and-students: Add Student and Course management routes to frontend`
14. `task/2-school-and-students: Update README with School Students and Courses section`

## Что проверить перед финализацией

```powershell
# Сборка без ошибок
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\backend'
dotnet build backend.sln --no-restore

# Все тесты проходят
dotnet test backend.sln --no-restore

# Frontend собирается без ошибок
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\frontend'
npm run build

# Docker Compose запускается и миграция применяется
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app'
docker compose up --build  # Проверить логи на "Migration applied"
docker compose down
```

## Обновить IMPLEMENTATION_PLAN.md

После завершения обновить таблицу статуса:

```
| 2. Школа, ученики и курсы | Завершена | **Выполнено:** Созданы сущности StudentEntity, ContactEntity, CourseEntity, StudentAssignmentEntity, StudentNoteEntity. Реализована EF Core миграция AddStudentManagement. Созданы сервисы StudentService, CourseService, StudentAssignmentService с проверкой авторизации по ролям и пространствам. Реализованы endpoints: StudentsController, CoursesController, StudentAssignmentsController, StudentNotesController. Фронтенд: компоненты StudentList, StudentCard, StudentForm, CourseFilter, StudentNotesList. Маршруты /teacher/students, /admin/school/students, /admin/school/courses. Юнит-тесты и интеграционные тесты покрывают авторизацию, фильтрацию и изоляцию данных. **Файлы:** Core/Entities/Student*.cs, Infrastructure/Services/Student*.cs, Presentation/Controllers/Students*.cs, frontend/src/pages/Students/*.js, frontend/src/components/Student*.js, frontend/src/api.js (обновлен). **Миграции:** AddStudentManagement. **Ветка:** task/2-school-and-students с 14 коммитами. |
```

## Общие требования ко всем промптам

1. Перед началом: прочитать IMPLEMENTATION_PLAN.md, frontend/README.md, проверить текущее состояние кода
2. Переиспользовать существующие компоненты, сервисы и соглашения проекта
3. Не добавлять новые зависимости без обоснования
4. Реализовать полный цикл: данные → API → фронтенд
5. Включить валидацию и обработку ошибок
6. Писать тесты для критичного функционала и граничных случаев
7. Не хранить секреты в коде
8. Не ослаблять авторизацию
9. Делать регулярные коммиты после логичных блоков
10. Обновить README с новыми возможностями
11. Убедиться, что Docker Compose работает с новыми миграциями
