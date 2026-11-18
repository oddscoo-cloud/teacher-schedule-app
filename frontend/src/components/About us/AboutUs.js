import React from "react";
import { Link } from "react-router-dom";
import "./AboutUs.css";

const features = [
  {
    number: "01",
    title: "Расписание перед глазами",
    description: "Планируйте неделю и быстро находите нужный урок.",
  },
  {
    number: "02",
    title: "Ученики в порядке",
    description: "Контакты, курсы и заметки собраны в одной карточке.",
  },
  {
    number: "03",
    title: "Ничего лишнего",
    description: "Простой интерфейс, который помогает сосредоточиться на занятиях.",
  },
];

export default function AboutUs() {
  return (
    <main className="welcome-page">
      <section className="welcome-hero">
        <div className="welcome-copy">
          <p className="welcome-eyebrow">Рабочее пространство учителя</p>
          <h1>Всё для уроков —<br />в одном месте.</h1>
          <p className="welcome-description">
            Расписание, ученики и заметки без лишних шагов. Войдите, чтобы сразу перейти к работе.
          </p>
          <Link className="welcome-primary" to="/login">
            Войти в кабинет <span aria-hidden="true">→</span>
          </Link>
        </div>
        <div className="welcome-summary" aria-label="Возможности кабинета">
          <div className="welcome-summary-top">
            <span className="welcome-summary-mark" aria-hidden="true">У</span>
            <span>Ваш рабочий день</span>
          </div>
          <div className="welcome-summary-line">
            <span className="welcome-summary-dot" aria-hidden="true" />
            <div>
              <strong>Расписание</strong>
              <span>План на всю неделю</span>
            </div>
            <span aria-hidden="true">01</span>
          </div>
          <div className="welcome-summary-line">
            <span className="welcome-summary-dot" aria-hidden="true" />
            <div>
              <strong>Ученики</strong>
              <span>Список и важные заметки</span>
            </div>
            <span aria-hidden="true">02</span>
          </div>
          <div className="welcome-summary-footer">Понятно с первого нажатия</div>
        </div>
      </section>

      <section className="welcome-features" aria-label="Возможности кабинета">
        {features.map((feature) => (
          <article className="welcome-feature" key={feature.number}>
            <span>{feature.number}</span>
            <h2>{feature.title}</h2>
            <p>{feature.description}</p>
          </article>
        ))}
      </section>
    </main>
  );
}
