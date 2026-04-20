"use client";

import { useEffect, useState } from "react";
import { Calendar, Clock, BookOpen, User } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

const DAYS = [
  { key: 0, label: "الأحد"    },
  { key: 1, label: "الاثنين"  },
  { key: 2, label: "الثلاثاء" },
  { key: 3, label: "الأربعاء" },
  { key: 4, label: "الخميس"  },
];

export default function StudentTimetablePage() {
  const [schedule, setSchedule] = useState([]);
  const [loading,  setLoading]  = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    fetch(`${API}/timetable/me?pageSize=200`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : { items: [] })
      .then(d => setSchedule(d?.items ?? []))
      .finally(() => setLoading(false));
  }, []);

  // Group by dayOfWeek
  const byDay = DAYS.map(({ key, label }) => ({
    label,
    items: schedule
      .filter(s => s.dayOfWeek === key)
      .sort((a, b) => a.startTime?.localeCompare(b.startTime)),
  }));

  if (loading) return <div className="stu-loading">جاري تحميل الجدول...</div>;

  return (
    <div>
      <h1 className="dash-page-title">جدولي الأسبوعي</h1>
      <p style={{ color: "var(--gray)", marginBottom: "2rem", fontSize: "0.9rem" }}>
        <Calendar size={14} style={{ display: "inline", marginLeft: "4px" }} />
        جدول حصص فصلك الدراسي
      </p>

      {schedule.length === 0 ? (
        <div className="stu-empty">لم يتم إعداد الجدول بعد</div>
      ) : (
        <div className="stu-timetable-grid">
          {byDay.map(({ label, items }) => (
            <div key={label} className="stu-day-col">
              <div className="stu-day-header">{label}</div>
              <div className="stu-day-slots">
                {items.length === 0 ? (
                  <div className="stu-no-class">لا حصص</div>
                ) : (
                  items.map((s) => (
                    <div key={s.id} className="stu-slot-card">
                      <div className="stu-slot-subject">
                        <BookOpen size={14} style={{ marginLeft: "5px", flexShrink: 0 }} />
                        {s.subjectName}
                      </div>
                      <div className="stu-slot-teacher">
                        <User size={12} style={{ marginLeft: "4px", flexShrink: 0 }} />
                        {s.teacherName}
                      </div>
                      <div className="stu-slot-time">
                        <Clock size={12} style={{ marginLeft: "4px", flexShrink: 0 }} />
                        {s.startTime?.slice(0, 5)} — {s.endTime?.slice(0, 5)}
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
