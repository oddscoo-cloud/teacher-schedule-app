# 📝 Пример запроса для реализации Задачи 2

## ✅ Вариант 1: МИНИМАЛЬНЫЙ запрос (если нужно быстро начать)

```
Начни реализацию Задачи 2 из IMPLEMENTATION_PLAN.md.
Следи файл PROMPTS_TASK_2.md.
Создай StudentEntity, ContactEntity и CourseEntity модели.
Запусти коммит после каждого логичного блока.
```

---

## ✅ Вариант 2: ПОЛНЫЙ запрос (рекомендуется)

```
Начни реализацию Задачи 2 "Школа, ученики и курсы" согласно PROMPTS_TASK_2.md

Контекст:
- Задача 1 ✅ завершена (аутентификация, рабочие пространства)
- Нужно реализовать управление школьными учениками, контактами и курсами
- Преподаватель видит только своих учеников, администратор видит всех

Работы (в порядке):
1. Создать модели (сущности):
   - StudentEntity (фамилия, имя, отчество, email, телефон, дата рождения)
   - ContactEntity (родители/опекуны)
   - StudentContactEntity (связь многие-ко-многим)
   - CourseEntity (категория/курс)
   - StudentAssignmentEntity (назначение преподавателя)
   - StudentNoteEntity (личные заметки преподавателя)

2. Создать EF Core миграцию AddStudentManagement:
   - Добавить индексы на email, FK школа-ученик, преподаватель-назначение
   - Обеспечить целостность FK
   - Миграция должна быть idempotent

3. Реализовать Backend сервисы:
   - StudentService (получение, создание, фильтрация, авторизация)
   - CourseService (CRUD курсов)
   - StudentAssignmentService (назначение учеников)
   - Проверить авторизацию на каждом запросе

4. Реализовать API endpoints (требует авторизации):
   - StudentsController: GET/POST/PUT учеников, фильтрация по имени/курсу
   - CoursesController: GET/POST/PUT/DELETE курсов
   - StudentAssignmentsController: назначение/отзыв преподавателя
   - StudentNotesController: заметки преподавателя

5. Реализовать фронтенд компоненты:
   - StudentList (таблица с поиском и фильтрацией)
   - StudentCard (карточка с контактами и заметками)
   - StudentForm (форма создания/редактирования)
   - CourseFilter (выпадающий список курсов)
   - StudentNotesList (список заметок)

6. Добавить маршруты:
   - /teacher/students
   - /teacher/students/:id
   - /admin/school/students
   - /admin/school/courses

7. Добавить юнит-тесты и интеграционные тесты

Критичные моменты:
- Преподаватель видит ТОЛЬКО своих учеников
- Администратор видит ВСЕХ учеников школы
- Личные заметки видны ТОЛЬКО автору и администратору
- Нет межпространственного доступа (школьные ≠ standalone)
- Серверная авторизация на КАЖДОМ запросе

Коммиты (в рекомендуемом порядке):
1. task/2-school-and-students: Create StudentEntity, ContactEntity and CourseEntity models
2. task/2-school-and-students: Create EF Core migration AddStudentManagement
3. task/2-school-and-students: Implement StudentService with authorization checks
4. ... (всего 14 коммитов, см. PROMPTS_TASK_2.md)

Проверка перед финализацией:
- dotnet build backend.sln --no-restore ✅
- dotnet test backend.sln --no-restore ✅
- npm run build (фронтенд) ✅
- docker compose up --build ✅

После завершения:
- Обновить IMPLEMENTATION_PLAN.md таблицу статуса
- Слить в main ветку
```

---

## ✅ Вариант 3: ДЕТАЛЬНЫЙ запрос (если нужны подробности)

