import React, { useCallback, useEffect, useRef, useState } from "react";
import {
  adjustStudentPayment,
  fetchStudentFinance,
  recordStudentPayment,
  setStudentPrice,
} from "../../api";

const newIdempotencyKey = () =>
  window.crypto?.randomUUID?.() ||
  `finance-${Date.now()}-${Math.random().toString(36).slice(2)}`;

const money = (amount) =>
  `${Number(amount).toFixed(2)} ${"₴"}`;

const StudentFinancePanel = ({ studentId }) => {
  const [finance, setFinance] = useState(null);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [price, setPrice] = useState("");
  const [payment, setPayment] = useState({
    amount: "",
    paidLessonCount: "1",
    paidAt: new Date().toISOString().slice(0, 10),
  });
  const [adjustments, setAdjustments] = useState({});
  const paymentKey = useRef(newIdempotencyKey());
  const adjustmentKeys = useRef({});

  const loadFinance = useCallback(async () => {
    const data = await fetchStudentFinance(studentId);
    setFinance(data);
  }, [studentId]);

  useEffect(() => {
    let active = true;
    fetchStudentFinance(studentId)
      .then((data) => {
        if (active) setFinance(data);
      })
      .catch((requestError) => {
        if (active) setError(requestError.message);
      });
    return () => {
      active = false;
    };
  }, [studentId]);

  const submit = async (operation) => {
    setError("");
    setSaving(true);
    try {
      await operation();
      await loadFinance();
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setSaving(false);
    }
  };

  const savePrice = (event) => {
    event.preventDefault();
    submit(async () => {
      await setStudentPrice(studentId, { amount: Number(price) });
      setPrice("");
    });
  };

  const savePayment = (event) => {
    event.preventDefault();
    submit(async () => {
      await recordStudentPayment(studentId, {
        amount: Number(payment.amount),
        paidLessonCount: Number(payment.paidLessonCount),
        paidAt: new Date(`${payment.paidAt}T12:00:00`).toISOString(),
        idempotencyKey: paymentKey.current,
      });
      paymentKey.current = newIdempotencyKey();
      setPayment({
        amount: "",
        paidLessonCount: "1",
        paidAt: new Date().toISOString().slice(0, 10),
      });
    });
  };

  const saveAdjustment = (event, paymentId) => {
    event.preventDefault();
    const values = adjustments[paymentId] || {};
    adjustmentKeys.current[paymentId] ||= newIdempotencyKey();
    submit(async () => {
      await adjustStudentPayment(studentId, paymentId, {
        amountDelta: Number(values.amountDelta || 0),
        paidLessonCountDelta: Number(values.paidLessonCountDelta || 0),
        reason: values.reason || "",
        idempotencyKey: adjustmentKeys.current[paymentId],
      });
      delete adjustmentKeys.current[paymentId];
      setAdjustments((current) => ({ ...current, [paymentId]: {} }));
    });
  };

  const updateAdjustment = (paymentId, field, value) =>
    setAdjustments((current) => ({
      ...current,
      [paymentId]: { ...current[paymentId], [field]: value },
    }));

  if (!finance && !error) return <p>Загрузка финансов...</p>;

  return (
    <section className="student-finance">
      <h2>Оплаты и балансы</h2>
      {error && <p role="alert" className="student-error">{error}</p>}
      {finance && (
        <>
          <dl className="finance-summary">
            <dt>Стандартная стоимость</dt>
            <dd>{finance.standardPrice == null ? "Не задана" : money(finance.standardPrice)}</dd>
            <dt>Остаток оплаченных занятий</dt>
            <dd>{finance.lessonBalance}</dd>
            <dt>Денежный баланс (долг + / предоплата −)</dt>
            <dd>{money(finance.moneyBalance)}</dd>
          </dl>
          {!finance.lessonChargesIntegrated && (
            <p className="student-inline-warning">
              Списания уроков пока не подключены: до появления модели календарного занятия
              значения показывают только зарегистрированные оплаты. Тарифы и история
              оплат сохраняются; баланс будет учитывать проведённые и поздно отменённые
              уроки после интеграции календаря.
            </p>
          )}

          <form className="finance-form" onSubmit={savePrice}>
            <h3>Изменить стандартную цену</h3>
            <label>
              Цена занятия (UAH)
              <input
                type="number"
                min="0"
                step="0.01"
                required
                value={price}
                onChange={(event) => setPrice(event.target.value)}
              />
            </label>
            <button type="submit" disabled={saving}>Сохранить тариф</button>
          </form>

          <form className="finance-form" onSubmit={savePayment}>
            <h3>Зарегистрировать оплату</h3>
            <label>
              Сумма (UAH)
              <input
                type="number"
                min="0.01"
                step="0.01"
                required
                value={payment.amount}
                onChange={(event) => setPayment({ ...payment, amount: event.target.value })}
              />
            </label>
            <label>
              Оплачено занятий
              <input
                type="number"
                min="0"
                step="1"
                required
                value={payment.paidLessonCount}
                onChange={(event) => setPayment({ ...payment, paidLessonCount: event.target.value })}
              />
            </label>
            <label>
              Дата оплаты
              <input
                type="date"
                required
                value={payment.paidAt}
                onChange={(event) => setPayment({ ...payment, paidAt: event.target.value })}
              />
            </label>
            <button type="submit" disabled={saving}>Добавить оплату</button>
          </form>

          <h3>История тарифов</h3>
          {finance.priceHistory.length ? (
            <ul className="finance-history">
              {finance.priceHistory.map((entry) => (
                <li key={entry.id}>
                  {money(entry.amount)} — с {new Date(entry.effectiveFrom).toLocaleString()}
                </li>
              ))}
            </ul>
          ) : <p>Тариф ещё не задан.</p>}

          <h3>Начисления занятий</h3>
          {finance.charges?.length ? (
            <ul className="finance-history">
              {finance.charges.map((charge) => (
                <li key={charge.id}>
                  {charge.type === "LateCancellation" ? "Поздняя отмена" : "Проведено"} ·{" "}
                  {new Date(charge.lessonStartsAt).toLocaleString()} · {money(charge.amount)}
                </li>
              ))}
            </ul>
          ) : <p>Начислений занятий пока нет.</p>}

          <h3>История оплат и корректировок</h3>
          {finance.payments.length ? finance.payments.map((entry) => (
            <article className="finance-payment" key={entry.id}>
              <p>
                {new Date(entry.paidAt).toLocaleDateString()} · {money(entry.amount)} ·
                {" "}{entry.paidLessonCount} занятий
              </p>
              <p>
                Итого после корректировок: {money(entry.netAmount)},{" "}
                {entry.netPaidLessonCount} занятий
              </p>
              {entry.adjustments.map((adjustment) => (
                <p className="finance-adjustment" key={adjustment.id}>
                  {adjustment.reason}: {money(adjustment.amountDelta)};{" "}
                  {adjustment.paidLessonCountDelta} занятий
                </p>
              ))}
              <form className="finance-form finance-adjustment-form" onSubmit={(event) => saveAdjustment(event, entry.id)}>
                <strong>Добавить корректировку / возврат</strong>
                <label>
                  Изменение суммы (UAH)
                  <input
                    type="number"
                    step="0.01"
                    value={adjustments[entry.id]?.amountDelta || ""}
                    onChange={(event) => updateAdjustment(entry.id, "amountDelta", event.target.value)}
                  />
                </label>
                <label>
                  Изменение числа занятий
                  <input
                    type="number"
                    step="1"
                    value={adjustments[entry.id]?.paidLessonCountDelta || ""}
                    onChange={(event) => updateAdjustment(entry.id, "paidLessonCountDelta", event.target.value)}
                  />
                </label>
                <label>
                  Причина
                  <input
                    required
                    maxLength="500"
                    value={adjustments[entry.id]?.reason || ""}
                    onChange={(event) => updateAdjustment(entry.id, "reason", event.target.value)}
                  />
                </label>
                <button type="submit" disabled={saving}>Сохранить корректировку</button>
              </form>
            </article>
          )) : <p>Оплат ещё нет.</p>}
        </>
      )}
    </section>
  );
};

export default StudentFinancePanel;
