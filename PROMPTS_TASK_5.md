# Промпт 5 — Домашние задания и проверка

## Цель
Реализовать систему отправки домашних заданий, сохранения вложений (текст, фото, ссылки) и проверки преподавателем с выставлением числовой оценки 0–100.

## Зависит от
- Задача 1 (завершена): аутентификация и рабочие пространства
- Задача 2 (завершена): модели учеников и курсов

## Работы

### Данные и модели

1. **HomeworkEntity** — задание, созданное преподавателем:
   - `Id` (GUID)
   - `TeacherId` (FK)
   - `StudentId` (FK на StudentEntity, NULL если для курса)
   - `CourseId` (FK на CourseEntity, опционально)
   - `WorkspaceId` (FK)
   - `Title` (string)
   - `Description` (string — описание задания)
   - `DueDate` (DateTime или NULL)
   - `CreatedAt` (DateTime)
   - `ModifiedAt` (DateTime)
   - `IsActive` (bool — мягкое удаление)

2. **HomeworkSubmissionEntity** — отправка решения ученика:
   - `Id` (GUID)
   - `HomeworkId` (FK на HomeworkEntity)
   - `StudentId` (FK на StudentEntity)
   - `SubmissionDate` (DateTime — когда ученик отправил)
   - `Content` (string — текстовое содержание)
   - `IsLate` (bool — отправлено ли после DueDate)
   - `Status` (enum: "Submitted", "Reviewed", "Graded")
   - `CreatedAt` (DateTime)
   - `UpdatedAt` (DateTime)

3. **SubmissionAttachmentEntity** — вложение в отправку:
   - `Id` (GUID)
   - `SubmissionId` (FK на HomeworkSubmissionEntity)
   - `FileName` (string)
   - `FileType` (string — MIME type: image/jpeg, image/png)
   - `FileSize` (long — размер в байтах)
   - `StoragePath` (string — путь к файлу в хранилище)
   - `Url` (string — если вложение — ссылка, URL сохраняется)
   - `CreatedAt` (DateTime)

4. **SubmissionGradeEntity** — оценка от преподавателя:
   - `Id` (GUID)
   - `SubmissionId` (FK на HomeworkSubmissionEntity)
   - `TeacherId` (FK)
   - `Score` (int — 0–100)
   - `Feedback` (string — текст обратной связи)
   - `GradedAt` (DateTime)
   - `ModifiedAt` (DateTime)

5. **Миграция EF Core:**
   - Создать миграцию `AddHomeworkSystem`
   - Добавить индексы на (HomeworkId, StudentId), (TeacherId), (CourseId)
   - Уникальный индекс на (HomeworkId, StudentId) в HomeworkSubmissionEntity (один студент, одна отправка на задание)
   - Каскадное удаление (HomeworkSubmissionEntity → SubmissionAttachmentEntity, SubmissionGradeEntity)

### Конфигурация и хранилище файлов

1. **appsettings.json** — добавить параметры:
   ```json
   "FileStorage": {
     "MaxFileSizeBytes": 5242880,  // 5 MB
     "AllowedMimeTypes": ["image/jpeg", "image/png", "image/webp"],
     "StoragePath": "uploads/homework",  // относительно корня приложения
     "Url": "http://localhost:5101/uploads/homework"  // базовый URL
   }
   ```

2. **Хранилище:**
   - Локальное хранилище в папке `backend/uploads/homework/` (генерируется при старте)
   - Или Docker volume `homework_uploads:/app/uploads/homework` в docker-compose.yml
   - Файлы переименовываются с GUID + оригинальное расширение для уникальности
   - Удаление файлов при удалении отправки (cleanup при каскадном удалении)

3. **Безопасность:**
   - Проверка MIME type (не доверять расширению)
   - Проверка размера файла перед сохранением
   - Путь к файлу на диске не должен совпадать с путём приложения (LFI-защита)
   - Недопустимые расширения отклоняются (напр. .exe, .dll)
   - Доступ к файлу проверяется по правам (может скачать только преподаватель, студент или администратор)

### API (Backend)

1. **HomeworkController** (требует авторизации):
   - `GET /api/homework` — список заданий для студента или преподавателя
   - `GET /api/homework/{id}` — деталь задания с историей отправок
   - `POST /api/homework` — создать задание (только преподаватель)
   - `PUT /api/homework/{id}` — обновить задание (только автор-преподаватель)
   - `DELETE /api/homework/{id}` — мягкое удаление задания

2. **SubmissionsController** (требует авторизации):
   - `GET /api/homework/{homeworkId}/submissions` — очередь проверки для преподавателя
   - `GET /api/homework/{homeworkId}/submissions/{studentId}` — отправка конкретного студента
   - `POST /api/homework/{homeworkId}/submit` — отправить решение (студент)
   - `GET /api/homework/{homeworkId}/submissions/{studentId}/attachments/{attachmentId}` — скачать файл
   - `PUT /api/homework/{homeworkId}/submissions/{studentId}/grade` — выставить оценку (преподаватель)

