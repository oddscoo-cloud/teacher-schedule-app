import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import "./index.css";

const savedTheme = window.localStorage.getItem("teacher-schedule-theme");
document.documentElement.dataset.theme = savedTheme === "dark" ? "dark" : "light";

const root = ReactDOM.createRoot(document.getElementById("root"));

root.render(
  <BrowserRouter>
    <App />
  </BrowserRouter>
);