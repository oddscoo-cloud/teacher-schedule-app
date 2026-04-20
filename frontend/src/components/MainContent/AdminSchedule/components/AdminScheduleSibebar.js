import React from "react";
import "../Styles/AdminSchedule.css";

function AdminScheduleSidebar({ teachers, selectedTeacher, onSelectTeacher }) {
    return(
        <div className="sidebar">
          <h3>Учителя</h3>
          <ul>
            {teachers.map((teacher) => (
              <li
                key={teacher.id}
                className={selectedTeacher?.id === teacher.id ? "active" : ""}
                onClick={() => onSelectTeacher(teacher)}
              >
                {teacher.name}
              </li>
            ))}
          </ul>
        </div>
    )
}

export default AdminScheduleSidebar;