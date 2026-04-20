import React from "react";
import "../Styles/AdminSchedule.css";

function AdminScheduleTable({ selectedTeacher, daysOfWeek }) {
    const isLessonInTime = (lesson, timeStr) => {
        const [startH] = lesson.start.split(":").map(Number);
        const [endH] = lesson.end.split(":").map(Number);
        const currentH = Number(timeStr.split(":")[0]);

        return currentH >= startH && currentH < endH;
    };
    const getRowSpan = (lesson) => {
        const [startH] = lesson.start.split(":").map(Number);
        const [endH] = lesson.end.split(":").map(Number);
        return endH - startH;
    };
    const isLessonStart = (lesson, timeStr) => {
        return lesson.start === timeStr;
    };
    return (
        <div className="schedule-content">
          {selectedTeacher ? (
            <>
              <table>
                <thead>
                  <tr>
                    {daysOfWeek.map((day) => (
                      <th key={day}>{day}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {Array.from({ length: 10 }, (_, i) => {
                    const hour = 8 + i;
                    const timeStr = hour.toString().padStart(2, "0") + ":00";

                    return (
                      <tr key={i}>
                        {daysOfWeek.map((day) => {
                          const lessons = selectedTeacher.schedule.filter(
                            (s) => s.day === day && isLessonInTime(s, timeStr)
                          );
                          return <td>
                            {lessons.map((l, i) => (
                                <div key={i}>
                                {l.lesson} ({l.start}-{l.end})
                                </div>
                            ))}
                        </td>;
                        })}
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </>
          ) : (
            <p>Выберите учителя слева, чтобы увидеть расписание</p>
          )}
        </div>
    )
}

export default AdminScheduleTable;