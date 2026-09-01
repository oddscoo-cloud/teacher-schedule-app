import React, { useRef, useState } from "react";

const classOptions = ["1", "2", "3", "4", "5"];
const subjectOptions = ["Українська мова", "Математика"];
const plans = [
  {
    id: "5",
    title: "5 уроків на місяць",
    lessons: "5 занять",
    format: "формат онлайн",
    teacher: "викладач",
    homework: "домашні завдання",
  },
  {
    id: "10",
    title: "10 уроків на місяць",
    lessons: "10 занять",
    format: "формат онлайн",
    teacher: "викладач",
    homework: "домашні завдання",
    featured: true,
  },
  {
    id: "15",
    title: "15 уроків на місяць",
    lessons: "15 занять",
    format: "формат онлайн",
    teacher: "викладач",
    homework: "домашні завдання",
  },
];

const steps = [
  {
    number: "1",
    title: "Заявка",
    text: "Батьки залишають свої контакти.",
  },
  {
    number: "2",
    title: "Знайомство",
    text: "Ми уточнюємо рівень учня та підбираємо формат навчання.",
  },
  {
    number: "3",
    title: "Навчання",
    text: "Учень займається з викладачем онлайн.",
  },
  {
    number: "4",
    title: "Результат",
    text: "Регулярна практика, домашні завдання та контроль прогресу.",
  },
];

const faqs = [
  {
    question: "Чи можна займатися онлайн з телефону?",
    answer: "Так, заняття проходять у зручному онлайн-форматі, і їх можна вести зі смартфона.",
  },
  {
    question: "Скільки триває один урок?",
    answer: "Зазвичай один урок триває 45–60 хвилин, залежно від формату навчання.",
  },
  {
    question: "Чи є домашні завдання?",
    answer: "Так, ми додаємо короткі та зрозумілі домашні завдання для закріплення матеріалу.",
  },
  {
    question: "Як визначається рівень дитини?",
    answer: "Після заявки ми зв’яжемося з вами та уточнимо рівень учня, щоб підібрати оптимальний формат.",
  },
  {
    question: "Чи можна змінити кількість уроків?",
    answer: "Так, кількість занять можна переглядати та коригувати залежно від потреб дитини.",
  },
  {
    question: "Як відбувається оплата?",
    answer: "Після підтвердження формату навчання ми пояснимо всі деталі оплати та підписання договору.",
  },
];

const contactMethods = [
  { label: "Telegram", value: "@school_placeholder" },
  { label: "Viber", value: "+38 (099) 000-00-00" },
  { label: "Телефон", value: "+38 (099) 000-00-00" },
  { label: "Email", value: "hello@school-placeholder.ua" },
];

export default function AboutUs() {
  const [selectedClass, setSelectedClass] = useState("3");
  const [selectedSubject, setSelectedSubject] = useState("Математика");
  const [selectedPlan, setSelectedPlan] = useState("10");
  const [menuOpen, setMenuOpen] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const formRef = useRef(null);
  const [formData, setFormData] = useState({
    parentName: "",
    childName: "",
    email: "",
    phone: "",
    childClass: "3",
    subject: "Математика",
    lessons: "10",
    comment: "",
  });

  const scrollToForm = () => {
    formRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
  };

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((prev) => ({ ...prev, [name]: value }));

    if (name === "childClass") {
      setSelectedClass(value);
    }

    if (name === "subject") {
      setSelectedSubject(value);
    }

    if (name === "lessons") {
      setSelectedPlan(value);
    }
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    setSubmitted(true);
  };

  return (
    <>
      <style>{`
        :root {
          --bg: #120223;
          --panel: #1b0d2f;
          --panel-soft: #25163d;
          --primary: #8C0286;
          --primary-dark: #C40361;
          --primary-soft: rgba(140, 2, 134, 0.18);
          --text: #f7ebff;
          --muted: #d8c2ef;
          --line: rgba(196, 3, 97, 0.2);
          --success: #b9efc9;
          --warning: #f7b955;
          --shadow: 0 18px 45px rgba(36, 10, 51, 0.35);
        }

        * { box-sizing: border-box; }

        html {
          scroll-behavior: smooth;
        }

        body {
          margin: 0;
          font-family: "Inter", "Segoe UI", sans-serif;
          background: linear-gradient(180deg, #120223 0%, #1a032f 100%);
          color: var(--text);
        }

        a {
          text-decoration: none;
          color: inherit;
        }

        button, input, textarea, select {
          font: inherit;
        }

        .school-page {
          min-height: 100vh;
          color: var(--text);
        }

        .container {
          width: min(1120px, calc(100% - 32px));
          margin: 0 auto;
        }

        .topbar {
          position: sticky;
          top: 0;
          z-index: 30;
          background: rgba(18, 2, 35, 0.88);
          backdrop-filter: blur(12px);
          border-bottom: 1px solid rgba(196, 3, 97, 0.28);
          box-shadow: 0 10px 25px rgba(18, 2, 35, 0.25);
        }

        .nav {
          display: flex;
          align-items: center;
          justify-content: space-between;
          min-height: 76px;
          gap: 20px;
        }

        .brand {
          display: inline-flex;
          align-items: center;
          gap: 10px;
          font-weight: 800;
          letter-spacing: -0.03em;
          font-size: clamp(1.1rem, 2vw, 1.5rem);
        }

        .brand-mark {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          width: 34px;
          height: 34px;
          border-radius: 12px;
          background: linear-gradient(135deg, var(--primary) 0%, #d52c96 100%);
          color: white;
          box-shadow: 0 10px 20px rgba(140, 2, 134, 0.24);
        }

        .nav-links {
          display: flex;
          align-items: center;
          gap: 18px;
          margin-left: auto;
        }

        .nav-select {
          appearance: none;
          border: 1px solid var(--line);
          background: rgba(255,255,255,0.04);
          color: var(--text);
          border-radius: 12px;
          padding: 11px 40px 11px 14px;
          min-width: 160px;
          outline: none;
          box-shadow: inset 0 0 0 1px rgba(255,255,255,0.06);
          background-image: linear-gradient(45deg, transparent 50%, var(--muted) 50%), linear-gradient(135deg, var(--muted) 50%, transparent 50%);
          background-position: calc(100% - 18px) calc(50% - 3px), calc(100% - 13px) calc(50% - 3px);
          background-size: 6px 6px, 6px 6px;
          background-repeat: no-repeat;
        }

        .cta-button,
        .primary-button,
        .secondary-button,
        .plan-button,
        .submit-button {
          border: none;
          border-radius: 14px;
          cursor: pointer;
          transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        .cta-button,
        .primary-button,
        .submit-button {
          background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
          color: white;
          box-shadow: 0 16px 24px rgba(140, 2, 134, 0.28);
        }

        .cta-button:hover,
        .primary-button:hover,
        .secondary-button:hover,
        .plan-button:hover,
        .submit-button:hover {
          transform: translateY(-1px);
        }

        .cta-button {
          padding: 12px 22px;
          font-weight: 700;
        }

        .mobile-menu {
          display: none;
        }

        .section {
          padding: 72px 0;
        }

        .hero {
          display: grid;
          grid-template-columns: 1.1fr 0.9fr;
          align-items: center;
          gap: 48px;
          padding: 68px 0 42px;
        }

        .eyebrow {
          display: inline-flex;
          align-items: center;
          gap: 8px;
          padding: 8px 12px;
          border-radius: 999px;
          background: var(--primary-soft);
          color: var(--primary-dark);
          font-weight: 700;
          font-size: 0.8rem;
          margin-bottom: 18px;
          letter-spacing: 0.02em;
        }

        .hero h1 {
          margin: 0;
          font-size: clamp(2.3rem, 5vw, 4.2rem);
          line-height: 1.04;
          letter-spacing: -0.05em;
        }

        .hero p {
          margin: 22px 0 0;
          color: var(--muted);
          font-size: 1.1rem;
          line-height: 1.7;
          max-width: 610px;
        }

        .hero-actions {
          display: flex;
          align-items: center;
          gap: 16px;
          margin-top: 30px;
          flex-wrap: wrap;
        }

        .primary-button {
          padding: 16px 26px;
          font-weight: 700;
        }

        .secondary-button {
          background: #fff;
          color: var(--text);
          border: 1px solid var(--line);
          padding: 16px 24px;
          font-weight: 700;
        }

        .hero-card {
          background: linear-gradient(180deg, #1d0f32 0%, #25163d 100%);
          border: 1px solid rgba(196, 3, 97, 0.2);
          border-radius: 28px;
          box-shadow: var(--shadow);
          padding: 26px;
        }

        .mini-card {
          background: linear-gradient(135deg, rgba(140,2,134,0.18), rgba(196,3,97,0.08));
          border: 1px solid rgba(196,3,97,0.24);
          border-radius: 18px;
          padding: 18px 18px 14px;
          margin-bottom: 18px;
        }

        .mini-label {
          font-size: 0.72rem;
          text-transform: uppercase;
          letter-spacing: 0.09em;
          color: var(--muted);
        }

        .mini-value {
          font-size: clamp(1.4rem, 3vw, 2rem);
          font-weight: 800;
          letter-spacing: -0.04em;
          margin-top: 10px;
        }

        .stat-list {
          display: grid;
          grid-template-columns: repeat(2, minmax(0, 1fr));
          gap: 12px;
        }

        .stat-item {
          background: rgba(255,255,255,0.03);
          border: 1px solid var(--line);
          border-radius: 16px;
          padding: 16px 14px;
        }

        .stat-item strong {
          display: block;
          font-size: 1.3rem;
          margin-bottom: 4px;
        }

        .selector-panel {
          background: rgba(255,255,255,0.04);
          border: 1px solid var(--line);
          border-radius: 28px;
          box-shadow: 0 18px 34px rgba(17, 5, 23, 0.2);
          padding: 26px;
        }

        .section-header {
          margin-bottom: 30px;
        }

        .section-header h2 {
          margin: 0;
          font-size: clamp(2rem, 4vw, 2.7rem);
          letter-spacing: -0.05em;
        }

        .selector-grid {
          display: grid;
          grid-template-columns: repeat(2, minmax(0, 1fr));
          gap: 28px;
        }

        .selector-block {
          background: rgba(255,255,255,0.03);
          border: 1px solid var(--line);
          border-radius: 22px;
          padding: 24px;
        }

        .selector-block label {
          display: block;
          margin-bottom: 16px;
          font-weight: 700;
          color: var(--text);
        }

        .chip-group {
          display: flex;
          flex-wrap: wrap;
          gap: 10px;
        }

        .chip {
          border: 1px solid var(--line);
          background: rgba(255,255,255,0.03);
          color: var(--text);
          border-radius: 999px;
          min-width: 68px;
          padding: 10px 16px;
          font-weight: 600;
          cursor: pointer;
          transition: all 0.2s ease;
        }

        .chip.active {
          background: linear-gradient(135deg, var(--primary) 0%, #c94ca2 100%);
          color: white;
          border-color: transparent;
          box-shadow: 0 10px 18px rgba(140, 2, 134, 0.22);
        }

        .selection-result {
          margin-top: 18px;
          font-size: 0.96rem;
          color: var(--muted);
          background: rgba(140, 2, 134, 0.12);
          border-radius: 14px;
          padding: 12px 14px;
        }

        .selection-result strong {
          color: var(--text);
        }

        .plans-grid,
        .steps-grid,
        .contact-grid,
        .faq-grid {
          display: grid;
          gap: 22px;
        }

        .plans-grid {
          grid-template-columns: repeat(3, minmax(0, 1fr));
          margin-top: 26px;
        }

        .plan-card {
          background: rgba(255,255,255,0.03);
          border: 1px solid var(--line);
          border-radius: 24px;
          padding: 24px 22px;
          display: flex;
          flex-direction: column;
          min-height: 100%;
          position: relative;
          box-shadow: 0 14px 26px rgba(17, 5, 23, 0.15);
        }

        .plan-card.featured {
          background: linear-gradient(180deg, rgba(140,2,134,0.1) 0%, rgba(255,255,255,0.03) 100%);
          border-color: rgba(196,3,97,0.35);
          box-shadow: 0 18px 30px rgba(140,2,134,0.15);
          transform: translateY(-6px);
        }

        .recommended {
          position: absolute;
          top: 16px;
          right: 16px;
          background: #eefbd9;
          color: #2c7f3f;
          border-radius: 999px;
          padding: 7px 11px;
          font-size: 0.7rem;
          font-weight: 800;
          letter-spacing: 0.04em;
          text-transform: uppercase;
        }

        .plan-card h3 {
          margin: 0 0 8px;
          font-size: 1.5rem;
          letter-spacing: -0.04em;
        }

        .plan-features {
          list-style: none;
          padding: 0;
          margin: 20px 0 18px;
          display: flex;
          flex-direction: column;
          gap: 10px;
          color: var(--muted);
        }

        .plan-features li {
          display: flex;
          align-items: center;
          gap: 10px;
        }

        .plan-features li::before {
          content: "✓";
          width: 22px;
          height: 22px;
          display: inline-flex;
          align-items: center;
          justify-content: center;
          border-radius: 50%;
          background: rgba(45, 157, 104, 0.10);
          color: var(--success);
          font-weight: 800;
        }

        .plan-button {
          width: 100%;
          margin-top: auto;
          background: rgba(255,255,255,0.04);
          color: var(--text);
          border: 1px solid var(--line);
          padding: 13px 16px;
          font-weight: 700;
        }

        .plan-card.featured .plan-button {
          background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
          color: white;
          border-color: transparent;
        }

        .steps-grid {
          grid-template-columns: repeat(4, minmax(0, 1fr));
          margin-top: 24px;
        }

        .step-card {
          background: rgba(255,255,255,0.03);
          border: 1px solid var(--line);
          border-radius: 22px;
          padding: 22px 18px;
        }

        .step-number {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          width: 42px;
          height: 42px;
          border-radius: 12px;
          background: var(--primary-soft);
          color: #fff;
          font-weight: 800;
          margin-bottom: 16px;
        }

        .step-card h3 {
          margin: 0 0 12px;
          font-size: 1.22rem;
        }

        .step-card p {
          margin: 0;
          color: var(--muted);
          line-height: 1.7;
        }

        .form-shell {
          display: grid;
          grid-template-columns: 0.8fr 1.2fr;
          gap: 28px;
          background: rgba(255,255,255,0.03);
          border: 1px solid rgba(196, 3, 97, 0.18);
          border-radius: 28px;
          box-shadow: var(--shadow);
          padding: 28px;
        }

        .form-intro {
          background: linear-gradient(180deg, rgba(140,2,134,0.08) 0%, rgba(196,3,97,0.04) 100%);
          border: 1px solid var(--line);
          border-radius: 22px;
          padding: 24px;
        }

        .form-intro h3 {
          margin-top: 0;
          margin-bottom: 10px;
          font-size: clamp(1.6rem, 3vw, 2.2rem);
          letter-spacing: -0.04em;
        }

        .form-intro p {
          color: var(--muted);
          line-height: 1.7;
          margin-bottom: 20px;
        }

        .contact-list {
          display: grid;
          gap: 12px;
        }

        .contact-item {
          padding: 14px 16px;
          border-radius: 14px;
          background: rgba(255,255,255,0.04);
          border: 1px solid var(--line);
        }

        .contact-item strong {
          display: block;
          margin-bottom: 2px;
        }

        .contact-item span {
          color: var(--muted);
        }

        .request-form {
          display: flex;
          flex-direction: column;
          gap: 18px;
        }

        .input-grid {
          display: grid;
          grid-template-columns: repeat(2, minmax(0, 1fr));
          gap: 18px;
        }

        .field {
          display: flex;
          flex-direction: column;
          gap: 8px;
        }

        .field label {
          font-weight: 700;
          color: var(--text);
        }

        .field input,
        .field select,
        .field textarea {
          width: 100%;
          border: 1px solid var(--line);
          border-radius: 14px;
          background: rgba(255,255,255,0.04);
          padding: 13px 14px;
          color: var(--text);
          outline: none;
          transition: border-color 0.2s ease, box-shadow 0.2s ease;
        }

        .field input:focus,
        .field select:focus,
        .field textarea:focus {
          border-color: rgba(196,3,97,0.6);
          box-shadow: 0 0 0 4px rgba(196,3,97,0.12);
        }

        .field textarea {
          min-height: 128px;
          resize: vertical;
        }

        .submit-button {
          width: fit-content;
          padding: 15px 26px;
          font-weight: 700;
        }

        .success-message {
          background: rgba(45, 157, 104, 0.08);
          border: 1px solid rgba(45, 157, 104, 0.18);
          color: #1e7d58;
          border-radius: 14px;
          padding: 14px 16px;
          font-weight: 600;
        }

        .form-note {
          margin: 0;
          color: var(--muted);
          font-size: 0.95rem;
        }

        .contact-grid {
          grid-template-columns: repeat(4, minmax(0, 1fr));
          margin-top: 28px;
        }

        .contact-card {
          background: rgba(255,255,255,0.03);
          border: 1px solid var(--line);
          border-radius: 18px;
          padding: 22px 18px;
        }

        .contact-card strong {
          display: block;
          margin-bottom: 8px;
          font-size: 1.1rem;
        }

        .contact-card span {
          color: var(--muted);
          line-height: 1.6;
        }

        .faq-grid {
          grid-template-columns: 1fr;
          margin-top: 24px;
        }

        .faq-item {
          background: rgba(255,255,255,0.03);
          border: 1px solid var(--line);
          border-radius: 18px;
          overflow: hidden;
        }

        .faq-item summary {
          list-style: none;
          cursor: pointer;
          padding: 18px 20px;
          font-weight: 700;
          display: flex;
          justify-content: space-between;
          align-items: center;
          gap: 12px;
        }

        .faq-item summary::-webkit-details-marker {
          display: none;
        }

        .faq-item summary::after {
          content: "+";
          font-size: 1.4rem;
          color: var(--primary-dark);
        }

        .faq-item[open] summary::after {
          content: "−";
        }

        .faq-answer {
          color: var(--muted);
          line-height: 1.7;
          padding: 0 20px 18px;
        }

        .site-footer {
          margin-top: 40px;
          border-top: 1px solid rgba(196,3,97,0.18);
          background: rgba(18,2,35,0.7);
        }

        .footer-inner {
          min-height: 90px;
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 12px;
          color: var(--muted);
        }

        @media (max-width: 980px) {
          .hero,
          .form-shell,
          .selector-grid,
          .plans-grid,
          .steps-grid,
          .contact-grid {
            grid-template-columns: 1fr 1fr;
          }

          .hero {
            grid-template-columns: 1fr;
          }

          .form-shell {
            grid-template-columns: 1fr;
          }

          .nav-links {
            position: absolute;
            top: 76px;
            left: 16px;
            right: 16px;
            background: rgba(255,255,255,0.96);
            border: 1px solid var(--line);
            border-radius: 18px;
            box-shadow: var(--shadow);
            padding: 16px;
            display: none;
            flex-direction: column;
            align-items: stretch;
          }

          .nav-links.open {
            display: flex;
          }

          .nav-select {
            width: 100%;
          }

          .cta-button {
            display: none;
          }

          .mobile-menu {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 44px;
            height: 44px;
            border: 1px solid var(--line);
            border-radius: 12px;
            background: white;
            cursor: pointer;
          }
        }

        @media (max-width: 720px) {
          .section {
            padding: 54px 0;
          }

          .hero {
            padding-top: 46px;
          }

          .selector-grid,
          .plans-grid,
          .steps-grid,
          .contact-grid,
          .input-grid {
            grid-template-columns: 1fr;
          }

          .plan-card.featured {
            transform: none;
          }

          .section-header h2 {
            font-size: 2rem;
          }

          .footer-inner {
            flex-direction: column;
            justify-content: center;
            padding: 18px 0;
            text-align: center;
          }
        }
      `}</style>

      <div className="school-page">
        <header className="topbar">
          <div className="container nav">
            <a href="#home" className="brand" aria-label="На головну">
              <span className="brand-mark">A</span>
              <span>Akademia</span>
            </a>

            <nav className={`nav-links ${menuOpen ? "open" : ""}`} aria-label="Основна навігація">
              <select
                className="nav-select"
                value={selectedClass}
                onChange={(event) => {
                  const value = event.target.value;
                  setSelectedClass(value);
                  setFormData((prev) => ({ ...prev, childClass: value }));
                }}
                aria-label="Вибір класу"
              >
                {classOptions.map((grade) => (
                  <option key={grade} value={grade}>
                    Клас {grade}
                  </option>
                ))}
              </select>

              <select
                className="nav-select"
                value={selectedSubject}
                onChange={(event) => {
                  const value = event.target.value;
                  setSelectedSubject(value);
                  setFormData((prev) => ({ ...prev, subject: value }));
                }}
                aria-label="Вибір предмета"
              >
                {subjectOptions.map((subject) => (
                  <option key={subject} value={subject}>
                    {subject}
                  </option>
                ))}
              </select>
            </nav>

            <button className="cta-button" type="button" onClick={scrollToForm}>
              Записатися
            </button>

            <button
              type="button"
              className="mobile-menu"
              aria-label="Відкрити меню"
              onClick={() => setMenuOpen((prev) => !prev)}
            >
              ☰
            </button>
          </div>
        </header>

        <main id="home">
          <section className="container hero">
            <div>
              <div className="eyebrow">Онлайн-школа для дітей</div>
              <h1>Онлайн-навчання для школярів 1–5 класів</h1>
              <p>
                Математика та українська мова з викладачем онлайн. Оберіть клас і формат навчання —
                залиште заявку, і ми зв’яжемося з вами.
              </p>

              <div className="hero-actions">
                <button type="button" className="primary-button" onClick={scrollToForm}>
                  Обрати навчання
                </button>
                <button type="button" className="secondary-button" onClick={() => document.getElementById("plans")?.scrollIntoView({ behavior: "smooth" })}>
                  Переглянути формати
                </button>
              </div>
            </div>

            <div className="hero-card" aria-live="polite">
              <div className="mini-card">
                <div className="mini-label">Обраний клас</div>
                <div className="mini-value">{selectedClass} клас</div>
              </div>

              <div className="mini-card">
                <div className="mini-label">Предмет</div>
                <div className="mini-value">{selectedSubject}</div>
              </div>

              <div className="stat-list">
                <div className="stat-item">
                  <strong>45–60</strong>
                  хвилин
                </div>
                <div className="stat-item">
                  <strong>1–5</strong>
                  класи
                </div>
                <div className="stat-item">
                  <strong>Онлайн</strong>
                  формат
                </div>
                <div className="stat-item">
                  <strong>{selectedPlan}</strong>
                  уроків
                </div>
              </div>
            </div>
          </section>

          <section className="container section" aria-labelledby="selection-title">
            <div className="section-header">
              <h2 id="selection-title">Вибір класу та предмета</h2>
            </div>

            <div className="selector-panel">
              <div className="selector-grid">
                <div className="selector-block">
                  <label>Клас</label>
                  <div className="chip-group">
                    {classOptions.map((grade) => (
                      <button
                        type="button"
                        key={grade}
                        className={`chip ${selectedClass === grade ? "active" : ""}`}
                        onClick={() => {
                          setSelectedClass(grade);
                          setFormData((prev) => ({ ...prev, childClass: grade }));
                        }}
                      >
                        {grade}
                      </button>
                    ))}
                  </div>
                  <div className="selection-result">
                    Обрано: <strong>{selectedClass} клас</strong>
                  </div>
                </div>

                <div className="selector-block">
                  <label>Предмет</label>
                  <div className="chip-group">
                    {subjectOptions.map((subject) => (
                      <button
                        type="button"
                        key={subject}
                        className={`chip ${selectedSubject === subject ? "active" : ""}`}
                        onClick={() => {
                          setSelectedSubject(subject);
                          setFormData((prev) => ({ ...prev, subject }));
                        }}
                      >
                        {subject}
                      </button>
                    ))}
                  </div>
                  <div className="selection-result">
                    Обрано: <strong>{selectedSubject}</strong>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section id="plans" className="container section" aria-labelledby="plans-title">
            <div className="section-header">
              <h2 id="plans-title">Формати навчання</h2>
            </div>

            <div className="plans-grid">
              {plans.map((plan) => (
                <article key={plan.id} className={`plan-card ${plan.featured ? "featured" : ""}`}>
                  {plan.featured && <span className="recommended">Рекомендовано</span>}
                  <h3>{plan.title}</h3>
                  <ul className="plan-features">
                    <li>{plan.lessons}</li>
                    <li>{plan.format}</li>
                    <li>{plan.teacher}</li>
                    <li>{plan.homework}</li>
                  </ul>

                  <button
                    type="button"
                    className="plan-button"
                    onClick={() => {
                      setSelectedPlan(plan.id);
                      setFormData((prev) => ({ ...prev, lessons: plan.id }));
                      scrollToForm();
                    }}
                  >
                    Обрати
                  </button>
                </article>
              ))}
            </div>
          </section>

          <section className="container section" aria-labelledby="how-title">
            <div className="section-header">
              <h2 id="how-title">Як проходить навчання</h2>
            </div>

            <div className="steps-grid">
              {steps.map((step) => (
                <article key={step.number} className="step-card">
                  <div className="step-number">{step.number}</div>
                  <h3>{step.title}</h3>
                  <p>{step.text}</p>
                </article>
              ))}
            </div>
          </section>

          <section id="form" ref={formRef} className="container section" aria-labelledby="form-title">
            <div className="form-shell">
              <aside className="form-intro">
                <h3 id="form-title">Залиште заявку</h3>
                <p>
                  Оберіть клас, предмет і кількість занять — ми підберемо зручний формат навчання і
                  зателефонуємо вам для уточнення деталей.
                </p>

                <div className="contact-list">
                  {contactMethods.map((item) => (
                    <div className="contact-item" key={item.label}>
                      <strong>{item.label}</strong>
                      <span>{item.value}</span>
                    </div>
                  ))}
                </div>
              </aside>

              <form className="request-form" onSubmit={handleSubmit}>
                <div className="input-grid">
                  <div className="field">
                    <label htmlFor="parentName">Ім’я батька / матері</label>
                    <input
                      id="parentName"
                      name="parentName"
                      type="text"
                      value={formData.parentName}
                      onChange={handleChange}
                      placeholder="Наприклад: Олена"
                      required
                    />
                  </div>

                  <div className="field">
                    <label htmlFor="childName">Ім’я дитини</label>
                    <input
                      id="childName"
                      name="childName"
                      type="text"
                      value={formData.childName}
                      onChange={handleChange}
                      placeholder="Наприклад: Артем"
                      required
                    />
                  </div>

                  <div className="field">
                    <label htmlFor="email">Email</label>
                    <input
                      id="email"
                      name="email"
                      type="email"
                      value={formData.email}
                      onChange={handleChange}
                      placeholder="email@example.com"
                      required
                    />
                  </div>

                  <div className="field">
                    <label htmlFor="phone">Номер телефону</label>
                    <input
                      id="phone"
                      name="phone"
                      type="tel"
                      value={formData.phone}
                      onChange={handleChange}
                      placeholder="+38 (___) ___ __ __"
                      required
                    />
                  </div>

                  <div className="field">
                    <label htmlFor="childClass">Клас дитини</label>
                    <select
                      id="childClass"
                      name="childClass"
                      value={formData.childClass}
                      onChange={handleChange}
                    >
                      {classOptions.map((grade) => (
                        <option key={grade} value={grade}>
                          {grade}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="field">
                    <label htmlFor="subject">Предмет</label>
                    <select id="subject" name="subject" value={formData.subject} onChange={handleChange}>
                      {subjectOptions.map((subject) => (
                        <option key={subject} value={subject}>
                          {subject}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="field">
                    <label htmlFor="lessons">Бажана кількість уроків</label>
                    <select id="lessons" name="lessons" value={formData.lessons} onChange={handleChange}>
                      <option value="5">5</option>
                      <option value="10">10</option>
                      <option value="15">15</option>
                    </select>
                  </div>

                  <div className="field" />
                </div>

                <div className="field">
                  <label htmlFor="comment">Коментар / побажання</label>
                  <textarea
                    id="comment"
                    name="comment"
                    value={formData.comment}
                    onChange={handleChange}
                    placeholder="Напишіть, якщо у дитини є конкретні труднощі або побажання щодо навчання."
                  />
                </div>

                {submitted && (
                  <div className="success-message">
                    Дякуємо! Заявку отримано. Ми зв’яжемося з вами найближчим часом.
                  </div>
                )}

                <button type="submit" className="submit-button">
                  Залишити заявку
                </button>
                <p className="form-note">Ми зв’яжемося з вами, щоб уточнити деталі навчання.</p>
              </form>
            </div>
          </section>

          <section id="contacts" className="container section" aria-labelledby="contacts-title">
            <div className="section-header">
              <h2 id="contacts-title">Способи зв’язку</h2>
            </div>

            <div className="contact-grid">
              {contactMethods.map((item) => (
                <div className="contact-card" key={item.label}>
                  <strong>{item.label}</strong>
                  <span>{item.value}</span>
                </div>
              ))}
            </div>
          </section>

          <section id="faq" className="container section" aria-labelledby="faq-title">
            <div className="section-header">
              <h2 id="faq-title">FAQ</h2>
            </div>

            <div className="faq-grid">
              {faqs.map((faq) => (
                <details key={faq.question} className="faq-item">
                  <summary>{faq.question}</summary>
                  <div className="faq-answer">{faq.answer}</div>
                </details>
              ))}
            </div>
          </section>
        </main>

        <footer className="site-footer">
          <div className="container footer-inner">
            <div>© 2025 Akademia</div>
            <div>Онлайн-школа для дітей 1–5 класів</div>
          </div>
        </footer>
      </div>
    </>
  );
}
