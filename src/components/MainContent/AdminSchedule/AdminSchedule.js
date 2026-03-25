import React, { useState } from "react";
import { teachers } from "../../../data/teachers";
import "./Styles/AdminSchedule.css";
import AdminScheduleSidebar from "./components/AdminScheduleSibebar";
import AdminScheduleTable from "./components/AdminScheduleTable";

const daysOfWeek = ["Пн", "Вт", "Ср", "Чт", "Пт"];

const AdminSchedule = () => {
  const [selectedTeacher, setSelectedTeacher] = useState(null);

  return (
    <div className="admin-schedule-container">
      <div className="admin-schedule-content">
        {/* Sidebar с именами учителей */}
        <AdminScheduleSidebar
          teachers={teachers}
          selectedTeacher={selectedTeacher}
          onSelectTeacher={setSelectedTeacher}
        />

        {/* Основное содержимое — таблица расписания */}
        <AdminScheduleTable
          selectedTeacher={selectedTeacher}
          daysOfWeek={daysOfWeek}
        />
      </div>
    </div>
  );
};

export default AdminSchedule;