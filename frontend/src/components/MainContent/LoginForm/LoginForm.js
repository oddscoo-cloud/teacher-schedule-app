import React, { useState } from "react";
import "./LoginFormStyle.css";
import { useNavigate } from "react-router-dom";
import API_BASE_URL from "../../../api";

const LoginForm = ({ onLogin }) => {
  const [email, setEmail] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isRegistering, setIsRegistering] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");
    try {
      const res = await fetch(
        `${API_BASE_URL}/auth/${isRegistering ? "register/teacher" : "login"}`,
        {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(isRegistering
          ? { email, password, firstName, lastName, userType: "standalone-teacher" }
          : { email, password }),
        }
      );

      if (!res.ok) {
        const response = await res.json().catch(() => null);
        setError(response?.message || (
          isRegistering
            ? "Не удалось создать аккаунт. Проверьте данные и попробуйте ещё раз."
            : "Неверный email или пароль."
        ));
        return;
      }

      const data = await res.json();
      if (isRegistering) {
        setIsRegistering(false);
        setPassword("");
        setSuccess("Аккаунт преподавателя создан. Теперь войдите с указанными email и паролем.");
        return;
      }

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
        <h1>{isRegistering ? "Создать аккаунт" : "С возвращением"}</h1>
        <p className="login-description">
          {isRegistering
            ? "Зарегистрируйтесь как преподаватель, чтобы создать личное рабочее пространство."
            : "Войдите, чтобы открыть расписание и список учеников."}
        </p>
        <form onSubmit={handleSubmit} className="login-form">
          {isRegistering && (
            <>
              <label htmlFor="register-first-name">Имя</label>
              <input
                id="register-first-name"
                autoComplete="given-name"
                required
                maxLength={255}
                value={firstName}
                onChange={(event) => setFirstName(event.target.value)}
              />
              <label htmlFor="register-last-name">Фамилия</label>
              <input
                id="register-last-name"
                autoComplete="family-name"
                required
                maxLength={255}
                value={lastName}
                onChange={(event) => setLastName(event.target.value)}
              />
            </>
          )}
          <label htmlFor="login-email">Email</label>
          <input
            id="login-email"
            type="email"
            autoComplete="email"
            required
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />

          <label htmlFor="login-password">Пароль</label>
          <input
            id="login-password"
            type="password"
            autoComplete={isRegistering ? "new-password" : "current-password"}
            minLength={isRegistering ? 6 : undefined}
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />

          <button type="submit" className="login-button" disabled={submitting}>
            {submitting
              ? (isRegistering ? "Создаём аккаунт…" : "Входим…")
              : (isRegistering ? "Зарегистрироваться" : "Войти")}
          </button>

          {error && <p className="login-error" role="alert">{error}</p>}
          {success && <p className="login-success" role="status">{success}</p>}
        </form>
        <p className="login-switch">
          {isRegistering ? "Уже есть аккаунт?" : "Нет аккаунта?"}{" "}
          <button
            type="button"
            onClick={() => {
              setIsRegistering((current) => !current);
              setError("");
              setSuccess("");
            }}
          >
            {isRegistering ? "Войти" : "Зарегистрироваться"}
          </button>
        </p>
      </section>
    </main>
  );
};

export default LoginForm;