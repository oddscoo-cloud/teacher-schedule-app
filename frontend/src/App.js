import { Routes, Route } from "react-router-dom";
import AboutUs from "./components/About us/AboutUs";
import LoginForm from "../src/components/MainContent/LoginForm/LoginForm";
import AdminSchedule from "./components/MainContent/AdminSchedule/AdminSchedule";
import TeacherSchedule from "./components/MainContent/TeacherSchedule/TeacherSchedule";
import ProtectedRoute from "./routers/ProtectedRoute";
import Header from "../src/components/Header/Header";

function App() {
  return (
    <>
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
      </Routes>
    </>
  );
}

export default App;