3. **HomeworkService** (Backend сервис):
   - `CreateHomework(teacherId, studentId?, courseId, title, dueDate, workspaceId)` → HomeworkEntity
   - `CreateSubmission(homeworkId, studentId, content, attachments[], workspaceId)` → HomeworkSubmissionEntity (проверка размера, MIME type)
   - `GradeSubmission(submissionId, teacherId, score, feedback)` → SubmissionGradeEntity (валидация score 0–100)
   - `GetSubmissionsForReview(teacherId, workspaceId)` → List (отсортировано по дате, новые первыми)
   - `DeleteSubmissionAttachment(attachmentId)` → bool (удаляет файл с диска)
   - Проверка доступа на каждом шаге: преподаватель видит своих студентов, студент видит свои отправки

4. **FileStorageService** (Backend сервис):
   - `SaveFile(stream, originalFileName)` → (fileName, path, url) (генерирует GUID, сохраняет на диск)
   - `DeleteFile(storagePath)` → bool (удаляет файл)
   - `ValidateFile(fileName, fileSize, mimeType)` → (isValid, error) (проверка размера, типа, расширения)
   - `GetFileUrl(storagePath)` → string (возвращает доступный URL)

### Фронтенд

1. **Компоненты:**
   - `HomeworkList` — список заданий для студента (фильтр по статусу: новые, отправленные, проверенные)
   - `HomeworkForm` — форма создания задания для преподавателя (выбор студента/курса, название, описание, срок)
   - `SubmissionForm` — форма отправки решения ученику (текст, фото, ссылки)
   - `SubmissionCard` — карточка отправки (ученик, дата, статус, кнопка просмотра)
   - `SubmissionDetails` — модалка/страница с полными деталями отправки, вложениями, оценкой
   - `SubmissionQueue` — очередь проверки для преподавателя (отсортировано по дате, новые первыми)
   - `GradingPanel` — панель выставления оценки (ползунок/числовой ввод 0–100, текстовое поле обратной связи)
   - `FileUpload` — компонент загрузки файлов с проверкой типа/размера на клиенте

2. **Маршруты:**
   - `/student/homework` — список всех заданий студента
   - `/student/homework/:id` — деталь задания с формой отправки
   - `/teacher/homework` — управление заданиями преподавателя (создание, просмотр)
   - `/teacher/homework/:id/submissions` — очередь проверки по конкретному заданию
   - `/teacher/submissions-queue` — общая очередь проверки (все задания)
   - `/admin/school/homework` — управление всеми заданиями школы (администратор)

3. **API-клиент:**
   - Расширить `frontend/src/api.js`: `fetchHomework`, `createHomework`, `submitHomework`, `fetchSubmissions`, `gradeSubmission`, `downloadAttachment`, `deleteSubmission`

4. **UX/обработка ошибок:**
   - Валидация файлов на клиенте (размер, формат) с понятным сообщением об ошибке
   - Прогресс-бар загрузки файла
   - Пустые состояния: нет заданий, нет отправок, нет оценок
   - Обработка ошибок сервера (размер превышен, неверный формат)

## Критерии готовности

✅ Модели и миграции:
- HomeworkEntity, HomeworkSubmissionEntity, SubmissionAttachmentEntity, SubmissionGradeEntity созданы
- Миграция `AddHomeworkSystem` применяется успешно
- Уникальный индекс на (HomeworkId, StudentId) работает
- Каскадное удаление функционирует

✅ Хранилище файлов:
- Папка `backend/uploads/homework/` создаётся и имеет права на запись
- Или Docker volume `homework_uploads` успешно смонтирован
- Файлы сохраняются с GUID-именем
- Удаление вложения удаляет файл с диска
- Конфигурация размера и типа читается из appsettings.json

✅ API:
- Все endpoints реализованы
- Преподаватель видит только своих студентов и их отправки
- Студент видит только свои отправки и заданания
- Администратор видит всех (если применимо)
- Загрузка файла проверяет размер и MIME type
- Скачивание файла проверяет права доступа
- Оценка валидирует диапазон 0–100

✅ Безопасность:
- Нет LFI (path traversal) при сохранении/чтении файлов
- Недопустимые расширения отклоняются
- Права доступа проверяются на сервере (не доверять клиенту)
- Временные файлы удаляются при сбое загрузки
- MIME type проверяется по содержимому файла, а не по расширению

✅ Фронтенд:
- Создание задания работает (преподаватель видит форму)
- Отправка решения работает (студент загружает файл и текст)
- Очередь проверки показывает новые отправки
- Выставление оценки сохраняется и отображается
- История отправок видна для студента
- Пустые состояния обработаны

