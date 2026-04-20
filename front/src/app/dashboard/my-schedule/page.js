"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Calendar, Loader2, Clock, Check, AlertCircle, GraduationCap, School, BookOpen
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

const DAYS = [
  { id: 0, name: "الأحد" },
  { id: 1, name: "الاثنين" },
  { id: 2, name: "الثلاثاء" },
  { id: 3, name: "الأربعاء" },
  { id: 4, name: "الخميس" },
  { id: 5, name: "الجمعة" },
  { id: 6, name: "السبت" }
];

function getToken() {
  return typeof window !== "undefined" ? sessionStorage.getItem("accessToken") : null;
}

function getUser() {
  if (typeof window === "undefined") return null;
  const stored = sessionStorage.getItem("user");
  return stored ? JSON.parse(stored) : null;
}

function authHdr(json = true) {
  const h = { Authorization: `Bearer ${getToken()}` };
  if (json) h["Content-Type"] = "application/json";
  return h;
}

export default function MySchedulePage() {
  const [user, setUser] = useState(null);
  const [profile, setProfile] = useState(null);
  const [schedules, setSchedules] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const loadSchedule = useCallback(async (u) => {
    setLoading(true);
    try {
      // 1. Identify Role and get specific profile info
      let profileRes;
      if (u.role === "Teacher") {
        profileRes = await fetch(`${API}/teachers/me`, { headers: authHdr() });
      } else if (u.role === "Student") {
        profileRes = await fetch(`${API}/students/me`, { headers: authHdr() });
      } else {
        setLoading(false);
        return; // Admin or other
      }

      if (!profileRes.ok) throw new Error("فشل تحميل الملف الشخصي");
      const pData = await profileRes.json();
      setProfile(pData);

      // 2. Load Table
      let timetableUrl;
      if (u.role === "Teacher") {
        timetableUrl = `${API}/timetable/teacher/${pData.id}?pageSize=100`;
      } else {
        timetableUrl = `${API}/timetable/class/${pData.classId}?pageSize=100`;
      }

      const tRes = await fetch(timetableUrl, { headers: authHdr() });
      const tData = await tRes.json();
      setSchedules(tData.items || []);

    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const u = getUser();
    if (u) {
      setUser(u);
      loadSchedule(u);
    } else {
      setLoading(false);
    }
  }, [loadSchedule]);

  const getSchedulesForDay = (dayId) => {
    return schedules
      .filter(s => s.dayOfWeek === dayId)
      .sort((a, b) => a.startTime.localeCompare(b.startTime));
  };

  if (loading) return (
    <div className="stu-loading" style={{ height: '70vh' }}>
      <Loader2 size={42} className="spin" color="var(--gold)" />
      <span>جارٍ تحميل جدولك الدراسي...</span>
    </div>
  );

  if (user?.role === "Admin") {
    return (
      <div className="stu-empty" style={{ height: '70vh' }}>
        <Calendar size={64} color="#333" />
        <h2>أهلاً بك أيها المدير</h2>
        <p>كمدير، ليس لديك جدول حصص شخصي. يمكنك إدارة جداول المدرسة من قسم "الجدول الدراسي".</p>
      </div>
    );
  }

  if (error) return (
    <div className="stu-empty" style={{ height: '70vh', color: '#f87171' }}>
      <AlertCircle size={48} />
      <p>{error}</p>
    </div>
  );

  return (
    <div className="stu-page">
      <div className="stu-header">
        <div>
          <h1 className="stu-title"><Calendar size={22} /> جدولي الأسبوعي</h1>
          <p className="stu-subtitle">
            {user?.role === "Teacher" ? (
              <>أهلاً أستاذ <strong>{profile?.firstName}</strong>. إليك جدول حصصك لهذا الأسبوع.</>
            ) : (
              <>جدول الحصص الأسبوعي لصف <strong>{profile?.className}</strong></>
            )}
          </p>
        </div>
      </div>

      <div className="timetable-horizontal-container">
        {DAYS.map(day => (
          <div key={day.id} className="t-day-row">
            <div className="t-day-label">{day.name}</div>
            <div className="t-day-content">
              {getSchedulesForDay(day.id).length === 0 ? (
                <div className="t-no-lessons">لا يوجد حصص</div>
              ) : (
                <div className="t-lessons-line">
                  {getSchedulesForDay(day.id).map(slot => (
                    <div key={slot.id} className="t-lesson-card">
                      <div className="t-lesson-time">
                        <Clock size={12} /> {slot.startTime.substring(0, 5)} - {slot.endTime.substring(0, 5)}
                      </div>
                      <div className="t-lesson-subject">{slot.subjectName}</div>
                      <div className="t-lesson-info">
                        {user?.role === "Teacher" ? (
                           <span title="الصف"><School size={10} /> صف: {slot.className}</span>
                        ) : (
                           <span title="الأستاذ"><GraduationCap size={10} /> أ. {slot.teacherName}</span>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>

      <style jsx>{`
        .timetable-horizontal-container {
          display: flex;
          flex-direction: column;
          gap: 1.25rem;
          margin-top: 1rem;
        }
        .t-day-row {
          display: grid;
          grid-template-columns: 120px 1fr;
          gap: 1rem;
          align-items: stretch;
        }
        .t-day-label {
          background: #111;
          color: var(--gold);
          display: flex;
          align-items: center;
          justify-content: center;
          font-weight: 700;
          border-radius: 12px;
          border: 1px solid #222;
        }
        .t-day-content {
          background: var(--black-card);
          padding: 1rem;
          border-radius: 12px;
          border: 1px solid #222;
          min-height: 80px;
        }
        .t-no-lessons {
          color: #444;
          font-size: 0.9rem;
          font-style: italic;
          display: flex;
          align-items: center;
          height: 100%;
        }
        .t-lessons-line {
          display: flex;
          flex-wrap: wrap;
          gap: 1rem;
        }
        .t-lesson-card {
          background: #181818;
          border: 1px solid #333;
          border-radius: 10px;
          padding: 0.75rem 1rem;
          min-width: 160px;
          border-right: 3px solid var(--gold);
        }
        .t-lesson-time {
          font-size: 0.72rem;
          color: var(--gold-dark);
          margin-bottom: 0.4rem;
          display: flex;
          align-items: center;
          gap: 0.3rem;
        }
        .t-lesson-subject {
          font-weight: 700;
          font-size: 0.95rem;
          margin-bottom: 0.25rem;
          color: var(--white);
        }
        .t-lesson-info {
          font-size: 0.78rem;
          color: var(--gray);
          display: flex;
          align-items: center;
          gap: 0.5rem;
        }
      `}</style>
    </div>
  );
}
