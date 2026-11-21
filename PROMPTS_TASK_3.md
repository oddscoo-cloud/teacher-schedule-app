# Промпт 3 — Оплаты и два баланса

## Цель
Реализовать систему учёта оплат и расчёта двух независимых показателей: остатка оплаченных занятий и денежного долга/предоплаты в гривнах (UAH).

## Зависит от
- Задача 1 (завершена): аутентификация и рабочие пространства
- Задача 2 (завершена): модели учеников и назначения преподавателей

## Работы

### Данные и модели
1. **PaymentEntity** — журнал оплат:
   - `Id` (GUID)
   - `StudentId` (FK на StudentEntity)
   - `TeacherId` (FK на TeacherEntity)
   - `WorkspaceId` (FK на WorkspaceEntity)
   - `Amount` (decimal в UAH)
   - `LessonsPaid` (int — количество оплаченных занятий)
   - `PaymentDate` (DateTime)
   - `Notes` (string — опционально, комментарий)
   - `CreatedAt` (DateTime)
   - `CreatedBy` (UserId для аудита)

2. **StudentPricingEntity** — тарифная сетка ученика:
   - `Id` (GUID)
   - `StudentId` (FK)
   - `TeacherId` (FK)
   - `WorkspaceId` (FK)
   - `StandardPrice` (decimal в UAH за одно занятие)
   - `EffectiveFrom` (DateTime — дата начала действия)
   - `EffectiveTo` (DateTime или NULL — если цена активна)
   - `Reason` (string — причина изменения)
   - `CreatedAt` (DateTime)

3. **LessonChargeEntity** — начисления за проведённые/отменённые занятия:
   - `Id` (GUID)
   - `StudentId` (FK)
   - `LessonId` (FK на LessonEntity из задачи 4)
   - `Type` (enum: "Completed", "LateCancel" — отмена менее 5 часов)
   - `Amount` (decimal в UAH)
   - `LessonsCharged` (int — количество списанных занятий)
   - `ChargedAt` (DateTime)
   - `IsPaid` (bool — рассчитано ли в текущий долг)
   - `Reference` (string — для идемпотентности; идентификатор занятия+операция)

4. **Миграция EF Core:**
   - Создать миграцию `AddPaymentSystem`
   - Добавить индексы на StudentId, TeacherId, PaymentDate
   - Добавить уникальный индекс на (LessonId, Type) в LessonChargeEntity для предотвращения дубликатов

### API (Backend)

1. **PaymentsController** (требует авторизации):
   - `GET /api/students/{studentId}/payments` — история оплат ученика
   - `POST /api/students/{studentId}/payments` — записать оплату (администратор или преподаватель)
   - `GET /api/students/{studentId}/balance` — текущий остаток занятий и денежный долг
   - `GET /api/students/{studentId}/balance-history` — история изменения балансов (опционально)
   - `PUT /api/students/{studentId}/pricing/standard-price` — установить стандартную цену

2. **BalanceCalculationService** (Backend сервис):
   - `GetLessonBalance(studentId, teacherId, workspaceId)` → int (остаток занятий)
   - `GetMoneyBalance(studentId, teacherId, workspaceId)` → decimal (долг или предоплата)
   - `RecordPayment(studentId, amount, lessonsPaid, teacherId, workspaceId)` → PaymentEntity
   - `ChargeLesson(studentId, lessonId, type, teacherId, workspaceId)` → LessonChargeEntity (idempotent)
   - `GetStudentPricingHistory(studentId, teacherId)` → List<StudentPricingEntity>

3. **Расчётная логика (критична для точности):**

   **Остаток занятий = Σ(лессонс_пайд) - Σ(лессонс_чарджед)**
   - Суммируются все оплаченные занятия (PaymentEntity.LessonsPaid)
   - Вычитаются все списанные занятия (LessonChargeEntity.LessonsCharged, где IsPaid=true)

   **Денежный долг = Σ(лессон_чарджес) - Σ(пейментс)**
   - Суммируются все начисления за проведённые/поздние отмены (LessonChargeEntity.Amount)
   - Вычитаются зарегистрированные оплаты (PaymentEntity.Amount)
   - Положительное значение = долг, отрицательное = предоплата

   **Правила начисления:**
   - Занятие проведено → начислить StandardPrice и 1 занятие
   - Отмена менее 5 часов → начислить StandardPrice и 1 занятие
   - Отмена за 5+ часов → ничего не начислять
   - Перенос на другую дату → ничего не начислять (занятие остаётся активным)

### Фронтенд
1. **Компоненты:**
   - `BalanceSummary` — показывает остаток занятий и денежный долг (цветовые коды: зелено=предоплата, красно=долг)
   - `PaymentHistory` — таблица оплат с датой, суммой, количеством занятий
   - `PaymentForm` — форма для записи оплаты (только администратор/преподаватель)
   - `PricingForm` — форма для установки стандартной цены ученика (история изменений)
   - `BalanceWarning` — предупреждение, если остаток занятий низкий или большой долг

2. **Маршруты:**
   - `/teacher/students/{id}/payments` — история платежей и баланс ученика
   - `/teacher/students/{id}/pricing` — управление тарифом ученика
   - `/admin/school/payments` — общая статистика платежей школы

3. **API-клиент:**
   - Расширить `frontend/src/api.js`: `fetchPayments`, `recordPayment`, `getBalance`, `updateStudentPrice`

