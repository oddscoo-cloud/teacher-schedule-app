import React, { useEffect, useState } from "react";
import "./TeacherSchedule.css";
import API_BASE_URL from "../../../api";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const TeacherSchedule = () => {
  const [teacher, setTeacher] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;
    const fetchTeacher = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/teacher/me`, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token") || ""}`,
          },
        });

        if (!response.ok) throw new Error("Не удалось загрузить расписание.");
        const data = await response.json();
        if (active) {
          setTeacher(data);
          setError("");
        }
      } catch (requestError) {
        if (active) setError(requestError.message);
      } finally {
        if (active) setLoading(false);
      }
    };

    fetchTeacher();
    return () => {
      active = false;
    };
  }, []);

  if (loading) {
    return <main className="schedule-page"><p className="schedule-message">Загружаем расписание…</p></main>;
  }
  if (error) {
    return <main className="schedule-page"><p className="schedule-message schedule-error" role="alert">{error}</p></main>;
  }
  if (!teacher) {
    return <main className="schedule-page"><p className="schedule-message">Данные учителя не найдены.</p></main>;
  }

  const lessonsCount = (teacher.lessons || []).length;

  return (
    <main className="schedule-page">
      <header className="schedule-heading">
        <div>
          <p className="schedule-eyebrow">Рабочая неделя</p>
          <h1>Расписание</h1>
          <p className="schedule-subtitle">
            {teacher.name} <span aria-hidden="true">·</span> {lessonsCount} {lessonsCount === 1 ? "урок" : "уроков"} на неделе
          </p>
        </div>
      </header>

      <section className="week-grid" aria-label="Расписание на неделю">
        {daysOfWeek.map((day) => {
          const lessons = (teacher.lessons || [])
            .filter((lesson) => lesson.day === day)
            .sort((first, second) => first.start.localeCompare(second.start));

          return (
            <section key={day} className="day-column" aria-label={`${day}, ${lessons.length} уроков`}>
              <div className="day-heading">
                <h2>{day}</h2>
                <span className="day-count">{lessons.length}</span>
              </div>
              {lessons.length === 0 ? (
                <p className="empty-day">Свободный день</p>
              ) : (
                <ul className="day-lessons">
                  {lessons.map((lesson) => (
                    <li key={lesson.id} className="lesson-card">
                      <span className="lesson-time">{lesson.start}–{lesson.end}</span>
                      <strong>{lesson.lessonName}</strong>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          );
        })}
      </section>
    </main>
  );
};

export default TeacherSchedule;
