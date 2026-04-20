import React, { useState } from "react";
import { users } from "../../../data/users";
import "./LoginFormStyle.css";

const LoginForm = ({ onLogin }) => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    const user = users.find(
      (u) => u.username === username && u.password === password
    );
    if (user) {
      setError("");
      onLogin(user); 
    } else {
      setError("* Invalid login or password");
    }
  };

  return (
    <>
        <div className="login-form-container">
            <div className="login-form">
                <form onSubmit={handleSubmit} className="form">
                <label htmlFor="login">Login</label>
                <input
                    placeholder="Login"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                />
                <label htmlFor="password">Password</label>
                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
                <button type="submit" className="login-button">
                    Sign in
                </button>
                {error && <p style={{ color: "red", fontWeight: "bold", fontSize: "20px" }}>{error}</p>}
                </form>
            </div>
        </div>
    </>
  );
};

export default LoginForm;