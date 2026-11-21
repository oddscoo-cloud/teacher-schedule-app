import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { deleteStudent, fetchStudent } from "../../api";
import StudentNotesList from "./StudentNotesList";
import StudentFinancePanel from "./StudentFinancePanel";
import "./StudentManagement.css";

const StudentCard = ({ admin = false }) => {
  const { id } = useParams();
  const [student, setStudent] = useState(null);
  const [error, setError] = useState("");
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    let active = true;
    setStudent(null);
    setError("");
    fetchStudent(id)
      .then((data) => {
        if (active) setStudent(data);
      })
      .catch((requestError) => {
        if (active) setError(requestError.message);
      });
    return () => {
      active = false;
    };
  }, [id]);

  if (error) return <p role="alert" className="student-error">{error}</p>;
  if (!student) return <p>Загрузка карточки...</p>;

  const basePath = admin ? "/admin/school/students" : "/teacher/students";
  const removeStudent = async () => {
    if (!window.confirm("Удалить карточку ученика?")) return;
    setDeleting(true);
    setError("");
    try {
      await deleteStudent(id);
      window.location.assign(basePath);
    } catch (requestError) {
      setError(requestError.message);
      setDeleting(false);
    }
  };
  return (
    <main className="student-management">
      <header className="student-page-header">
        <div>
          <Link className="student-back-link" to={basePath}>← К списку учеников</Link>
          <h1>{[student.lastName, student.firstName, student.patronymic].filter(Boolean).join(" ")}</h1>
        </div>
        <div className="student-card-actions">
          <Link className="student-primary-button" to={`${basePath}/${id}/edit`}>Редактировать</Link>
          <button type="button" className="student-danger-button" onClick={removeStudent} disabled={deleting}>
            {deleting ? "Удаление..." : "Удалить"}
          </button>
        </div>
      </header>
      <dl className="student-details">
        <dt>Email</dt><dd>{student.email || "—"}</dd>
        <dt>Телефон</dt><dd>{student.phoneNumber || "—"}</dd>
        <dt>Дата рождения</dt>
        <dd>{student.dateOfBirth ? new Date(student.dateOfBirth).toLocaleDateString() : "—"}</dd>
        <dt>Статус</dt><dd>{student.status}</dd>
      </dl>

      <section className="student-info-section">
        <h2>Курсы</h2>
        {student.courses.length ? (
          <ul>{student.courses.map((course) => <li key={course.id}>{course.name}</li>)}</ul>
        ) : <p>Курсы не назначены.</p>}
      </section>
      <section className="student-info-section">
        <h2>Родители и опекуны</h2>
        {student.contacts.length ? (
          <ul>
            {student.contacts.map((contact, index) => (
              <li key={`${contact.email || contact.phoneNumber}-${index}`}>
                {contact.firstName} {contact.lastName}
                {contact.relationship ? ` (${contact.relationship})` : ""}
                {contact.phoneNumber ? ` · ${contact.phoneNumber}` : ""}
                {contact.email ? ` · ${contact.email}` : ""}
              </li>
            ))}
          </ul>
        ) : <p>Контакты не добавлены.</p>}
      </section>
      <section className="student-info-section">
        <h2>История занятий</h2>
        <p>История появится после подключения календаря занятий.</p>
      </section>
      <StudentFinancePanel studentId={Number(id)} />
      <StudentNotesList studentId={Number(id)} canEdit={!admin} />
    </main>
  );
};

export default StudentCard;
