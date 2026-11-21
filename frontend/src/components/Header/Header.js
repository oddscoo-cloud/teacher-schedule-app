import React from "react";
import { Link, NavLink, useLocation, useNavigate } from "react-router-dom";
import { switchWorkspace as switchWorkspaceRequest } from "../../api";
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
  const workspaces = JSON.parse(localStorage.getItem("workspaces") || "[]");
  const activeWorkspaceId = localStorage.getItem("activeWorkspaceId");
  const canSwitchWorkspace = workspaces.length > 1;

  const signOut = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    localStorage.removeItem("teacherId");
    localStorage.removeItem("workspaces");
    localStorage.removeItem("activeWorkspaceId");
    navigate("/login");
  };

  const switchWorkspace = async (event) => {
    const membershipId = Number(event.target.value);
    if (!membershipId) return;
    try {
      const data = await switchWorkspaceRequest(membershipId);
      localStorage.setItem("token", data.token);
      localStorage.setItem("role", data.user.role);
      localStorage.setItem("workspaces", JSON.stringify(data.workspaces || []));
      localStorage.setItem("activeWorkspaceId", membershipId.toString());
      window.location.reload();
    } catch (error) {
      window.alert(error.message);
    }
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
            {canManageSchedule && (
              <NavLink
                to={isAdmin ? "/admin/school/calendar" : "/teacher/calendar"}
                className={({ isActive }) => `app-nav-link${isActive ? " active" : ""}`}
              >
                Календарь
              </NavLink>
            )}
            <NavLink
              to={basePath}
              className={({ isActive }) => `app-nav-link${isActive ? " active" : ""}`}
            >
              Ученики
            </NavLink>
            <NavLink
              to="/homework"
              className={({ isActive }) => `app-nav-link${isActive ? " active" : ""}`}
            >
              Домашние задания
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
          <>
            {canSwitchWorkspace && (
              <label className="app-workspace-switcher">
                <span>Пространство</span>
                <select value={activeWorkspaceId || ""} onChange={switchWorkspace}>
                  {workspaces.map((workspace) => (
                    <option key={workspace.id} value={workspace.id}>
                      {workspace.schoolName || (workspace.type === "Standalone" ? "Личное пространство" : `Школа ${workspace.schoolId || ""}`)}
                    </option>
                  ))}
                </select>
              </label>
            )}
            <button className="app-header-action app-signout" type="button" onClick={signOut}>
              Выйти
            </button>
          </>
        )}
      </div>
    </header>
  );
}

export default Header;