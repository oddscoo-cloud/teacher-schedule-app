import React, { useState } from "react";
import LoginForm from "./LoginForm/LoginForm";
import AdminSchedule from "./AdminSchedule/AdminSchedule";
import TeacherSchedule from "./TeacherSchedule/TeacherSchedule";

const MainContent = () => {
  const [user, setUser] = useState(null);

  if (!user) return <LoginForm onLogin={setUser} />;

  if (user.role === "admin") return <AdminSchedule />;

  if (user.role === "teacher") return <TeacherSchedule teacherId={user.id} />;

  return null;
};

export default MainContent;