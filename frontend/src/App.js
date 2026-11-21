import { Routes, Route } from "react-router-dom";
import "./App.css";
import AboutUs from "./components/About us/AboutUs";
import LoginForm from "../src/components/MainContent/LoginForm/LoginForm";
import AdminSchedule from "./components/MainContent/AdminSchedule/AdminSchedule";
import TeacherSchedule from "./components/MainContent/TeacherSchedule/TeacherSchedule";
import ProtectedRoute from "./routers/ProtectedRoute";
import Header from "../src/components/Header/Header";
import StudentList from "./components/Students/StudentList";
import StudentCard from "./components/Students/StudentCard";
import StudentForm from "./components/Students/StudentForm";
import SchoolCourses from "./components/Students/SchoolCourses";
import HomeworkPage from "./components/Homework/HomeworkPage";

function App() {
  return (
    <div className="app-shell">
      <Header />
      <Routes>
        <Route path="/" element={<AboutUs />} />
        <Route path="/about" element={<AboutUs />} />
        <Route path="/login" element={<LoginForm />} />

        <Route
          path="/admin"
          element={
            <ProtectedRoute role="admin">
              <AdminSchedule />
            </ProtectedRoute>
          }
        />

        <Route
          path="/teacher"
          element={
            <ProtectedRoute role="teacher">
              <TeacherSchedule />
            </ProtectedRoute>
          }
        />

        <Route
          path="/teacher/students"
          element={
            <ProtectedRoute role="teacher">
              <StudentList />
            </ProtectedRoute>
          }
        />
        <Route
          path="/teacher/students/new"
          element={
            <ProtectedRoute role="teacher">
              <StudentForm />
            </ProtectedRoute>
          }
        />
        <Route
          path="/teacher/students/:id/edit"
          element={
            <ProtectedRoute role="teacher">
              <StudentForm />
            </ProtectedRoute>
          }
        />
        <Route
          path="/teacher/students/:id"
          element={
            <ProtectedRoute role="teacher">
              <StudentCard />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/school/students"
          element={
            <ProtectedRoute role="schooladmin">
              <StudentList admin />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/school/students/new"
          element={
            <ProtectedRoute role="schooladmin">
              <StudentForm admin />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/school/students/:id/edit"
          element={
            <ProtectedRoute role="schooladmin">
              <StudentForm admin />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/school/students/:id"
          element={
            <ProtectedRoute role="schooladmin">
              <StudentCard admin />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/school/courses"
          element={
            <ProtectedRoute role="schooladmin">
              <SchoolCourses />
            </ProtectedRoute>
          }
        />
        <Route
          path="/homework"
          element={
            <ProtectedRoute>
              <HomeworkPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </div>
  );
}

export default App;