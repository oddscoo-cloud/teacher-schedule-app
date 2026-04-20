import React, { useState } from "react";
import { teachers } from "../../../data/teachers";
import "./Styles/AdminSchedule.css";
import AdminScheduleSidebar from "./components/AdminScheduleSibebar";
import AdminScheduleTable from "./components/AdminScheduleTable";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const AdminSchedule = () => {
  const [teacherData, setTeacherData] = useState(teachers);
  const [selectedTeacher, setSelectedTeacher] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    day: "Пн",
    start: "",
    end: "",
    lesson: "",
  });

  const addLesson = () => {
    if (!selectedTeacher) return;

    const newLesson = {
      day: form.day,
      start: form.start,
      end: form.end,
      lesson: form.lesson,
    };

    if (isOverlapping(selectedTeacher.schedule, newLesson)) {
      alert("Ошибка: урок пересекается с другим!");
      return;
    }

    setTeacherData((prev) =>
      prev.map((t) =>
        t.id === selectedTeacher.id
          ? { ...t, schedule: [...t.schedule, newLesson] }
          : t
      )
    );

    setSelectedTeacher((prev) => ({
      ...prev,
      schedule: [...prev.schedule, newLesson],
    }));

    setForm({ day: "Пн", start: "", end: "", lesson: "" });
    setShowForm(false);
  };

  const toMinutes = (time) => {
    const [hours, minutes] = time.split(":").map(Number);
    return hours * 60 + minutes;
  }

  const isOverlapping = (existingLessons, newLesson) => {
    const newStart = toMinutes(newLesson.start);
    const newEnd = toMinutes(newLesson.end);

    return existingLessons.some((lesson) => {
      if (lesson.day !== newLesson.day) return false;

      const start = toMinutes(lesson.start);
      const end = toMinutes(lesson.end);

      return newStart < end && newEnd > start;
    });
  };

  return (
    <div className="admin-schedule-container">
      <div className="admin-schedule-content">
        {/* Sidebar с именами учителей */}
        <AdminScheduleSidebar
          teachers={teachers}
          selectedTeacher={selectedTeacher}
          onSelectTeacher={setSelectedTeacher}
        />

        {/* Основное содержимое — таблица расписания */}
        <AdminScheduleTable
          selectedTeacher={selectedTeacher}
          daysOfWeek={daysOfWeek}
        />

        <button onClick={() => setShowForm(!showForm)}>
          Добавить урок
        </button>

        {showForm && (
          <div className="lesson-form">
            <select
              value={form.day}
              onChange={(e) => setForm({ ...form, day: e.target.value })}
            >
              <option>Пн</option>
              <option>Вт</option>
              <option>Ср</option>
              <option>Чт</option>
              <option>Пт</option>
            </select>

            <input
              placeholder="Начало (09:00)"
              value={form.start}
              onChange={(e) => setForm({ ...form, start: e.target.value })}
            />

            <input
              placeholder="Конец (10:30)"
              value={form.end}
              onChange={(e) => setForm({ ...form, end: e.target.value })}
            />

            <input
              placeholder="Предмет"
              value={form.lesson}
              onChange={(e) => setForm({ ...form, lesson: e.target.value })}
            />

            <button onClick={addLesson}>Сохранить</button>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminSchedule;