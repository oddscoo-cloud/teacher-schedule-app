import React, { useRef, useState } from "react";
import "./AboutUs.css";

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
            <button className="cta-button" type="button" onClick={scrollToForm}>
              <a href="/login" className="login-link">Вхід</a>
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
