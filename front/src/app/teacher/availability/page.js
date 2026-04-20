"use client";

import { useEffect, useState } from "react";
import { Clock, Calendar } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

const DAYS = [
  { key: 0, label: "الأحد"    },
  { key: 1, label: "الاثنين"  },
  { key: 2, label: "الثلاثاء" },
  { key: 3, label: "الأربعاء" },
  { key: 4, label: "الخميس"  },
];

export default function TeacherAvailabilityPage() {
  const [availability, setAvailability] = useState([]);
  const [loading,      setLoading]      = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    fetch(`${API}/teacher-availability/me?pageSize=200`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : { items: [] })
      .then(d => setAvailability(d?.items ?? []))
      .finally(() => setLoading(false));
  }, []);

  const byDay = DAYS.map(({ key, label }) => ({
    label,
    items: availability
      .filter(a => a.dayOfWeek === key)
      .sort((a, b) => (a.startTime ?? "").localeCompare(b.startTime ?? "")),
  }));

  if (loading) return <div className="stu-loading">جاري تحميل أوقات التوافر...</div>;

  return (
    <div>
      <h1 className="dash-page-title">أوقات توفري</h1>
      <p style={{ color: "var(--gray)", marginBottom: "2rem", fontSize: "0.9rem" }}>
        <Calendar size={14} style={{ display: "inline", marginLeft: "4px" }} />
        الأوقات التي حددتها كمتاحة للتدريس
      </p>

      {availability.length === 0 ? (
        <div className="stu-empty">لم يتم إضافة أوقات توافر بعد</div>
      ) : (
        <div className="stu-timetable-grid">
          {byDay.map(({ label, items }) => (
            <div key={label} className="stu-day-col">
              <div className="stu-day-header">{label}</div>
              <div className="stu-day-slots">
                {items.length === 0 ? (
                  <div className="stu-no-class">غير متاح</div>
                ) : (
                  items.map(a => (
                    <div key={a.id} className="stu-slot-card" style={{ borderColor: "rgba(74,222,128, 0.4)" }}>
                      <div className="stu-slot-subject" style={{ color: "#4ade80" }}>
                        <Clock size={14} style={{ marginLeft: 5, flexShrink: 0 }} />
                        متاح
                      </div>
                      <div className="stu-slot-time">
                        {a.startTime?.slice(0, 5)} — {a.endTime?.slice(0, 5)}
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
