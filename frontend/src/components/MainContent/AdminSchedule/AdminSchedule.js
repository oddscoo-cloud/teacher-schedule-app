import React, { useState, useEffect } from "react";
import "../AdminSchedule/Styles/AdminSchedule.css";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const AdminSchedule = () => {
  const [teacherData, setTeacherData] = useState([]);
  const [selectedTeacherId, setSelectedTeacherId] = useState(null);
  const [showForm, setShowForm] = useState(false);

  const [form, setForm] = useState({
    day: "Пн",
    start: "",
    end: "",
    lessonName: "",
  });

  const fetchTeachers = async () => {
    const res = await fetch("https://localhost:7151/api/teachers", {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
    });
    const data = await res.json();
    setTeacherData(data);
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
      alert("Заповніть усі поля");
      return;
    }

    // Валідація формату часу
    const timeRegex = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/;
    if (!timeRegex.test(form.start) || !timeRegex.test(form.end)) {
      alert("Неправильний формат часу. Використовуйте HH:MM (наприклад, 09:00)");
      return;
    }

    // Проверка, что время начала раньше времени окончания
    const [startHour, startMin] = form.start.split(":").map(Number);
    const [endHour, endMin] = form.end.split(":").map(Number);
    const startTotalMin = startHour * 60 + startMin;
    const endTotalMin = endHour * 60 + endMin;

    if (startTotalMin >= endTotalMin) {
      alert("Час початку повинен бути раніше часу завершення");
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
      alert("Час перетинається з існуючим уроком цього дня");
      return;
    }

    await fetch(
      `https://localhost:7151/api/teachers/${selectedTeacherId}/lesson`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          day: form.day,
          start: form.start,
          end: form.end,
          lessonName: form.lessonName,
        }),
      }
    );

    await fetchTeachers();

    setForm({
      day: "Пн",
      start: "",
      end: "",
      lessonName: "",
    });
  };

  const deleteLesson = async (lessonId) => {
    await fetch(
      `https://localhost:7151/api/teachers/${selectedTeacherId}/lesson/${lessonId}`,
      {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );

    await fetchTeachers();
  };

  return (
    <div className="admin-container">
      {/* SIDEBAR */}
      <div className="sidebar">
        <h3>Вчителі</h3>
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

      {/* КОНТЕНТ */}
      <div className="content">
        {selectedTeacher ? (
          <>
            <h2>{selectedTeacher.name}</h2>

            <button className="toggle-form-button" onClick={() => setShowForm(!showForm)}>
              Додати урок
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
                  value={form.lessonName}
                  onChange={(e) =>
                    setForm({ ...form, lessonName: e.target.value })
                  }
                />

                <button onClick={addLesson}>Зберегти</button>
              </div>
            )}
           {/* КАЛЕНДАРЬ */}
            <div className="week-grid">
              {daysOfWeek.map((day) => {
                const lessons = (selectedTeacher.lessons || [])
                  .filter((s) => s.day === day)
                  .sort((a, b) => a.start.localeCompare(b.start));

                return (
                  <div key={day} className="day-column">
                    <h3>{day}</h3>

                    {lessons.length === 0 ? (
                      <p className="empty">Немає уроків</p>
                    ) : (
                      lessons.map((l) => (
                        <div key={l.id} className="lesson-card">
                          <strong>{l.lessonName}</strong>
                          <div>
                            {l.start} - {l.end}
                          </div>

                          <button onClick={() => deleteLesson(l.id)}>
                            x
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
          <p>Виберіть вчителя</p>
        )}
      </div>
    </div>
  );
};

export default AdminSchedule;