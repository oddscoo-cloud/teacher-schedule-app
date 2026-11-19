import { Navigate } from "react-router-dom";

const ProtectedRoute = ({ children, role }) => {
  const token = localStorage.getItem("token");
  const userRole = localStorage.getItem("role")?.toLowerCase();

  if (!token) {
    return <Navigate to="/login" />;
  }

  const expectedRole = role?.toLowerCase();
  const isSchoolAdmin =
    expectedRole === "schooladmin" && ["schooladmin", "admin"].includes(userRole);

  if (role && userRole !== expectedRole && !isSchoolAdmin) {
    return <Navigate to="/login" />;
  }

  return children;
};

export default ProtectedRoute;