import React, { useEffect, useState } from "react";
import "./TeacherSchedule.css";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const TeacherSchedule = () => {
  const [teacher, setTeacher] = useState(null);
  const [loading, setLoading] = useState(true);

  const teacherId = Number(localStorage.getItem("teacherId"));

  useEffect(() => {
    const fetchTeacher = async () => {
      try {
        const res = await fetch(
          `https://localhost:7151/api/teachers/${teacherId}`,
          {
            headers: {
              Authorization: `Bearer ${localStorage.getItem("token")}`,
            },
          }
        );

        if (!res.ok) throw new Error("Failed to load teacher");

        const data = await res.json();
        setTeacher(data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    if (teacherId) fetchTeacher();
  }, [teacherId]);

  if (loading) return <p>Загрузка...</p>;
  if (!teacher) return <p>Учитель не найден</p>;

  return (
    <div className="teacher-schedule">
      <h2>Моё расписание: {teacher.name}</h2>

      <div className="week-grid">
        {daysOfWeek.map((day) => {
          const lessons = (teacher.lessons || []).filter(
            (s) => s.day === day
          );

          return (
            <div key={day} className="day-column">
              <h3>{day}</h3>

              {lessons.length === 0 ? (
                <p className="empty">Нет уроков</p>
              ) : (
                lessons.map((lesson) => (
                  <div key={lesson.id} className="lesson-card">
                    <strong>{lesson.lessonName}</strong>
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