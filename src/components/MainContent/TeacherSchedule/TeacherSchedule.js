import React from "react";
import "./TeacherSchedule.css";

const TeacherSchedule = ({ teacherId }) => {
  return <div className="teacher-schedule">Учитель {teacherId}: здесь будет его расписание</div>;
};

export default TeacherSchedule;