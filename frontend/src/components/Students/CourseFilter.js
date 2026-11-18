import React from "react";

const CourseFilter = ({ courses, value, onChange }) => (
  <label className="student-filter">
    Курс
    <select value={value} onChange={(event) => onChange(event.target.value)}>
      <option value="">Все курсы</option>
      {courses.map((course) => (
        <option key={course.id} value={course.id}>
          {course.name}
        </option>
      ))}
    </select>
  </label>
);

export default CourseFilter;
