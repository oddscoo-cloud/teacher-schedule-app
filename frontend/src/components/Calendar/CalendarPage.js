import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import {
  createLesson,
  createSeries,
  fetchCourses,
  fetchLessonDetails,
  fetchLessons,
  fetchStudents,
  fetchLessonTeachers,
  rescheduleLesson,
  updateLesson,
  updateLessonSeries,
  updateLessonStatus,
} from "../../api";
import "./CalendarPage.css";

const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
const pad = (value) => String(value).padStart(2, "0");
const inputDate = (date) => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
const inputTime = (date) => `${pad(date.getHours())}:${pad(date.getMinutes())}`;
const startOfDay = (date) => new Date(date.getFullYear(), date.getMonth(), date.getDate());
const formatDate = (date) => new Intl.DateTimeFormat(undefined, { weekday: "long", month: "long", day: "numeric" }).format(date);
const formatClock = (date) => new Intl.DateTimeFormat(undefined, { hour: "2-digit", minute: "2-digit" }).format(date);
const weekdays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

function rangeFor(date, view) {
  if (view === "day") {
    const from = startOfDay(date);
    return [from, new Date(from.getFullYear(), from.getMonth(), from.getDate() + 1)];
  }
  if (view === "month") {
    return [new Date(date.getFullYear(), date.getMonth(), 1), new Date(date.getFullYear(), date.getMonth() + 1, 1)];
  }
  const from = startOfDay(date);
  const mondayOffset = (from.getDay() + 6) % 7;
  from.setDate(from.getDate() - mondayOffset);
  return [from, new Date(from.getFullYear(), from.getMonth(), from.getDate() + 7)];
}

function asLocalDateTime(value) {
  const date = new Date(value);
  return { date: inputDate(date), time: inputTime(date) };
}

function CalendarPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const { id: routeLessonId } = useParams();
  const isAdmin = location.pathname.startsWith("/admin/");
  const routeIsNew = location.pathname.endsWith("/lessons/new");
  const [view, setView] = useState(new URLSearchParams(location.search).get("view") || "week");
  const [date, setDate] = useState(() => {
    const queryDate = new URLSearchParams(location.search).get("date");
    return queryDate ? new Date(`${queryDate}T12:00:00`) : new Date();
  });
  const [studentFilter, setStudentFilter] = useState(new URLSearchParams(location.search).get("student") || "");
  const [courseFilter, setCourseFilter] = useState("");
  const [lessons, setLessons] = useState([]);
  const [students, setStudents] = useState([]);
  const [courses, setCourses] = useState([]);
  const [teachers, setTeachers] = useState([]);
  const [selected, setSelected] = useState(null);
  const [editor, setEditor] = useState(null);
  const [reschedule, setReschedule] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const [from, to] = useMemo(() => rangeFor(date, view), [date, view]);
  const reload = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [items, studentItems, courseItems, teacherItems] = await Promise.all([
        fetchLessons({
          from: from.toISOString(),
          to: to.toISOString(),
          studentId: studentFilter || undefined,
          courseId: courseFilter || undefined,
        }),
        fetchStudents(),
        fetchCourses(),
        isAdmin ? fetchLessonTeachers() : Promise.resolve([]),
      ]);
      setLessons(items);
      setStudents(studentItems);
      setCourses(courseItems);
      setTeachers(teacherItems);
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  }, [from, to, studentFilter, courseFilter, isAdmin]);

  useEffect(() => { reload(); }, [reload]);

  useEffect(() => {
    if (routeIsNew) {
      setEditor({ lesson: null, series: false });
    } else if (routeLessonId) {
      fetchLessonDetails(routeLessonId)
        .then(setSelected)
        .catch((requestError) => setError(requestError.message));
    }
  }, [routeIsNew, routeLessonId]);

  const changeDate = (amount) => {
    const next = new Date(date);
    if (view === "month") next.setMonth(next.getMonth() + amount);
    else if (view === "week") next.setDate(next.getDate() + amount * 7);
    else next.setDate(next.getDate() + amount);
    setDate(next);
  };

  const openNew = (selectedDate = date) => {
    setSelected(null);
    setEditor({ lesson: null, series: false, initialDate: selectedDate });
    if (routeIsNew) navigate(isAdmin ? "/admin/school/calendar" : "/teacher/calendar", { replace: true });
  };

  const act = async (operation) => {
    setBusy(true);
    setError("");
    try {
      await operation();
      setSelected(null);
      setReschedule(null);
      setEditor(null);
      await reload();
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setBusy(false);
    }
  };

  const saveLesson = async (event) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const localStart = new Date(`${form.get("date")}T${form.get("time")}`);
    const base = {
      studentId: Number(form.get("studentId")),
      courseId: form.get("courseId") ? Number(form.get("courseId")) : null,
      teacherId: isAdmin && form.get("teacherId") ? Number(form.get("teacherId")) : null,
      title: form.get("title"),
      description: form.get("description") || null,
      durationMinutes: Number(form.get("durationMinutes")),
      timezone,
    };
    const startTime = `${form.get("time")}:00`;
    const isSeries = form.get("series") === "on";
    await act(async () => {
      if (editor.lesson?.seriesId && form.get("applySeries") === "on") {
        await updateLessonSeries(editor.lesson.seriesId, {
          dayOfWeek: Number(form.get("dayOfWeek")),
          startTime,
          durationMinutes: base.durationMinutes,
          title: base.title,
          description: base.description,
          timezone,
        });
      } else if (isSeries) {
        const startDate = new Date(`${form.get("date")}T12:00:00`);
        await createSeries({
          ...base,
          dayOfWeek: Number(form.get("dayOfWeek")),
          startTime,
          startDate: inputDate(startDate),
          endDate: form.get("endDate") || null,
        });
      } else if (editor.lesson) {
        await updateLesson(editor.lesson.id, {
          title: base.title,
          description: base.description,
          courseId: base.courseId,
          scheduledAt: localStart.toISOString(),
          durationMinutes: base.durationMinutes,
          timezone,
          notes: form.get("notes") || null,
        });
      } else {
        await createLesson({ ...base, scheduledAt: localStart.toISOString() });
      }
    });
  };

  const saveReschedule = async (event) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const start = new Date(`${form.get("date")}T${form.get("time")}`);
    await act(() => rescheduleLesson(reschedule.id, start.toISOString(), form.get("reason") || null));
  };

  const filtered = lessons;
  const heading = view === "month"
    ? new Intl.DateTimeFormat(undefined, { month: "long", year: "numeric" }).format(date)
    : view === "day" ? formatDate(date) : `${formatDate(from)} – ${formatDate(new Date(to.getTime() - 86400000))}`;
  const days = useMemo(() => {
    if (view === "day") return [startOfDay(date)];
    if (view === "month") {
      const first = new Date(date.getFullYear(), date.getMonth(), 1);
      const start = new Date(first);
      start.setDate(start.getDate() - (start.getDay() + 6) % 7);
      return Array.from({ length: 42 }, (_, index) => new Date(start.getFullYear(), start.getMonth(), start.getDate() + index));
    }
    return Array.from({ length: 5 }, (_, index) => new Date(from.getFullYear(), from.getMonth(), from.getDate() + index));
  }, [date, from, view]);

  return (
    <main className="calendar-page">
      <header className="calendar-header">
        <div>
          <p className="calendar-eyebrow">Планирование занятий</p>
          <h1>Календарь</h1>
          <p className="calendar-subtitle">{heading} · {timezone}</p>
        </div>
        <button type="button" className="calendar-primary" onClick={() => openNew(date)}>+ Новое занятие</button>
      </header>

      <section className="calendar-toolbar" aria-label="Настройки календаря">
        <div className="calendar-navigation">
          <button type="button" onClick={() => changeDate(-1)} aria-label="Предыдущий период">‹</button>
          <button type="button" onClick={() => setDate(new Date())}>Сегодня</button>
          <button type="button" onClick={() => changeDate(1)} aria-label="Следующий период">›</button>
        </div>
        <div className="calendar-filters">
          <select aria-label="Ученик" value={studentFilter} onChange={(event) => setStudentFilter(event.target.value)}>
            <option value="">Все ученики</option>
            {students.map((student) => <option key={student.id} value={student.id}>{student.firstName} {student.lastName}</option>)}
          </select>
          <select aria-label="Курс" value={courseFilter} onChange={(event) => setCourseFilter(event.target.value)}>
            <option value="">Все курсы</option>
            {courses.map((course) => <option key={course.id} value={course.id}>{course.name}</option>)}
          </select>
          <div className="calendar-view-switch" role="group" aria-label="Вид календаря">
            {["day", "week", "month"].map((item) => (
              <button type="button" key={item} className={view === item ? "active" : ""} onClick={() => setView(item)}>
                {{ day: "День", week: "Неделя", month: "Месяц" }[item]}
              </button>
            ))}
          </div>
        </div>
      </section>

      {error && <p className="calendar-error" role="alert">{error}</p>}
      {loading ? <p className="calendar-empty">Загружаем занятия…</p> : filtered.length === 0 ? null : null}
      {!loading && view === "month" ? (
        <section className="month-grid" aria-label="Месяц">
          {["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"].map((name) => <strong className="month-weekday" key={name}>{name}</strong>)}
          {days.map((day) => {
            const items = filtered.filter((lesson) => new Date(lesson.scheduledAt).toDateString() === day.toDateString());
            return (
              <button type="button" key={day.toISOString()} className={`month-day${day.getMonth() !== date.getMonth() ? " outside" : ""}`} onClick={() => { setDate(day); setView("day"); }}>
                <span>{day.getDate()}</span>
                {items.slice(0, 3).map((lesson) => <small key={lesson.id} className={`month-dot status-${lesson.status.toLowerCase()}`}>{formatClock(new Date(lesson.scheduledAt))} {lesson.title}</small>)}
                {items.length > 3 && <small>+{items.length - 3} ещё</small>}
              </button>
            );
          })}
        </section>
      ) : !loading ? (
        <section className={`calendar-days ${view === "day" ? "single-day" : ""}`} aria-label={view === "day" ? "День" : "Неделя"}>
          {days.map((day) => {
            const items = filtered.filter((lesson) => new Date(lesson.scheduledAt).toDateString() === day.toDateString());
            return (
              <section key={day.toISOString()} className="calendar-day-column">
                <header className="calendar-day-heading"><h2>{formatDate(day)}</h2><span>{items.length}</span></header>
                {items.length ? (
                  <ul className="calendar-lesson-list">
                    {items.sort((a, b) => new Date(a.scheduledAt) - new Date(b.scheduledAt)).map((lesson) => (
                      <li key={lesson.id}>
                        <button type="button" className={`calendar-lesson status-${lesson.status.toLowerCase()}`} onClick={() => setSelected(lesson)}>
                          <time>{formatClock(new Date(lesson.scheduledAt))}</time>
                          <strong>{lesson.title}</strong>
                          <span>{lesson.studentName}</span>
                          <small>{lesson.status === "Scheduled" ? "Запланировано" : lesson.status === "Completed" ? "Проведено" : lesson.status === "Cancelled" ? "Отменено" : "Перенесено"}</small>
                        </button>
                      </li>
                    ))}
                  </ul>
                ) : <p className="calendar-empty-day">Свободный день</p>}
                <button type="button" className="calendar-add-slot" onClick={() => openNew(day)}>+ Добавить</button>
              </section>
            );
          })}
        </section>
      ) : null}
      {!loading && filtered.length === 0 && <p className="calendar-empty">На этот период занятий нет.</p>}

      {editor && (
        <div className="calendar-overlay" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget) setEditor(null); }}>
          <section className="calendar-dialog" role="dialog" aria-modal="true" aria-labelledby="lesson-form-title">
            <button type="button" className="dialog-close" onClick={() => setEditor(null)} aria-label="Закрыть">×</button>
            <h2 id="lesson-form-title">{editor.lesson ? "Изменить занятие" : "Новое занятие"}</h2>
            <form onSubmit={saveLesson} className="calendar-form">
              <label>Ученик<select name="studentId" required defaultValue={editor.lesson?.studentId || studentFilter}>
                <option value="">Выберите ученика</option>{students.map((item) => <option key={item.id} value={item.id}>{item.firstName} {item.lastName}</option>)}
              </select></label>
              <label>Курс<select name="courseId" defaultValue={editor.lesson?.courseId || ""}><option value="">Без курса</option>{courses.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
              {isAdmin && <label>Преподаватель<select name="teacherId" required defaultValue={editor.lesson?.teacherId || ""}>
                <option value="">Выберите преподавателя</option>{teachers.map((item) => <option key={item.id} value={item.id}>{item.firstName} {item.lastName}</option>)}
              </select></label>}
              <label>Название<input name="title" maxLength="255" required defaultValue={editor.lesson?.title || ""} /></label>
              <label>Описание<textarea name="description" maxLength="4000" defaultValue={editor.lesson?.description || ""} /></label>
              <div className="calendar-form-row">
                <label>Дата<input name="date" type="date" required defaultValue={editor.lesson ? asLocalDateTime(editor.lesson.scheduledAt).date : inputDate(editor.initialDate || date)} /></label>
                <label>Время<input name="time" type="time" required defaultValue={editor.lesson ? asLocalDateTime(editor.lesson.scheduledAt).time : "09:00"} /></label>
                <label>Минут<input name="durationMinutes" type="number" min="1" max="1440" required defaultValue={editor.lesson?.durationMinutes || 60} /></label>
              </div>
              <label className="calendar-checkbox"><input type="checkbox" name="series" disabled={Boolean(editor.lesson)} /> Еженедельно повторять</label>
              {editor.lesson?.seriesId && <label className="calendar-checkbox"><input type="checkbox" name="applySeries" /> Изменить всю серию (будущие занятия)</label>}
              {(!editor.lesson || editor.lesson.seriesId) && <label>День недели серии<select name="dayOfWeek" defaultValue={editor.lesson ? (new Date(editor.lesson.scheduledAt).getDay() + 6) % 7 : ((editor.initialDate || date).getDay() + 6) % 7}>
                {weekdays.map((name, index) => <option key={name} value={index}>{["Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье"][index]}</option>)}
              </select></label>}
              {!editor.lesson && <label>Повторять до (необязательно)<input name="endDate" type="date" /></label>}
              {editor.lesson && <label>Заметки<textarea name="notes" maxLength="4000" defaultValue={editor.lesson.notes || ""} /></label>}
              <footer><button type="button" onClick={() => setEditor(null)}>Назад</button><button className="calendar-primary" disabled={busy}>{busy ? "Сохраняем…" : "Сохранить"}</button></footer>
            </form>
          </section>
        </div>
      )}

      {selected && (
        <div className="calendar-overlay" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget) setSelected(null); }}>
          <section className="calendar-dialog lesson-details" role="dialog" aria-modal="true" aria-labelledby="lesson-detail-title">
            <button type="button" className="dialog-close" onClick={() => setSelected(null)} aria-label="Закрыть">×</button>
            <p className={`calendar-status status-${selected.status.toLowerCase()}`}>{selected.status}</p>
            <h2 id="lesson-detail-title">{selected.title}</h2>
            <p>{selected.studentName}{selected.courseName ? ` · ${selected.courseName}` : ""}</p>
            <p>{formatDate(new Date(selected.scheduledAt))}, {formatClock(new Date(selected.scheduledAt))} · {selected.durationMinutes} мин.</p>
            <p>{selected.description || "Нет описания."}</p>
            {selected.notes && <p><strong>Заметки:</strong> {selected.notes}</p>}
            {selected.statusReason && <p><strong>Причина изменения:</strong> {selected.statusReason} · {new Date(selected.modifiedAtUtc).toLocaleString()}</p>}
            {!!selected.rescheduleHistory?.length && <div><h3>История переносов</h3><ul>{selected.rescheduleHistory.map((item) => <li key={`${item.originalLessonId}-${item.newLessonId}`}>{new Date(item.rescheduledAt).toLocaleString()} — {item.reason || "Без причины"}</li>)}</ul></div>}
            {selected.status === "Scheduled" && <div className="lesson-actions">
              <button type="button" onClick={() => act(() => updateLessonStatus(selected.id, "Completed"))}>Провести</button>
              <button type="button" onClick={() => act(() => updateLessonStatus(selected.id, "Cancelled"))}>Отменить</button>
              <button type="button" onClick={() => { setReschedule(selected); setSelected(null); }}>Перенести</button>
              <button type="button" onClick={() => { setEditor({ lesson: selected, series: false }); setSelected(null); }}>Изменить</button>
            </div>}
            <button type="button" onClick={() => navigate(isAdmin ? "/admin/school/calendar" : `/teacher/lessons/${selected.id}`)}>Открыть подробности</button>
          </section>
        </div>
      )}

      {reschedule && (
        <div className="calendar-overlay">
          <section className="calendar-dialog" role="dialog" aria-modal="true" aria-labelledby="reschedule-title">
            <button type="button" className="dialog-close" onClick={() => setReschedule(null)} aria-label="Закрыть">×</button>
            <h2 id="reschedule-title">Перенести занятие</h2>
            <form onSubmit={saveReschedule} className="calendar-form">
              <label>Дата<input name="date" type="date" required defaultValue={asLocalDateTime(reschedule.scheduledAt).date} /></label>
              <label>Время<input name="time" type="time" required defaultValue={asLocalDateTime(reschedule.scheduledAt).time} /></label>
              <label>Причина<input name="reason" maxLength="500" /></label>
              <footer><button type="button" onClick={() => setReschedule(null)}>Назад</button><button className="calendar-primary" disabled={busy}>Подтвердить перенос</button></footer>
            </form>
          </section>
        </div>
      )}
    </main>
  );
}

export default CalendarPage;
