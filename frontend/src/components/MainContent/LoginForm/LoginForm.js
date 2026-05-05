import React, { useState, useEffect, useRef } from "react";
import "./LoginFormStyle.css";
import { useNavigate } from "react-router-dom";
import Header from "../../Header/Header";

const LoginForm = ({ onLogin }) => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [scrollY, setScrollY] = useState(0);
  const navigate = useNavigate();
  const formRef = useRef(null);

  useEffect(() => {
    const handleScroll = () => {
      setScrollY(window.scrollY);
    };

    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  useEffect(() => {
    if (formRef.current) {
      const scale = Math.max(0.85, 1 - scrollY * 0.0008);
      const opacity = Math.max(0.6, 1 - scrollY * 0.0015);
      const translateY = scrollY * 0.3;

      formRef.current.style.transform = `scale(${scale}) translateY(${translateY}px)`;
      formRef.current.style.opacity = opacity;
    }
  }, [scrollY]);
  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const res = await fetch("https://localhost:7151/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          login: username,
          password: password,
        }),
      });

      if (!res.ok) {
        setError("Невірний логін або пароль");
        return;
      }

      const data = await res.json();

      setError("");

      localStorage.setItem("token", data.token);
      localStorage.setItem("role", data.role);
      localStorage.setItem("teacherId", data.teacherId ?? "");
      console.log("Полученный токен:", data);

      if (data.role?.toLowerCase() === "admin") {
        navigate("/admin");
      } else if (data.role?.toLowerCase() === "teacher") {
        navigate("/teacher");
      }

    } catch (err) {
      setError("Помилка сервера");
      console.error(err);
    }
  };

  return (
    <>
      <div className="login-form-container">
        <div className="login-form" ref={formRef}>
          <form onSubmit={handleSubmit} className="form">
            
            <label>Логін</label>
            <input
              placeholder="Логін"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />

            <label>Пароль</label>
            <input
              type="password"
              placeholder="Пароль"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />

            <button type="submit" className="login-button">
              Увійти
            </button>

            {error && (
              <p className="login-error">
                {error}
              </p>
            )}
          </form>
        </div>
      </div>
    </>
  );
};

export default LoginForm;