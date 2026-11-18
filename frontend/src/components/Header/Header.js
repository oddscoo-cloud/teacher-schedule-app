import React from "react";
import { Link, NavLink, useLocation, useNavigate } from "react-router-dom";
import "./HeaderStyles/HeaderStyle.css";

function Header() {
  const location = useLocation();
  const navigate = useNavigate();
  const role = localStorage.getItem("role")?.toLowerCase();
  const isLoggedIn = Boolean(localStorage.getItem("token"));
  const isAdmin = role === "admin" || role === "schooladmin";
  const canManageSchedule = role === "admin" || role === "teacher";
  const homePath = role === "admin" ? "/admin" : role === "teacher" ? "/teacher" : "/admin/school/students";
  const basePath = isAdmin ? "/admin/school/students" : "/teacher/students";

  const signOut = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    localStorage.removeItem("teacherId");
    navigate("/login");
  };

  return (
    <header className="app-header">
      <div className="app-header-inner">
        <Link className="app-brand" to={isLoggedIn ? homePath : "/"} aria-label="На главную">
          <span className="app-brand-mark" aria-hidden="true">У</span>
          <span>Уроки<span className="app-brand-period">.</span></span>
        </Link>

        {isLoggedIn ? (
          <nav className="app-navigation" aria-label="Основная навигация">
            {canManageSchedule && (
              <NavLink
                to={homePath}
                end
                className={({ isActive }) => `app-nav-link${isActive ? " active" : ""}`}
              >
                Расписание
              </NavLink>
            )}
            <NavLink
              to={basePath}
              className={({ isActive }) => `app-nav-link${isActive ? " active" : ""}`}
            >
              Ученики
            </NavLink>
            {isAdmin && (
              <NavLink
                to="/admin/school/courses"
                className={({ isActive }) => `app-nav-link${isActive ? " active" : ""}`}
              >
                Курсы
              </NavLink>
            )}
          </nav>
        ) : location.pathname !== "/login" ? (
          <Link className="app-header-action" to="/login">
            Войти
          </Link>
        ) : null}

        {isLoggedIn && (
          <button className="app-header-action app-signout" type="button" onClick={signOut}>
            Выйти
          </button>
        )}
      </div>
    </header>
  );
}

export default Header;