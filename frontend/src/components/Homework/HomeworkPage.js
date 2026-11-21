import React, { useCallback, useEffect, useMemo, useState } from "react";
import {
  createHomework,
  downloadHomeworkAttachment,
  fetchCourses,
  fetchHomeworks,
  fetchStudents,
  gradeSubmission,
  submitHomework,
} from "../../api";
import "./HomeworkPage.css";

const role = localStorage.getItem("role")?.toLowerCase();
const isTeacher = role === "teacher";
const canReview = isTeacher || ["admin", "schooladmin"].includes(role);

function HomeworkPage() {
  const [homeworks, setHomeworks] = useState([]);
  const [students, setStudents] = useState([]);
  const [courses, setCourses] = useState([]);
  const [studentFilter, setStudentFilter] = useState("");
  const [courseFilter, setCourseFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [title, setTitle] = useState("");
  const [instructions, setInstructions] = useState("");
  const [studentId, setStudentId] = useState("");
  const [courseId, setCourseId] = useState("");
  const [dueAt, setDueAt] = useState("");

  const loadData = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const filters = {
        studentId: studentFilter || undefined,
        courseId: courseFilter || undefined,
      };
      const [work, studentList, courseList] = await Promise.all([
        fetchHomeworks(filters),
        canReview ? fetchStudents({ status: "Active" }) : Promise.resolve([]),
        canReview ? fetchCourses() : Promise.resolve([]),
      ]);
      setHomeworks(work);
      setStudents(studentList);
      setCourses(courseList);
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, [studentFilter, courseFilter]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const submitForm = async (event) => {
    event.preventDefault();
    setBusy(true);
    setError("");
    try {
      await createHomework({
        studentId: Number(studentId),
        courseId: courseId ? Number(courseId) : null,
        title,
        instructions,
        dueAt: dueAt ? new Date(dueAt).toISOString() : null,
      });
      setTitle("");
      setInstructions("");
      setDueAt("");
      await loadData();
    } catch (submitError) {
      setError(submitError.message);
    } finally {
      setBusy(false);
    }
  };

  const handleGrade = async (submissionId, grade) => {
    setBusy(true);
    setError("");
    try {
      await gradeSubmission(submissionId, grade);
      await loadData();
    } catch (gradeError) {
      setError(gradeError.message);
    } finally {
      setBusy(false);
    }
  };

  const handleDownload = async (attachment) => {
    try {
      const blob = await downloadHomeworkAttachment(attachment.id);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = attachment.displayName;
      link.click();
      window.setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch (downloadError) {
      setError(downloadError.message);
    }
  };

  const studentOptions = useMemo(
    () => students.filter((student) => student.status?.toLowerCase() === "active"),
    [students],
  );

  return (
    <main className="homework-page">
      <header className="homework-page-heading">
        <div>
          <p className="homework-eyebrow">Учебный процесс</p>
          <h1>{isTeacher ? "Домашние задания" : "Мои домашние задания"}</h1>
          <p>Задания, ответы и результаты проверки в одном месте.</p>
        </div>
      </header>

      {error && <div className="homework-alert" role="alert">{error}</div>}

      {isTeacher && (
        <section className="homework-panel">
          <h2>Новое задание</h2>
          <form className="homework-create-form" onSubmit={submitForm}>
            <label>
              Ученик
              <select required value={studentId} onChange={(event) => setStudentId(event.target.value)}>
                <option value="">Выберите ученика</option>
                {studentOptions.map((student) => (
                  <option key={student.id} value={student.id}>
                    {student.firstName} {student.lastName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Курс
              <select value={courseId} onChange={(event) => setCourseId(event.target.value)}>
                <option value="">Без курса</option>
                {courses.map((course) => (
                  <option key={course.id} value={course.id}>{course.name}</option>
                ))}
              </select>
            </label>
            <label className="homework-form-wide">
              Название
              <input required maxLength={255} value={title} onChange={(event) => setTitle(event.target.value)} />
            </label>
            <label className="homework-form-wide">
              Инструкция
              <textarea maxLength={4000} rows="3" value={instructions} onChange={(event) => setInstructions(event.target.value)} />
            </label>
            <label>
              Срок сдачи
              <input type="datetime-local" value={dueAt} onChange={(event) => setDueAt(event.target.value)} />
            </label>
            <button type="submit" disabled={busy || !studentId}>Создать задание</button>
          </form>
        </section>
      )}

      {canReview && (
        <section className="homework-filters" aria-label="Фильтры заданий">
          <label>
            Ученик
            <select value={studentFilter} onChange={(event) => setStudentFilter(event.target.value)}>
              <option value="">Все ученики</option>
              {studentOptions.map((student) => (
                <option key={student.id} value={student.id}>
                  {student.firstName} {student.lastName}
                </option>
              ))}
            </select>
          </label>
          <label>
            Курс
            <select value={courseFilter} onChange={(event) => setCourseFilter(event.target.value)}>
              <option value="">Все курсы</option>
              {courses.map((course) => (
                <option key={course.id} value={course.id}>{course.name}</option>
              ))}
            </select>
          </label>
        </section>
      )}

      {loading ? (
        <p className="homework-empty">Загружаем задания…</p>
      ) : homeworks.length === 0 ? (
        <p className="homework-empty">Пока нет домашних заданий.</p>
      ) : (
        <section className="homework-list" aria-label="Домашние задания">
          {homeworks.map((homework) => {
            const latest = homework.submissions?.[homework.submissions.length - 1];
            const canResubmit = !latest || latest.grade !== null;
            return (
              <article className="homework-card" key={homework.id}>
                <div className="homework-card-heading">
                  <div>
                    <h2>{homework.title}</h2>
                    <p>{isTeacher ? homework.studentName : homework.teacherName}{homework.courseName ? ` · ${homework.courseName}` : ""}</p>
                  </div>
                  <span className={`homework-status${latest?.grade !== null && latest ? " graded" : ""}`}>
                    {!latest ? "Ожидает ответа" : latest.grade === null ? "На проверке" : `Оценка: ${latest.grade}/100`}
                  </span>
                </div>
                {homework.instructions && <p className="homework-instructions">{homework.instructions}</p>}
                {homework.dueAt && <p className="homework-due">Срок: {new Date(homework.dueAt).toLocaleString()}</p>}

                {homework.submissions?.map((submission) => (
                  <div className="homework-submission" key={submission.id}>
                    <div className="homework-submission-heading">
                      <strong>Ответ #{submission.attemptNumber}</strong>
                      <span>{new Date(submission.submittedAt).toLocaleString()}</span>
                    </div>
                    {submission.text && <p>{submission.text}</p>}
                    {submission.links.map((link) => (
                      <a href={link} key={link} target="_blank" rel="noreferrer">{link}</a>
                    ))}
                    {submission.attachments.map((attachment) => (
                      <button
                        className="homework-attachment"
                        type="button"
                        key={attachment.id}
                        onClick={() => handleDownload(attachment)}
                      >
                        Скачать: {attachment.displayName}
                      </button>
                    ))}
                    {canReview && submission.grade === null && (
                      <form
                        className="homework-grade-form"
                        onSubmit={(event) => {
                          event.preventDefault();
                          handleGrade(submission.id, event.currentTarget.elements.grade.value);
                        }}
                      >
                        <label>
                          Оценка 0–100
                          <input name="grade" type="number" min="0" max="100" required />
                        </label>
                        <button type="submit" disabled={busy}>Сохранить оценку</button>
                      </form>
                    )}
                  </div>
                ))}

                {!isTeacher && canResubmit && (
                  <SubmissionForm
                    homeworkId={homework.id}
                    disabled={busy}
                    onSubmit={async (data) => {
                      setBusy(true);
                      setError("");
                      try {
                        await submitHomework(homework.id, data);
                        await loadData();
                        return true;
                      } catch (submitError) {
                        setError(submitError.message);
                        return false;
                      } finally {
                        setBusy(false);
                      }
                    }}
                  />
                )}
              </article>
            );
          })}
        </section>
      )}
    </main>
  );
}

function SubmissionForm({ homeworkId, disabled, onSubmit }) {
  const [text, setText] = useState("");
  const [links, setLinks] = useState("");
  const [files, setFiles] = useState([]);

  const handleSubmit = async (event) => {
    event.preventDefault();
    const succeeded = await onSubmit({
      text,
      links: links.split(/\r?\n/).map((link) => link.trim()).filter(Boolean),
      files: Array.from(files),
    });
    if (!succeeded) return;
    setText("");
    setLinks("");
    setFiles([]);
    event.target.reset();
  };

  return (
    <form className="homework-submission-form" onSubmit={handleSubmit}>
      <label>
        Текст ответа
        <textarea maxLength={8000} rows="3" value={text} onChange={(event) => setText(event.target.value)} />
      </label>
      <label>
        Ссылки (по одной на строку)
        <textarea rows="2" value={links} onChange={(event) => setLinks(event.target.value)} />
      </label>
      <label>
        Фото (JPEG или PNG, до 5 МБ каждое)
        <input type="file" accept="image/jpeg,image/png" multiple onChange={(event) => setFiles(event.target.files)} />
      </label>
      <button type="submit" disabled={disabled}>Отправить ответ</button>
    </form>
  );
}

export default HomeworkPage;
