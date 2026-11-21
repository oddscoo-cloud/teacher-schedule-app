# 📖 Быстрый справочник: Промпты для реализации Задач 2–7

## Созданные файлы (8 файлов, ~118 KB)

| Файл | Размер | Описание |
|---|---|---|
| **PROMPTS_SUMMARY.md** | 15 KB | 📋 Эта сводка. Начните отсюда! |
| **PROMPTS_INDEX.md** | 15 KB | 📚 Справочник со структурой проекта и инструкциями |
| **PROMPTS_TASK_2.md** | 13 KB | 👥 Задача 2: Школа, ученики, курсы (14 коммитов) |
| **PROMPTS_TASK_3.md** | 14 KB | 💰 Задача 3: Оплаты и два баланса (13 коммитов) |
| **PROMPTS_TASK_4.md** | 18 KB | 📅 Задача 4: Календарь и повторения (16 коммитов) |
| **PROMPTS_TASK_5.md** | 19 KB | 📝 Задача 5: Домашние задания (18 коммитов) |
| **PROMPTS_TASK_6.md** | 15 KB | 📊 Задача 6: Дашборд и предупреждения (17 коммитов) |
| **PROMPTS_TASK_7.md** | 29 KB | ✅ Задача 7: Сквозная проверка (13 коммитов) |

**Всего:** ~90 обязательных коммитов по задачам 2–7

---

## 🎯 Начните отсюда!

### Шаг 1: Прочитайте эту сводку (вы здесь 👇)

### Шаг 2: Откройте PROMPTS_TASK_2.md

```bash
# Перейти в папку проекта
cd D:\teacher-schedule-app\teacher-schedule-app

# Прочитать промпт задачи 2
cat PROMPTS_TASK_2.md

# Или откройте в редакторе
code PROMPTS_TASK_2.md
```

### Шаг 3: Следуйте структуре промпта

Каждый промпт содержит:

```
1. Цель ← начните отсюда
2. Зависит от ← убедитесь, что они завершены
3. Работы ← подробный план реализации
   ├── Данные и модели
   ├── API (Backend)
   └── Фронтенд
4. Критерии готовности ← чеклист для проверки
5. Обязательные коммиты ← порядок коммитов
6. Что проверить ← команды для валидации
```

### Шаг 4: Делайте коммиты в правильном формате

```bash
git add .
git commit -m "task/2-school-and-students: Create StudentEntity and CourseEntity models

Добавлены сущности для управления учениками и курсами школы.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## 📋 Порядок реализации (обязателен)

```
✅ Задача 1: Аутентификация (завершена)
  │
  ├─→ Задача 2: Школа, ученики, курсы (14 коммитов)
  │   │
  │   ├─→ Задача 3: Оплаты и два баланса (13 коммитов)
  │   │   │
  │   │   └─→ Задача 4: Календарь и повторения (16 коммитов)
  │   │
  │   └─→ Задача 5: Домашние задания (18 коммитов) [опционально параллельно]
  │
  └─→ Задача 6: Дашборд и предупреждения (17 коммитов)
      │
      └─→ Задача 7: Сквозная проверка (13 коммитов)
```

---

## ⚠️ Критичные моменты (прочитайте!)

### Задача 2: Авторизация
```
- Преподаватель видит только своих учеников
- Администратор видит всех
- Нет межпространственного доступа
```

### Задача 3: Денежные расчёты
```
- Используйте decimal, не float
- Идемпотентность: повторная запись не дублирует
- Граница 5 часов: тестируйте ровно в 5 часов
```

### Задача 4: Часовой пояс
```
- Всё в UTC в БД
- Преобразуйте при чтении
- Используйте NodaTime или TimeZoneInfo
- Тестируйте DST переходы
```

### Задача 5: Безопасность файлов
```
- Используйте GUID-имена файлов
- Проверьте MIME type (не расширение)
- Защитите от LFI (path traversal)
- Максимум 5 МБ на сервере
```

### Задача 6: Производительность
```
- Избегайте N+1 запросов (используйте Include)
- Оптимизируйте для 100+ учеников
- Параметры читайте из конфига
```

### Задача 7: Миграции
```
- Новая БД: миграции должны работать
- Существующая БД: не теряйте старые данные
- Docker Compose: volume должен сохранять данные
```

---

## 🔧 Быстрая справка по командам

### Перед началом каждой задачи

```powershell
# Убедитесь, что нет неслеживаемых изменений
git status

# Переключитесь на новую ветку
git checkout -b task/2-school-and-students

# Или для других задач
git checkout -b task/3-payments-and-balance
git checkout -b task/4-calendar-and-lessons
# и т.д.
```

### Во время работы

```powershell
# Компилируйте и тестируйте часто
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\backend'
dotnet build backend.sln --no-restore
dotnet test backend.sln --no-restore

# Проверяйте фронтенд
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\frontend'
npm run build

