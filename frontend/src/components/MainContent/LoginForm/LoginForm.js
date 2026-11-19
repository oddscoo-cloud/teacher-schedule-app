import React, { useState } from "react";
import "./LoginFormStyle.css";
import { useNavigate } from "react-router-dom";
import API_BASE_URL from "../../../api";

const LoginForm = ({ onLogin }) => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();
  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    try {
      const res = await fetch(`${API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email: username,
          password,
        }),
      });

      if (!res.ok) {
        setError("Неверный логин или пароль.");
        return;
      }

      const data = await res.json();
      const normalizedRole = data.user?.role?.toLowerCase();
      const destination = {
        admin: "/admin",
        teacher: "/teacher",
        schooladmin: "/admin/school/students",
      }[normalizedRole];
      if (!destination) {
        setError("Для этой учётной записи не настроен доступ.");
        return;
      }

      setError("");
      localStorage.setItem("token", data.token);
      localStorage.setItem("role", data.user.role);
      localStorage.setItem("workspaces", JSON.stringify(data.workspaces || []));
      const activeWorkspace = (data.workspaces || []).find((workspace) => workspace.isDefault);
      localStorage.setItem("activeWorkspaceId", activeWorkspace?.id?.toString() || "");
      onLogin?.(data);
      navigate(destination);

    } catch (err) {
      setError("Не удалось подключиться к серверу. Попробуйте ещё раз.");
      console.error(err);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="login-page">
      <section className="login-panel">
        <p className="login-eyebrow">Рабочее пространство школы</p>
        <h1>С возвращением</h1>
        <p className="login-description">Войдите, чтобы открыть расписание и список учеников.</p>
        <form onSubmit={handleSubmit} className="login-form">
          <label htmlFor="login-username">Логин</label>
          <input
            id="login-username"
            autoComplete="username"
            required
            value={username}
            onChange={(event) => setUsername(event.target.value)}
          />

          <label htmlFor="login-password">Пароль</label>
          <input
            id="login-password"
            type="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />

          <button type="submit" className="login-button" disabled={submitting}>
            {submitting ? "Входим…" : "Войти"}
          </button>

          {error && <p className="login-error" role="alert">{error}</p>}
        </form>
      </section>
    </main>
  );
};

export default LoginForm;