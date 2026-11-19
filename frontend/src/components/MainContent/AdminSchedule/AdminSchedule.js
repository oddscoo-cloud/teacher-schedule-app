import React, { useState, useEffect } from "react";
import "../AdminSchedule/Styles/AdminSchedule.css";
import API_BASE_URL from "../../../api";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const AdminSchedule = () => {
  const [teacherData, setTeacherData] = useState([]);
  const [selectedTeacherId, setSelectedTeacherId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [form, setForm] = useState({
    day: "Пн",
    start: "",
    end: "",
    lessonName: "",
  });

  const fetchTeachers = async () => {
    try {
      const res = await fetch(`${API_BASE_URL}/teachers`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token") || ""}`,
        },
      });
      if (!res.ok) throw new Error("Не удалось загрузить список учителей.");
      const data = await res.json();
      setTeacherData(data);
      setSelectedTeacherId((currentId) => currentId || data[0]?.id || null);
      setError("");
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTeachers();
  }, []);

  const selectedTeacher = teacherData.find(
    (t) => t.id === Number(selectedTeacherId)
  );

  const addLesson = async () => {
    if (!selectedTeacherId) return;

    if (!form.lessonName || !form.start || !form.end) {
      setError("Заполните предмет и время начала и окончания.");
      return;
    }

    // Валідація формату часу
    const timeRegex = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/;
    if (!timeRegex.test(form.start) || !timeRegex.test(form.end)) {
      setError("Укажите время в формате ЧЧ:ММ, например 09:00.");
      return;
    }

    // Проверка, что время начала раньше времени окончания
    const [startHour, startMin] = form.start.split(":").map(Number);
    const [endHour, endMin] = form.end.split(":").map(Number);
    const startTotalMin = startHour * 60 + startMin;
    const endTotalMin = endHour * 60 + endMin;

    if (startTotalMin >= endTotalMin) {
      setError("Время начала должно быть раньше времени окончания.");
      return;
    }

    // Проверка пересечения с существующими уроками
    const existingLessons = (selectedTeacher.lessons || []).filter(
      (lesson) => lesson.day === form.day
    );

    const hasConflict = existingLessons.some((lesson) => {
      const [existingStartHour, existingStartMin] = lesson.start
        .split(":")
        .map(Number);
      const [existingEndHour, existingEndMin] = lesson.end.split(":").map(Number);
      const existingStartTotalMin = existingStartHour * 60 + existingStartMin;
      const existingEndTotalMin = existingEndHour * 60 + existingEndMin;

      // Проверяем пересечение: start1 < end2 И start2 < end1
      return startTotalMin < existingEndTotalMin && existingStartTotalMin < endTotalMin;
    });

    if (hasConflict) {
      setError("Это время пересекается с другим уроком в выбранный день.");
      return;
    }

    setError("");
    try {
      const response = await fetch(
        `${API_BASE_URL}/teachers/${selectedTeacherId}/lesson`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("token") || ""}`,
          },
          body: JSON.stringify({
            day: form.day,
            start: form.start,
            end: form.end,
            lessonName: form.lessonName,
          }),
        },
      );
      if (!response.ok) throw new Error("Не удалось добавить урок. Проверьте данные и попробуйте ещё раз.");

      await fetchTeachers();
      setForm({
        day: "Пн",
        start: "",
        end: "",
        lessonName: "",
      });
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const deleteLesson = async (lessonId) => {
    try {
      const response = await fetch(
        `${API_BASE_URL}/teachers/${selectedTeacherId}/lesson/${lessonId}`,
        {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token") || ""}`,
          },
        }
      );
      if (!response.ok) throw new Error("Не удалось удалить урок. Попробуйте ещё раз.");

      await fetchTeachers();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    
    <main className="admin-container">
      <div className="sidebar">
        <h3>Учителя</h3>
        <ul>
          {teacherData.map((t) => (
            <li
              key={t.id}
            >
              <button
                type="button"
                className={selectedTeacherId === t.id ? "active" : ""}
                aria-pressed={selectedTeacherId === t.id}
                onClick={() => setSelectedTeacherId(t.id)}
              >
                {t.name}
              </button>
            </li>
          ))}
        </ul>
      </div>

      <div className="content">
        {loading && <p className="admin-message">Загружаем расписание…</p>}
        {error && <p className="admin-message admin-error" role="alert">{error}</p>}
        {selectedTeacher ? (
          <>
            <header className="admin-heading">
              <div>
                <h1>Расписание</h1>
                <p>{selectedTeacher.name}</p>
              </div>
              <button className="toggle-form-button" onClick={() => setShowForm(!showForm)}>
                {showForm ? "Закрыть форму" : "Добавить урок"}
              </button>
            </header>

            {showForm && (
              <div className="lesson-form">
                <label>
                  День
                  <select
                    value={form.day}
                    onChange={(e) => setForm({ ...form, day: e.target.value })}
                  >
                    {daysOfWeek.map((d) => <option key={d}>{d}</option>)}
                  </select>
                </label>
                <label>
                  Начало
                  <input
                    type="time"
                    value={form.start}
                    onChange={(e) => setForm({ ...form, start: e.target.value })}
                  />
                </label>
                <label>
                  Окончание
                  <input
                    type="time"
                    value={form.end}
                    onChange={(e) => setForm({ ...form, end: e.target.value })}
                  />
                </label>
                <label>
                  Предмет
                  <input
                    value={form.lessonName}
                    onChange={(e) => setForm({ ...form, lessonName: e.target.value })}
                  />
                </label>
                <button type="button" onClick={addLesson}>Сохранить</button>
              </div>
            )}
            <div className="week-grid">
              {daysOfWeek.map((day) => {
                const lessons = (selectedTeacher.lessons || [])
                  .filter((s) => s.day === day)
                  .sort((a, b) => a.start.localeCompare(b.start));

                return (
                  <div key={day} className="day-column">
                    <h3>{day}</h3>

                    {lessons.length === 0 ? (
                      <p className="empty">Нет уроков</p>
                    ) : (
                      lessons.map((l) => (
                        <div key={l.id} className="lesson-card">
                          <strong>{l.lessonName}</strong>
                          <span>{l.start}–{l.end}</span>
                          <button type="button" aria-label={`Удалить урок ${l.lessonName}`} onClick={() => deleteLesson(l.id)}>×</button>
                        </div>
                      ))
                    )}
                  </div>
                );
              })}
            </div>
          </>
        ) : (
          !loading && !error && <p className="admin-message">Учителей пока нет.</p>
        )}
      </div>
    </main>
  );
};

export default AdminSchedule;