# ✅ ПРОМПТЫ ГОТОВЫ К ИСПОЛЬЗОВАНИЮ

## 📋 Что было сделано

### 1. Анализ проекта ✅
- Прочитан `IMPLEMENTATION_PLAN.md` (313 строк) — полный план проекта
- Прочитан `frontend/README.md` (107 строк) — инструкции по запуску
- Определено состояние проекта: MVP версия 2.0, Задача 1 ✅ завершена

### 2. Написаны промпты для всех задач ✅

| № | Задача | Файл | Коммитов | Размер |
|---|---|---|---|---|
| 2 | Школа, ученики, курсы | `PROMPTS_TASK_2.md` | 14 | 13KB |
| 3 | Оплаты и два баланса | `PROMPTS_TASK_3.md` | 13 | 14KB |
| 4 | Календарь и повторения | `PROMPTS_TASK_4.md` | 16 | 18KB |
| 5 | Домашние задания | `PROMPTS_TASK_5.md` | 18 | 18KB |
| 6 | Дашборд и предупреждения | `PROMPTS_TASK_6.md` | 17 | 15KB |
| 7 | Сквозная проверка | `PROMPTS_TASK_7.md` | 13 | 29KB |

**Итого:** 91 обязательный коммит

### 3. Созданы справочные материалы ✅

| Файл | Описание | Размер |
|---|---|---|
| `INDEX.md` | Индекс файлов — быстрый поиск | 12KB |
| `README_PROMPTS.md` | Быстрый справочник для новичков | 13KB |
| `PROMPTS_SUMMARY.md` | Сводка, статистика, контрольные списки | 14KB |
| `PROMPTS_INDEX.md` | Справочник проекта и инструкции | 15KB |

---

## 📊 Статистика

```
✅ Всего файлов:            10
✅ Общий размер:            ~159KB
✅ Обязательных коммитов:    91 (14+13+16+18+17+13)
✅ Строк документации:       2000+
✅ Задач:                    7 (1 ✅ завершена, 6 в очереди)
✅ Новых файлов кода:        50+ (сущности, сервисы, компоненты, тесты)
```

---

## 🎯 Структура каждого промпта

Каждый файл `PROMPTS_TASK_N.md` содержит:

1. **Цель** — что нужно сделать
2. **Зависит от** — какие задачи должны быть завершены
3. **Работы** — подробный план реализации:
   - Данные и модели
   - API (Backend)
   - Фронтенд
4. **Критерии готовности** — полный чеклист проверки
5. **Обязательные коммиты** — рекомендуемый порядок коммитов
6. **Что проверить перед финализацией** — команды и проверки
7. **Обновить IMPLEMENTATION_PLAN.md** — шаблон для обновления статуса

---

## 🚀 Как начать

### Вариант 1: Быстрый старт (5 минут)
```bash
# 1. Откройте индекс файлов
code INDEX.md

# 2. Откройте быстрый справочник
code README_PROMPTS.md

# 3. Начните с первого промпта
code PROMPTS_TASK_2.md
```

### Вариант 2: Полный обзор (15 минут)
```bash
# 1. Прочитайте основной план
code IMPLEMENTATION_PLAN.md

# 2. Прочитайте справочник проекта
code PROMPTS_INDEX.md

# 3. Начните с первого промпта
code PROMPTS_TASK_2.md
```

---

## 📌 Ключевые моменты

### Формат коммитов (обязателен)
```
task/N-feature-name: <описание>

<Опциональное подробное описание>

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Порядок реализации (обязателен)
```
✅ Задача 1 (завершена)
  ↓
Задача 2 (14 коммитов)
  ↓
Задача 3 (13 коммитов)
  ↓
Задача 4 (16 коммитов)
  ↓
Задача 5 (18 коммитов) [опционально параллельно]
  ↓
Задача 6 (17 коммитов)
  ↓
