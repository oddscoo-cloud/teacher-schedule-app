import React, { useState } from "react";
import { teachers } from "../../../data/teachers";
import "../AdminSchedule/Styles/AdminSchedule.css";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const AdminSchedule = () => {
  const [teacherData, setTeacherData] = useState(teachers);
  const [selectedTeacherId, setSelectedTeacherId] = useState(null);
  const [showForm, setShowForm] = useState(false);

  const [form, setForm] = useState({
    day: "Пн",
    start: "",
    end: "",
    lesson: "",
  });

  const selectedTeacher = teacherData.find(
    (t) => t.id === selectedTeacherId
  );

  const toMinutes = (timeStr) => {
    const [h, m] = timeStr.split(":").map(Number);
    return h * 60 + m;
  };

  const isOverlapping = (newLesson) => {
    const newStart = toMinutes(newLesson.start);
    const newEnd = toMinutes(newLesson.end);

    return selectedTeacher.schedule.some((l) => {
      if (l.day !== newLesson.day) return false;
      const existingStart = toMinutes(l.start);
      const existingEnd = toMinutes(l.end);

      return (newStart < existingEnd && newEnd > existingStart);
    });
  }

  // ➕ добавление
  const addLesson = () => {
    if (!selectedTeacher) return;

    const newLesson = {
      id: Date.now() + Math.random(), 
      day: form.day,
      start: form.start,
      end: form.end,
      lesson: form.lesson,
    };

    if (isOverlapping(newLesson)) {
      alert("Этот урок пересекается с уже существующим!");
      return;
    }

    setTeacherData((prev) =>
      prev.map((t) =>
        t.id === selectedTeacherId
          ? { ...t, schedule: [...t.schedule, newLesson] }
          : t
      )
    );

    setForm({ day: "Пн", start: "", end: "", lesson: "" });
    setShowForm(false);
  };

  // ❌ удаление
  const deleteLesson = (teacherId, lessonId) => {
    setTeacherData((prev) =>
      prev.map((t) =>
        t.id === teacherId
          ? {
              ...t,
              schedule: t.schedule.filter((l) => l.id !== lessonId),
            }
          : t
      )
    );
  };

  return (
    <div className="admin-container">
      {/* Sidebar */}
      <div className="sidebar">
        <h3>Учителя</h3>
        <ul>
          {teacherData.map((t) => (
            <li
              key={t.id}
              className={selectedTeacherId === t.id ? "active" : ""}
              onClick={() => setSelectedTeacherId(t.id)}
            >
              {t.name}
            </li>
          ))}
        </ul>
      </div>

      {/* Контент */}
      <div className="content">
        {selectedTeacher ? (
          <>
            <h2>{selectedTeacher.name}</h2>

            {/* КНОПКА */}
            <button onClick={() => setShowForm(!showForm)}>
              Добавить урок
            </button>

            {/* ФОРМА */}
            {showForm && (
              <div className="lesson-form">
                <select
                  value={form.day}
                  onChange={(e) =>
                    setForm({ ...form, day: e.target.value })
                  }
                >
                  {daysOfWeek.map((d) => (
                    <option key={d}>{d}</option>
                  ))}
                </select>

                <input
                  placeholder="09:00"
                  value={form.start}
                  onChange={(e) =>
                    setForm({ ...form, start: e.target.value })
                  }
                />

                <input
                  placeholder="10:30"
                  value={form.end}
                  onChange={(e) =>
                    setForm({ ...form, end: e.target.value })
                  }
                />

                <input
                  placeholder="Предмет"
                  value={form.lesson}
                  onChange={(e) =>
                    setForm({ ...form, lesson: e.target.value })
                  }
                />

                <button onClick={addLesson}>Сохранить</button>
              </div>
            )}

            {/* КАЛЕНДАРЬ */}
            <div className="week-grid">
              {daysOfWeek.map((day) => {
                const lessons = selectedTeacher.schedule
                  .filter((s) => s.day === day)
                  .sort((a, b) => a.start.localeCompare(b.start));

                return (
                  <div key={day} className="day-column">
                    <h3>{day}</h3>

                    {lessons.length === 0 ? (
                      <p className="empty">Нет уроков</p> ) : (
                      lessons.map((l) => (
                        <div key={l.id} className="lesson-card">
                          <strong>{l.lesson}</strong>
                          <div>
                            {l.start} - {l.end}
                          </div>

                          <button
                            onClick={() => {deleteLesson(selectedTeacherId, l.id)
                              console.log("Удалён урок с id:", l.id, "у учителя с id:", selectedTeacherId);
                            }}
                          >
                            ✖
                          </button>
                        </div>
                      ))
                    )}
                  </div>
                );
              })}
            </div>
          </>
        ) : (
          <p>Выбери учителя</p>
        )}
      </div>
    </div>
  );
};

export default AdminSchedule;