import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { fetchCourses, fetchStudents } from "../../api";
import CourseFilter from "./CourseFilter";
import "./StudentManagement.css";

const StudentList = ({ admin = false }) => {
  const [students, setStudents] = useState([]);
  const [courses, setCourses] = useState([]);
  const [search, setSearch] = useState("");
  const [course, setCourse] = useState("");
  const [status, setStatus] = useState("");
  const [error, setError] = useState("");
  const [courseError, setCourseError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;
    setLoading(true);
    fetchStudents({ search, course, status })
      .then((data) => {
        if (active) {
          setStudents(data);
          setError("");
        }
      })
      .catch((requestError) => {
        if (active) setError(requestError.message);
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [search, course, status]);

  useEffect(() => {
    let active = true;
    fetchCourses()
      .then((data) => {
        if (active) setCourses(data);
      })
      .catch((requestError) => {
        if (active) setCourseError(requestError.message);
      });
    return () => {
      active = false;
    };
  }, []);

  const newStudentPath = admin
    ? "/admin/school/students/new"
    : "/teacher/students/new";

  return (
    <main className="student-management">
      <header className="student-page-header">
        <h1>{admin ? "Ученики школы" : "Мои ученики"}</h1>
        <Link className="student-primary-button" to={newStudentPath}>
          Добавить ученика
        </Link>
      </header>

      <section className="student-filters" aria-label="Фильтры учеников">
        <label className="student-filter">
          Поиск
          <input
            type="search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Имя, фамилия или email"
          />
        </label>
        <CourseFilter courses={courses} value={course} onChange={setCourse} />
        <label className="student-filter">
          Статус
          <select value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">Любой</option>
            <option value="Active">Активный</option>
            <option value="Pending">Ожидает подтверждения</option>
            <option value="Disabled">Отключен</option>
          </select>
        </label>
      </section>

      {courseError && <p className="student-inline-warning">{courseError}</p>}
      {error && <p role="alert" className="student-error">{error}</p>}
      {loading && <p>Загрузка учеников...</p>}
      {!loading && !error && students.length === 0 && (
        <p className="student-empty">Ученики не найдены.</p>
      )}
      {!loading && students.length > 0 && (
        <div className="student-table-wrap">
          <table className="student-table">
            <thead>
              <tr>
                <th>Ученик</th>
                <th>Email</th>
                <th>Телефон</th>
                <th>Статус</th>
              </tr>
            </thead>
            <tbody>
              {students.map((student) => (
                <tr key={student.id}>
                  <td>
                    <Link to={`${admin ? "/admin/school/students" : "/teacher/students"}/${student.id}`}>
                      {[student.lastName, student.firstName, student.patronymic]
                        .filter(Boolean)
                        .join(" ")}
                    </Link>
                  </td>
                  <td>{student.email || "—"}</td>
                  <td>{student.phoneNumber || "—"}</td>
                  <td>{student.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </main>
  );
};

export default StudentList;