Задача 7 (13 коммитов)
```

### Критичные моменты
- **Задача 2:** Авторизация (преподаватель видит своих, администратор видит всех)
- **Задача 3:** Денежные расчёты (decimal, идемпотентность, граница 5 часов)
- **Задача 4:** Часовой пояс (UTC в БД, NodaTime, DST переходы)
- **Задача 5:** Безопасность файлов (GUID, MIME type, защита от LFI)
- **Задача 6:** Производительность (Include, оптимизация для 100+)
- **Задача 7:** Миграции (новая БД работает, старые данные не теряются)

---

## ✅ Содержание проекта

### Задача 2: Школа, ученики, курсы (14 коммитов)
**Файл:** `PROMPTS_TASK_2.md`

Что реализовать:
- Модели: StudentEntity, ContactEntity, CourseEntity, StudentAssignmentEntity, StudentNoteEntity
- Миграция: AddStudentManagement
- API: StudentsController, CoursesController, StudentAssignmentsController, StudentNotesController
- Фронтенд: StudentList, StudentCard, StudentForm, CourseFilter, StudentNotesList

**Критичное:** Преподаватель видит только своих учеников; заметки видны только автору

---

### Задача 3: Оплаты и два баланса (13 коммитов)
**Файл:** `PROMPTS_TASK_3.md`

Что реализовать:
- Модели: PaymentEntity, StudentPricingEntity, LessonChargeEntity
- Миграция: AddPaymentSystem
- Сервис: BalanceCalculationService (остаток занятий и денежный баланс)
- API: PaymentsController
- Фронтенд: BalanceSummary, PaymentHistory, PaymentForm, BalanceWarning

**Критичное:** Используйте `decimal`; идемпотентность; граница 5 часов

---

### Задача 4: Календарь и повторения (16 коммитов)
**Файл:** `PROMPTS_TASK_4.md`

Что реализовать:
- Модели: LessonSeriesEntity, LessonEntity, LessonRescheduleEntity
- Миграция: AddLessonSystem
- Сервис: LessonService с генерацией серий, редактированием, переносами
- Часовой пояс: NodaTime или TimeZoneInfo (UTC в БД, локальное при чтении)
- API: LessonsController
- Фронтенд: Calendar (день/неделя/месяц), LessonCard, LessonForm, RescheduleModal

**Критичное:** Часовой пояс (DST); перенос не дублирует начисления

---

### Задача 5: Домашние задания (18 коммитов)
**Файл:** `PROMPTS_TASK_5.md`

Что реализовать:
- Модели: HomeworkEntity, HomeworkSubmissionEntity, SubmissionAttachmentEntity, SubmissionGradeEntity
- Миграция: AddHomeworkSystem
- Сервис: FileStorageService (валидация, безопасность)
- API: HomeworkController, SubmissionsController (submit, grade, download)
- Хранилище: `backend/uploads/homework/` (максимум 5 МБ)
- Фронтенд: HomeworkList, SubmissionForm, FileUpload, SubmissionQueue, GradingPanel

**Критичное:** Безопасность файлов (GUID, MIME type, LFI защита)

---

### Задача 6: Дашборд и предупреждения (17 коммитов)
**Файл:** `PROMPTS_TASK_6.md`

Что реализовать:
- Сервис: DashboardService с агрегацией данных
- API: GET /api/dashboard/today (занятия, очередь, финансы)
- Параметры: LowBalanceThreshold, HighDebtThreshold (appsettings.json)
- Фронтенд: DashboardPage, TodaysLessons, SubmissionsQueue, FinancialReport, StatsPanel
- Маршрут: / (главный экран после входа)

**Критичное:** N+1 запросы (Include); производительность

---

### Задача 7: Сквозная проверка и документация (13 коммитов)
**Файл:** `PROMPTS_TASK_7.md`

Что сделать:
- 7 функциональных сценариев (от создания школы до дашборда)
- Проверка миграций (на чистой и существующей БД)
- Проверка Docker Compose и адаптивности
- Обработка ошибок и пустые состояния
- Обновление документации (README)

**Критичное:** Новая БД (миграции работают); существующая (нет потери данных); Docker volume (persistence)

---

## 📚 Справочные материалы

### Для новичков
1. **[INDEX.md](INDEX.md)** — Индекс файлов и быстрый поиск
2. **[README_PROMPTS.md](README_PROMPTS.md)** — Быстрый справочник

### Для разработки
1. **[PROMPTS_INDEX.md](PROMPTS_INDEX.md)** — Справочник проекта и инструкции
2. **[PROMPTS_SUMMARY.md](PROMPTS_SUMMARY.md)** — Сводка, статистика, контрольные списки
3. **[IMPLEMENTATION_PLAN.md](../IMPLEMENTATION_PLAN.md)** — Основной план проекта

### Для каждой задачи
1. **[PROMPTS_TASK_2.md](PROMPTS_TASK_2.md)** — Задача 2
2. **[PROMPTS_TASK_3.md](PROMPTS_TASK_3.md)** — Задача 3
3. **[PROMPTS_TASK_4.md](PROMPTS_TASK_4.md)** — Задача 4
4. **[PROMPTS_TASK_5.md](PROMPTS_TASK_5.md)** — Задача 5
5. **[PROMPTS_TASK_6.md](PROMPTS_TASK_6.md)** — Задача 6
6. **[PROMPTS_TASK_7.md](PROMPTS_TASK_7.md)** — Задача 7

---

## 🔗 Файлы проекта

```
D:\teacher-schedule-app\teacher-schedule-app\
├── INDEX.md                        ← Начните отсюда!
├── README_PROMPTS.md               ← Быстрый справочник
├── PROMPTS_SUMMARY.md              ← Сводка всех задач
├── PROMPTS_INDEX.md                ← Справочник проекта
├── PROMPTS_TASK_2.md               ← Задача 2 (14 коммитов)
├── PROMPTS_TASK_3.md               ← Задача 3 (13 коммитов)
├── PROMPTS_TASK_4.md               ← Задача 4 (16 коммитов)
├── PROMPTS_TASK_5.md               ← Задача 5 (18 коммитов)
├── PROMPTS_TASK_6.md               ← Задача 6 (17 коммитов)
├── PROMPTS_TASK_7.md               ← Задача 7 (13 коммитов)
├── IMPLEMENTATION_PLAN.md          ← Основной план
├── frontend/README.md              ← Инструкции фронтенда
└── backend/                        ← Существующий код (Задача 1)
```

---

## ✅ Контрольный список перед началом

- [x] Прочитал `IMPLEMENTATION_PLAN.md` (узнал о проекте)
- [x] Прочитал `frontend/README.md` (узнал о структуре)
- [x] Определил текущее состояние (Задача 1 ✅ завершена)
- [x] Получил промпты для всех задач 2–7
- [x] Получил справочные материалы и инструкции
- [x] Готов начать с Задачи 2

---

## 🎓 Дополнительная информация

### Версионирование
Рекомендуется создавать теги после завершения каждой задачи:
```bash
git tag -a v0.2.0 -m "Task 2: School and Students"
git tag -a v0.3.0 -m "Task 3: Payments and Balance"
# ... и т.д.
git tag -a v1.0.0 -m "MVP v2.0 complete"
```

### Параллельное выполнение
Задачи 2 и 5 могут выполняться параллельно (разные зависимости), но рекомендуется следовать основному порядку.

### Вопросы и блокеры
Если что-то непонятно:
1. Перепроверьте раздел "Работы" в соответствующем промпте
2. Смотрите "Критерии готовности" — подробный чеклист
3. Изучите код Задачи 1 (завершена)
4. Прочитайте "Общие требования" в начале каждого промпта

---

## 🚀 TL;DR

```bash
# 1. Начните с этого файла
cat THIS_FILE.md

# 2. Откройте индекс
code INDEX.md

# 3. Откройте первый промпт
code PROMPTS_TASK_2.md

# 4. Следуйте структуре (Цель → Зависимости → Работы)

# 5. Делайте коммиты с правильным форматом
git commit -m "task/2-school-and-students: description

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"

# 6. Проверьте критерии готовности

# 7. Слейте в main и переходите к Задаче 3
```

---

**Удачи в реализации! 🎉**

Все готово. Начните с **[INDEX.md](INDEX.md)** или **[PROMPTS_TASK_2.md](PROMPTS_TASK_2.md)**