## Критерии готовности

✅ Модели и миграции:
- PaymentEntity, StudentPricingEntity, LessonChargeEntity созданы
- Миграция `AddPaymentSystem` применяется успешно
- Все индексы добавлены
- Уникальный индекс (LessonId, Type) предотвращает дубликаты

✅ Расчёты:
- Остаток занятий вычисляется корректно (оплачено минус списано)
- Денежный долг вычисляется без потери точности (используется decimal)
- Идемпотентность: повторная запись одного платежа/начисления не дублирует данные
- Граничные случаи обработаны: пустой баланс, отрицательный долг (предоплата), нулевой остаток

✅ API:
- Все endpoints реализованы и возвращают корректные данные
- Серверная авторизация: преподаватель видит только своих учеников, администратор видит всех
- Запись оплаты проверяет корректность суммы и количества занятий
- Изменение цены создаёт новую запись в StudentPricingEntity, не перезаписывает старую

✅ Данные:
- История платежей полна и неизменяема (append-only логика)
- История цен полна (EffectiveTo заполнена при создании новой цены)
- Расчёты согласованы между двумя балансами

✅ Фронтенд:
- Баланс отображается в реальном времени
- История платежей читается без ошибок
- Форма оплаты валидирует сумму и количество занятий
- Предупреждения о низком остатке или большом долге видны

✅ Тесты:
- Юнит-тесты для `BalanceCalculationService` (расчёт остатка, долга, граничные случаи)
- Юнит-тесты для `PaymentService` (запись оплаты, идемпотентность, авторизация)
- Интеграционные тесты для PaymentsController (доступ, расчёты, история)
- Граничные тесты: отмена точно в 5 часов, отмена в -1 сек до лимита, нулевой баланс
- Все тесты проходят: `dotnet test backend.sln`

## Обязательные коммиты

Выполняй коммиты после каждого логичного блока. Формат:

```
task/3-payments-and-balance: <описание>

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Рекомендуемый порядок коммитов:

1. `task/3-payments-and-balance: Create PaymentEntity, StudentPricingEntity and LessonChargeEntity models`
2. `task/3-payments-and-balance: Create EF Core migration AddPaymentSystem with indices`
3. `task/3-payments-and-balance: Implement BalanceCalculationService with lesson and money balance logic`
4. `task/3-payments-and-balance: Implement PaymentService with idempotent recording and pricing history`
5. `task/3-payments-and-balance: Create PaymentsController with CRUD and balance endpoints`
6. `task/3-payments-and-balance: Add unit tests for BalanceCalculationService and boundary cases`
7. `task/3-payments-and-balance: Add integration tests for PaymentsController and authorization`
8. `task/3-payments-and-balance: Implement BalanceSummary and PaymentHistory components`
9. `task/3-payments-and-balance: Implement PaymentForm and PricingForm components`
10. `task/3-payments-and-balance: Implement BalanceWarning component with color coding`
11. `task/3-payments-and-balance: Create API client functions for Payments and Balance`
12. `task/3-payments-and-balance: Add payment management routes to frontend`
13. `task/3-payments-and-balance: Update README with payment system and financial rules`

## Что проверить перед финализацией

```powershell
# Сборка без ошибок
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\backend'
dotnet build backend.sln --no-restore

# Все тесты проходят, включая граничные случаи
dotnet test backend.sln --no-restore --filter "Category=PaymentTests or Category=BalanceTests"

# Frontend собирается
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app\frontend'
npm run build

# Docker Compose с новыми миграциями
Set-Location 'D:\teacher-schedule-app\teacher-schedule-app'
docker compose up --build
# Проверить в логах: "Migration 'AddPaymentSystem' applied"
docker compose down
```

## Обновить IMPLEMENTATION_PLAN.md

```
| 3. Оплаты и два баланса | Завершена | **Выполнено:** Созданы сущности PaymentEntity, StudentPricingEntity, LessonChargeEntity. Реализована EF Core миграция AddPaymentSystem. Реализован BalanceCalculationService с точным расчётом остатка занятий и денежного долга в UAH. Идемпотентная запись платежей и начисления за проведённые/поздно отменённые занятия. Реализован PaymentService и PaymentsController. Фронтенд: компоненты BalanceSummary, PaymentHistory, PaymentForm, PricingForm, BalanceWarning. Маршруты /teacher/students/{id}/payments, /admin/school/payments. Юнит-тесты покрывают граничные случаи (ровно 5 часов, предоплата, нулевой баланс). Все тесты пройдены. **Файлы:** Core/Entities/Payment*.cs, Core/Services/BalanceCalculationService.cs, Infrastructure/Services/PaymentService.cs, Presentation/Controllers/PaymentsController.cs, frontend/src/components/Balance*.js, frontend/src/pages/Payments.js. **Миграции:** AddPaymentSystem. **Ветка:** task/3-payments-and-balance с 13 коммитами. |
```

## Критический момент: точность денежных расчётов

- Всегда используйте `decimal` для денег, а не `float` или `double`
- Вычисления денежных средств проводите на уровне БД (SUM, WHERE) или после получения данных в памяти
- Исключите потерю точности: 0.1 + 0.2 ≠ 0.3 в float/double
- Проверьте, что денежный долг вычисляется одинаково при прямом/обратном порядке платежей
- Тестируйте граничные случаи: платёж 1 копейка, долг ровно 0, очень большие суммы