✅ Тесты:
- Юнит-тесты для `HomeworkService` (создание, отправка, оценивание, авторизация)
- Юнит-тесты для `FileStorageService` (валидация, сохранение, удаление)
- Интеграционные тесты для HomeworkController и SubmissionsController
- Граничные тесты: файл ровно на лимите размера, пустой текст + вложение, оценка 0 и 100
- Тесты доступа: студент не может видеть чужие отправки, преподаватель не может оценивать студентов другого преподавателя
- Все тесты проходят: `dotnet test backend.sln`

## Обязательные коммиты

Формат:
```
task/5-homework-and-grading: <описание>

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Рекомендуемый порядок коммитов:

1. `task/5-homework-and-grading: Create HomeworkEntity, SubmissionEntity and GradeEntity models`
2. `task/5-homework-and-grading: Create SubmissionAttachmentEntity and EF Core migration AddHomeworkSystem`
3. `task/5-homework-and-grading: Implement FileStorageService with validation and disk operations`
4. `task/5-homework-and-grading: Add file storage configuration to appsettings.json and docker-compose.yml`
5. `task/5-homework-and-grading: Implement HomeworkService with creation, submission and grading logic`
6. `task/5-homework-and-grading: Implement authorization checks for homework and submissions`
7. `task/5-homework-and-grading: Create HomeworkController with CRUD endpoints`
8. `task/5-homework-and-grading: Create SubmissionsController with submit, grade and download endpoints`
9. `task/5-homework-and-grading: Add unit tests for HomeworkService and FileStorageService`
10. `task/5-homework-and-grading: Add security tests for file upload and LFI prevention`
11. `task/5-homework-and-grading: Add integration tests for HomeworkController and authorization`
12. `task/5-homework-and-grading: Implement HomeworkList and HomeworkForm components`
13. `task/5-homework-and-grading: Implement SubmissionForm, FileUpload and SubmissionCard components`
14. `task/5-homework-and-grading: Implement SubmissionDetails and SubmissionQueue components`
15. `task/5-homework-and-grading: Implement GradingPanel with score input and feedback`
16. `task/5-homework-and-grading: Create API client functions for homework and submissions`
17. `task/5-homework-and-grading: Add homework and submission routes to frontend`
18. `task/5-homework-and-grading: Update README with file upload limits, formats and storage configuration`

## Что проверить перед финализацией

```powershell
# Сборка без ошибок
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\backend'
dotnet build backend.sln --no-restore

# Все тесты, включая безопасность и граничные случаи
dotnet test backend.sln --no-restore --filter "Category=HomeworkTests or Category=FileStorageTests or Category=SecurityTests"

# Frontend собирается
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\frontend'
npm run build

# Docker Compose с новыми миграциями и volume для файлов
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app'
docker compose up --build
# Проверить в логах: "Migration 'AddHomeworkSystem' applied"
# Проверить: папка uploads/homework создана в контейнере
docker compose down
```

## Обновить IMPLEMENTATION_PLAN.md

```
| 5. Домашние задания | Завершена | **Выполнено:** Созданы сущности HomeworkEntity, HomeworkSubmissionEntity, SubmissionAttachmentEntity, SubmissionGradeEntity. EF Core миграция AddHomeworkSystem. Реализован FileStorageService с валидацией размера (5 МБ), MIME type, защитой от LFI. Местное хранилище в backend/uploads/homework или Docker volume homework_uploads. Реализован HomeworkService с проверкой авторизации по студентам, курсам и пространствам. Endpoints: HomeworkController (CRUD), SubmissionsController (submit, grade, download). Оценка 0–100 с обратной связью. Фронтенд: компоненты HomeworkList, HomeworkForm, SubmissionForm, FileUpload, SubmissionQueue, GradingPanel, SubmissionDetails. Маршруты /student/homework, /teacher/homework, /teacher/submissions-queue, /admin/school/homework. Юнит-тесты для HomeworkService, FileStorageService и граничные случаи. Тесты безопасности для LFI и доступа. Все тесты пройдены. **Файлы:** Core/Entities/Homework*.cs, Core/Services/FileStorageService.cs, Infrastructure/Services/HomeworkService.cs, Presentation/Controllers/Homework*.cs, frontend/src/components/Homework/*.js, frontend/src/pages/Homework.js. **Миграции:** AddHomeworkSystem. **Ветка:** task/5-homework-and-grading с 18 коммитами. |
```

## Критический момент: безопасность файлов

- **Никогда** не используйте оригинальное имя файла для сохранения (используйте GUID)
- **Никогда** не доверяйте расширению файла; проверяйте MIME type по содержимому (напр. `System.Net.Mime.MediaTypeNames`)
- **Никогда** не сохраняйте файлы в папку, доступную из веб-приложения, без явного разрешения
- **Всегда** проверяйте путь на path traversal: `Path.GetFullPath()` и убедитесь, что результат находится внутри папки хранилища
- Удаляйте временные файлы при сбое загрузки
- Ограничивайте размер файла и типы на сервере, не на клиенте

