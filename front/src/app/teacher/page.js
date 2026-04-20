"use client";

import { useEffect, useState } from "react";
import { Award, Calendar, Clock, User, BookOpen, School } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function TeacherHomePage() {
  const [teacher,  setTeacher]  = useState(null);
  const [grades,   setGrades]   = useState([]);
  const [schedule, setSchedule] = useState([]);
  const [loading,  setLoading]  = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    const h = { Authorization: `Bearer ${token}` };

    Promise.all([
      fetch(`${API}/teachers/me`,          { headers: h }).then(r => r.ok ? r.json() : null),
      fetch(`${API}/grades/my-given`,       { headers: h }).then(r => r.ok ? r.json() : []),
      fetch(`${API}/timetable/me?pageSize=200`, { headers: h }).then(r => r.ok ? r.json() : { items: [] }),
    ]).then(([t, g, s]) => {
      setTeacher(t);
      setGrades(g ?? []);
      setSchedule(s?.items ?? []);
    }).finally(() => setLoading(false));
  }, []);

  const stats = [
    { icon: Award,    label: "علامات أدخلتها",  value: grades.length,   color: "#d4af37" },
    { icon: Calendar, label: "حصص أسبوعية",      value: schedule.length, color: "#60a5fa" },
    { icon: BookOpen, label: "المادة",            value: teacher?.subjectName ?? "—", color: "#4ade80", isText: true },
    { icon: School,   label: "الصف",             value: teacher?.className  ?? "—", color: "#f472b6", isText: true },
  ];

  if (loading) return <div className="stu-loading">جاري التحميل...</div>;

  return (
    <div>
      {/* Profile Card */}
      <div className="stu-profile-card">
        <div className="stu-profile-avatar">
          {teacher?.imagePath
            ? <img src={`https://localhost:7045${teacher.imagePath}`} alt="صورة الأستاذ" />
            : <User size={48} color="var(--gold)" />}
        </div>
        <div className="stu-profile-info">
          <h1 className="stu-profile-name">
            {teacher ? `${teacher.firstName} ${teacher.lastName}` : "—"}
          </h1>
          <p className="stu-profile-class">
            <BookOpen size={14} style={{ display: "inline", marginLeft: "4px" }} />
            {teacher?.subjectName ?? "—"}
          </p>
          <p style={{ color: "var(--gray)", fontSize: "0.85rem", marginTop: "0.3rem" }}>
            {teacher?.city ?? ""}
          </p>
        </div>
      </div>

      {/* Stats */}
      <div className="dash-stats-grid" style={{ marginTop: "2rem" }}>
        {stats.map(({ icon: Icon, label, value, color, isText }) => (
          <div key={label} className="dash-stat-card">
            <div className="dash-stat-icon" style={{ background: `${color}18` }}>
              <Icon size={24} color={color} />
            </div>
            <div className="dash-stat-info">
              <div className="dash-stat-value" style={{ color, fontSize: isText ? "1.1rem" : undefined }}>
                {value}
              </div>
              <div className="dash-stat-label">{label}</div>
            </div>
          </div>
        ))}
      </div>

      {/* Recent Grades */}
      {grades.length > 0 && (
        <div style={{ marginTop: "2.5rem" }}>
          <h2 style={{ fontSize: "1.1rem", color: "var(--gold)", marginBottom: "1rem" }}>
            آخر العلامات التي أدخلتها
          </h2>
          <div className="stu-table-wrap">
            <table className="stu-table">
              <thead>
                <tr>
                  <th>الطالب</th>
                  <th>المادة</th>
                  <th>النوع</th>
                  <th>العلامة</th>
                </tr>
              </thead>
              <tbody>
                {grades.slice(0, 5).map(g => (
                  <tr key={g.id}>
                    <td>{g.studentName}</td>
                    <td>{g.subjectName}</td>
                    <td><span className="stu-badge">{g.gradeType}</span></td>
                    <td>
                      <span style={{ color: g.score >= 50 ? "#4ade80" : "#f87171", fontWeight: 700 }}>
                        {g.score}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
