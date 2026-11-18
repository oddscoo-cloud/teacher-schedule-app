import React, { useEffect, useState } from "react";
import { createCourse, deleteCourse, fetchCourses, updateCourse } from "../../api";
import "./StudentManagement.css";

const emptyCourse = { name: "", description: "" };

const SchoolCourses = () => {
  const [courses, setCourses] = useState([]);
  const [course, setCourse] = useState(emptyCourse);
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState("");

  const loadCourses = async () => {
    try {
      setCourses(await fetchCourses());
      setError("");
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  useEffect(() => {
    loadCourses();
  }, []);

  const save = async (event) => {
    event.preventDefault();
    try {
      if (editingId) {
        await updateCourse(editingId, course);
      } else {
        await createCourse(course);
      }
      setCourse(emptyCourse);
      setEditingId(null);
      await loadCourses();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const edit = (item) => {
    setEditingId(item.id);
    setCourse({ name: item.name, description: item.description || "" });
  };

  const remove = async (id) => {
    try {
      await deleteCourse(id);
      await loadCourses();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <main className="student-management">
      <h1>Курсы школы</h1>
      {error && <p role="alert" className="student-error">{error}</p>}
      {courses.length === 0 && !error && <p>Курсы пока не созданы.</p>}
      <ul>
        {courses.map((item) => (
          <li key={item.id}>
            <strong>{item.name}</strong>
            {item.description && <span> — {item.description}</span>}
            <button type="button" onClick={() => edit(item)}>Изменить</button>
            <button type="button" onClick={() => remove(item.id)}>Удалить</button>
          </li>
        ))}
      </ul>
      <form className="student-form" onSubmit={save}>
        <label>
          Название *
          <input
            required
            maxLength="255"
            value={course.name}
            onChange={(event) => setCourse({ ...course, name: event.target.value })}
          />
        </label>
        <label>
          Описание
          <textarea
            value={course.description}
            onChange={(event) => setCourse({ ...course, description: event.target.value })}
          />
        </label>
        <button type="submit">{editingId ? "Сохранить курс" : "Добавить курс"}</button>
        {editingId && (
          <button
            type="button"
            onClick={() => {
              setEditingId(null);
              setCourse(emptyCourse);
            }}
          >
            Отмена
          </button>
        )}
      </form>
    </main>
  );
};

export default SchoolCourses;