```
Начни реализацию Задачи 2 согласно PROMPTS_TASK_2.md

📋 Часть 1: Модели и миграции

Создай файлы в Core/Entities/:
1. StudentEntity.cs
   - Свойства: Id (GUID), FirstName, LastName, Patronymic, Email, Phone, DateOfBirth, WorkspaceId
   - Навигационное свойство: Contacts (многие-ко-многим через StudentContactEntity)
   - Навигационное свойство: Assignments (многие-ко-многим через StudentAssignmentEntity)
   - Навигационное свойство: Notes (одно-ко-многим, StudentNoteEntity)

2. ContactEntity.cs
   - Свойства: Id (GUID), FullName, Phone, Email, Relationship
   - Навигационное свойство: Students (многие-ко-многим)

3. StudentContactEntity.cs
   - Свойства: StudentId (FK), ContactId (FK)
   - Первичный ключ: (StudentId, ContactId)

4. CourseEntity.cs
   - Свойства: Id (GUID), Title, Description, SchoolId (FK на SchoolEntity), CreatedAt
   - Индекс на SchoolId для быстрого поиска

5. StudentAssignmentEntity.cs
   - Свойства: StudentId (FK), TeacherId (FK), WorkspaceId (FK), AssignmentType (enum: "School", "Standalone")
   - Первичный ключ: (StudentId, TeacherId)

6. StudentNoteEntity.cs
   - Свойства: Id (GUID), StudentId (FK), TeacherId (FK), Content, CreatedAt, UpdatedAt
   - Только автор-преподаватель может редактировать/удалять

Создай миграцию:
dotnet ef migrations add AddStudentManagement --project backend --startup-project backend
- Добавить индексы на Email, FK школа-ученик, преподаватель-назначение
- Уникальный индекс на (StudentId, TeacherId) в StudentAssignmentEntity

📋 Часть 2: Backend сервисы

Создай файлы в Core/Services/:
1. StudentService.cs
   - GetStudentsForTeacher(teacherId, workspaceId) - преподаватель видит только своих
   - GetAllStudentsForSchool(schoolId) - администратор видит всех
   - GetStudent(studentId, userId, workspaceId) - проверка прав доступа
   - CreateStudent(...) - создать ученика
   - UpdateStudent(...) - обновить ученика
   - DeleteStudent(...) - удалить ученика (мягкое удаление)
   - GetStudentsBySearch(searchTerm, teacherId) - поиск по имени

2. CourseService.cs
   - GetCourses(schoolId) - список всех курсов школы
   - CreateCourse(...) - только администратор
   - UpdateCourse(...) - только администратор
   - DeleteCourse(...) - только администратор

3. StudentAssignmentService.cs
   - AssignStudent(studentId, teacherId, workspaceId) - назначить ученика
   - UnassignStudent(studentId, teacherId) - отозвать назначение
   - GetAssignments(studentId) - список всех назначений ученика

Все сервисы должны проверять WorkspaceId и роль пользователя на СЕРВЕРЕ.

📋 Часть 3: API контроллеры

Создай Presentation/Controllers/:
1. StudentsController.cs с endpoints:
   - [HttpGet] /api/students - список учеников текущего преподавателя
   - [HttpGet] /api/students/{id} - карточка ученика (полные контакты, заметки, история)
   - [HttpPost] /api/students - создать ученика
   - [HttpPut] /api/students/{id} - обновить ученика
   - [HttpGet] /api/students?search=&course=&status= - фильтрация

2. CoursesController.cs с endpoints:
   - [HttpGet] /api/courses - список всех курсов школы
   - [HttpPost] /api/courses - создать курс (только администратор)
   - [HttpPut] /api/courses/{id} - обновить курс
   - [HttpDelete] /api/courses/{id} - удалить курс

3. StudentAssignmentsController.cs
4. StudentNotesController.cs

Все endpoints имеют [Authorize], проверяют WorkspaceId, StudentAssignment и роль.

📋 Часть 4: Фронтенд компоненты

Создай frontend/src/components/:
1. StudentList.js - таблица учеников с поиском, фильтрацией
2. StudentCard.js - полная карточка ученика
3. StudentForm.js - форма создания/редактирования
4. CourseFilter.js - выпадающий список курсов
5. StudentNotesList.js - список заметок

Создай frontend/src/pages/:
1. StudentsPage.js - главная страница учеников
2. StudentDetailsPage.js - карточка конкретного ученика

📋 Часть 5: Маршруты и API клиент

Обнови frontend/src/App.js:
- /teacher/students -> StudentsPage
- /teacher/students/:id -> StudentDetailsPage
- /admin/school/students -> AdminStudentsPage
- /admin/school/courses -> AdminCoursesPage

Обнови frontend/src/api.js:
- fetchStudents(filters)
- fetchStudent(id)
- createStudent(data)
- updateStudent(id, data)
- deleteStudent(id)
- fetchCourses()
- fetchNotes(studentId)
- createNote(studentId, content)
- updateNote(studentId, noteId, content)
- deleteNote(studentId, noteId)

📋 Часть 6: Тесты

Создай backend.Tests/:
1. StudentServiceTests.cs - 10+ тестов
   - GetStudentsForTeacher - преподаватель видит только своих
   - GetAllStudentsForSchool - администратор видит всех
   - CreateStudent, UpdateStudent, DeleteStudent
   - GetStudentsBySearch - поиск корректен
   - Авторизация - преподаватель не может видеть чужих

2. CourseServiceTests.cs - 5+ тестов

3. StudentControllerIntegrationTests.cs
4. CourseControllerIntegrationTests.cs

📋 Часть 7: Проверка и коммиты

После каждого логичного блока (модели, миграция, сервис, контроллер, компонент):
git add .
git commit -m "task/2-school-and-students: [описание что именно сделал]

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"

После всего:
dotnet build backend.sln --no-restore
dotnet test backend.sln --no-restore
npm run build
docker compose up --build
docker compose down

Обновить D:\teacher-schedule-app\teacher-schedule-app\IMPLEMENTATION_PLAN.md таблицу статуса Задачи 2.
```

---

## 🎯 Как выбрать вариант?

| Вариант | Когда использовать |
|---|---|
| **Вариант 1 (Минимальный)** | Когда ты хорошо знаешь файл PROMPTS_TASK_2.md и просто нужно начать |
| **Вариант 2 (Полный)** | **РЕКОМЕНДУЕТСЯ** — стандартный запрос для большинства случаев |
| **Вариант 3 (Детальный)** | Когда нужны микро-детали (имена методов, точная структура БД, etc) |

---

## 📌 Шаблон для собственного запроса

Используй этот шаблон для Задач 3–7:

```
Начни реализацию Задачи [N] согласно PROMPTS_TASK_[N].md

Контекст:
- Задача 1 ✅ завершена
- Задача [N-1] ✅ завершена
- Нужно реализовать [краткое описание]

Работы (в порядке):
1. [Часть 1]
2. [Часть 2]
3. [Часть 3]
...

Критичные моменты:
- [Критичный момент 1]
- [Критичный момент 2]

Коммиты (всего [N] коммитов):
1. task/[N]-feature: [описание]
2. task/[N]-feature: [описание]
...

Проверка перед финализацией:
- dotnet build ✅
- dotnet test ✅
- npm run build ✅
- docker compose up --build ✅
```

---

## 🚀 Готово!

Скопируй **Вариант 2** (Полный запрос) выше и используй его как основу для запроса реализации Задачи 2.

Если нужны большие детали, используй **Вариант 3**.

После каждой задачи переходи к следующей в таком же формате.
