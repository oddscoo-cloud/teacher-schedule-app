import React from "react";
import { teachers } from "../../../data/teachers";
import "./TeacherSchedule.css";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const TeacherSchedule = ({ teacherId }) => {
  const teacher = teachers.find((t) => t.id === teacherId);

  if (!teacher) return <p>Учитель не найден</p>;

  return (
    <div className="teacher-schedule">
      <h2>Моё расписание: {teacher.name}</h2>

      <div className="week-grid">
        {daysOfWeek.map((day) => {
          const lessons = teacher.schedule.filter(
            (s) => s.day === day
          );

          return (
            <div key={day} className="day-column">
              <h3>{day}</h3>

              {lessons.length === 0 ? (
                <p className="empty">Нет уроков</p>
              ) : (
                lessons.map((lesson, index) => (
                  <div key={index} className="lesson-card">
                    <strong>{lesson.lesson}</strong>
                    <div>
                      {lesson.start} - {lesson.end}
                    </div>
                  </div>
                ))
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default TeacherSchedule;