# Docker Compose
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app'
docker compose up --build
docker compose down
```

### После каждого логичного блока

```bash
# Смотрите статус
git status

# Коммитьте с правильным форматом
git add .
git commit -m "task/N-feature: описание

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"

# Смотрите историю
git log --oneline -5
```

### После завершения задачи

```bash
# Слейте в main
git checkout main
git merge task/2-school-and-students

# Или создайте pull request (если используется GitHub)
git push origin task/2-school-and-students
```

---

## 📚 Порядок чтения файлов

### Первый раз (перед всеми задачами):

1. **IMPLEMENTATION_PLAN.md** — основной план проекта (прочитайте полностью)
2. **frontend/README.md** — инструкции по запуску
3. **PROMPTS_SUMMARY.md** (этот файл) — общий обзор

### Перед каждой задачей:

1. **Соответствующий PROMPTS_TASK_N.md** (например, PROMPTS_TASK_2.md)
2. **Существующий код от задачи 1** для понимания стиля и соглашений

### Если нужна справка:

1. **PROMPTS_INDEX.md** — структура проекта и инструкции
2. **Документация в начале каждого промпта** — общие требования

---

## ✅ Контрольный список для каждой задачи

### Перед началом:
- [ ] Прочитал промпт (Цель → Зависимости → Работы)
- [ ] Прочитал Критерии готовности
- [ ] Прочитал раздел о критичных моментах в этой сводке
- [ ] Убедился, что зависимости завершены
- [ ] Создал новую ветку (task/N-feature-name)

### Во время реализации:
- [ ] Реализовал Модели и Миграции
- [ ] Реализовал API (Backend)
- [ ] Реализовал Фронтенд
- [ ] Написал тесты (юнит и интеграционные)
- [ ] Делал коммиты после каждого логичного блока
- [ ] Все коммиты с правильным форматом

### Перед финализацией:
- [ ] Все сборки проходят без ошибок (`dotnet build`, `npm run build`)
- [ ] Все тесты проходят (`dotnet test backend.sln --no-restore`)
- [ ] Docker Compose запускается и миграции применяются
- [ ] Проверены критерии готовности (полный чеклист из промпта)
- [ ] Обновил IMPLEMENTATION_PLAN.md таблицу статуса
- [ ] README обновлён с новыми возможностями

---

## 🎓 Примеры коммитов

### Правильный формат

```
task/2-school-and-students: Create StudentEntity, ContactEntity and CourseEntity models

Добавлены сущности для управления учениками и курсами школы.
Реализованы связи многие-ко-многим через StudentAssignmentEntity.
Добавлены FK индексы для оптимизации запросов.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Еще примеры

```
task/3-payments-and-balance: Implement BalanceCalculationService

Реализована служба расчёта остатка занятий и денежного баланса.
Оба показателя рассчитываются независимо из PaymentEntity и LessonChargeEntity.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

```
task/4-calendar-and-lessons: Implement timezone handling with NodaTime

Добавлена работа с часовыми поясами (UTC в БД, локальное время при чтении).
Используется NodaTime для корректной обработки DST переходов.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

---

## 🔗 Файлы для быстрого доступа

```
D:\teacher-schedule-app\teacher-schedule-app\
├── PROMPTS_SUMMARY.md         ← Вы здесь
├── PROMPTS_INDEX.md           ← Справочник проекта
├── PROMPTS_TASK_2.md          ← Задача 2
├── PROMPTS_TASK_3.md          ← Задача 3
├── PROMPTS_TASK_4.md          ← Задача 4
├── PROMPTS_TASK_5.md          ← Задача 5
├── PROMPTS_TASK_6.md          ← Задача 6
├── PROMPTS_TASK_7.md          ← Задача 7
├── IMPLEMENTATION_PLAN.md     ← Основной план
├── frontend/README.md         ← Запуск фронтенда
└── backend/                   ← Существующий код (задача 1)
```

---

## 📞 Нужна помощь?

1. **Перепроверьте раздел "Работы"** в соответствующем PROMPTS_TASK_N.md
2. **Смотрите "Критерии готовности"** — подробный чеклист
3. **Изучите код задачи 1** в `backend/` и `frontend/`
4. **Прочитайте "Общие требования"** в начале каждого промпта
5. **Обновите IMPLEMENTATION_PLAN.md** с вопросом, если блокирующее

---

## 🚀 TL;DR — Начните сейчас!

```bash
# 1. Перейдите в проект
cd D:\teacher-schedule-app\teacher-schedule-app

# 2. Откройте PROMPTS_TASK_2.md
code PROMPTS_TASK_2.md

# 3. Создайте ветку
git checkout -b task/2-school-and-students

# 4. Следуйте разделу "Работы" в промпте

# 5. После каждого блока: коммит
git commit -m "task/2-...: description

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"

# 6. Проверьте критерии готовности

# 7. Слейте в main
git checkout main
git merge task/2-school-and-students
```

---

**Удачи! 🎉 Начните с PROMPTS_TASK_2.md**
