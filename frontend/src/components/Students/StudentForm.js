import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { createStudent, fetchCourses, fetchStudent, updateStudent } from "../../api";
import "./StudentManagement.css";

const blankStudent = {
  firstName: "",
  lastName: "",
  patronymic: "",
  email: "",
  phoneNumber: "",
  dateOfBirth: "",
  courseIds: [],
  contacts: [],
};

const StudentForm = ({ admin = false }) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [student, setStudent] = useState(blankStudent);
  const [courses, setCourses] = useState([]);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let active = true;
    Promise.all([
      fetchCourses(),
      id ? fetchStudent(id) : Promise.resolve(null),
    ])
      .then(([availableCourses, currentStudent]) => {
        if (!active) return;
        setCourses(availableCourses);
        if (currentStudent) {
          setStudent({
            ...currentStudent,
            dateOfBirth: currentStudent.dateOfBirth
              ? currentStudent.dateOfBirth.slice(0, 10)
              : "",
            courseIds: (currentStudent.courses || []).map((item) => item.id),
            contacts: currentStudent.contacts || [],
          });
        }
      })
      .catch((requestError) => {
        if (active) setError(requestError.message);
      });
    return () => {
      active = false;
    };
  }, [id]);

  const setField = (field, value) =>
    setStudent((current) => ({ ...current, [field]: value }));

  const save = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      const payload = {
        ...student,
        dateOfBirth: student.dateOfBirth || null,
        courseIds: student.courseIds.map(Number),
        contacts: student.contacts,
      };
      const saved = id
        ? await updateStudent(id, payload)
        : await createStudent(payload);
      navigate(
        `${admin ? "/admin/school/students" : "/teacher/students"}/${saved.id}`
      );
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <main className="student-management student-form-page">
      <h1>{id ? "Редактирование ученика" : "Новый ученик"}</h1>
      {error && <p role="alert" className="student-error">{error}</p>}
      <form className="student-form" onSubmit={save}>
        <label>
          Фамилия *
          <input
            required
            maxLength="255"
            value={student.lastName}
            onChange={(event) => setField("lastName", event.target.value)}
          />
        </label>
        <label>
          Имя *
          <input
            required
            maxLength="255"
            value={student.firstName}
            onChange={(event) => setField("firstName", event.target.value)}
          />
        </label>
        <label>
          Отчество
          <input
            maxLength="255"
            value={student.patronymic || ""}
            onChange={(event) => setField("patronymic", event.target.value)}
          />
        </label>
        <label>
          Email
          <input
            type="email"
            value={student.email || ""}
            onChange={(event) => setField("email", event.target.value)}
          />
        </label>
        <label>
          Телефон
          <input
            type="tel"
            value={student.phoneNumber || ""}
            onChange={(event) => setField("phoneNumber", event.target.value)}
          />
        </label>
        <label>
          Дата рождения
          <input
            type="date"
            value={student.dateOfBirth || ""}
            onChange={(event) => setField("dateOfBirth", event.target.value)}
          />
        </label>
        {courses.length > 0 && (
          <label>
            Курсы
            <select
              multiple
              value={student.courseIds.map(String)}
              onChange={(event) =>
                setField(
                  "courseIds",
                  Array.from(event.target.selectedOptions, (option) => option.value)
                )
              }
            >
              {courses.map((course) => (
                <option key={course.id} value={course.id}>{course.name}</option>
              ))}
            </select>
          </label>
        )}
        <fieldset className="student-contacts-form">
          <legend>Родители и опекуны</legend>
          {student.contacts.map((contact, index) => (
            <div className="student-contact-fields" key={`contact-${index}`}>
              <label>
                Имя *
                <input
                  required
                  value={contact.firstName}
                  onChange={(event) => {
                    const contacts = [...student.contacts];
                    contacts[index] = { ...contact, firstName: event.target.value };
                    setField("contacts", contacts);
                  }}
                />
              </label>
              <label>
                Фамилия *
                <input
                  required
                  value={contact.lastName}
                  onChange={(event) => {
                    const contacts = [...student.contacts];
                    contacts[index] = { ...contact, lastName: event.target.value };
                    setField("contacts", contacts);
                  }}
                />
              </label>
              <label>
                Email
                <input
                  type="email"
                  value={contact.email || ""}
                  onChange={(event) => {
                    const contacts = [...student.contacts];
                    contacts[index] = { ...contact, email: event.target.value };
                    setField("contacts", contacts);
                  }}
                />
              </label>
              <label>
                Телефон
                <input
                  type="tel"
                  value={contact.phoneNumber || ""}
                  onChange={(event) => {
                    const contacts = [...student.contacts];
                    contacts[index] = { ...contact, phoneNumber: event.target.value };
                    setField("contacts", contacts);
                  }}
                />
              </label>
              <label>
                Кем приходится
                <input
                  value={contact.relationship || ""}
                  onChange={(event) => {
                    const contacts = [...student.contacts];
                    contacts[index] = { ...contact, relationship: event.target.value };
                    setField("contacts", contacts);
                  }}
                />
              </label>
              <button
                type="button"
                onClick={() =>
                  setField(
                    "contacts",
                    student.contacts.filter((_, contactIndex) => contactIndex !== index)
                  )
                }
              >
                Удалить контакт
              </button>
            </div>
          ))}
          <button
            type="button"
            onClick={() =>
              setField("contacts", [
                ...student.contacts,
                { firstName: "", lastName: "", email: "", phoneNumber: "", relationship: "" },
              ])
            }
          >
            Добавить контакт
          </button>
        </fieldset>
        <div className="student-form-actions">
          <button type="submit" disabled={saving}>
            {saving ? "Сохранение..." : "Сохранить"}
          </button>
          <Link to={admin ? "/admin/school/students" : "/teacher/students"}>
            Отмена
          </Link>
        </div>
      </form>
    </main>
  );
};

export default StudentForm;
