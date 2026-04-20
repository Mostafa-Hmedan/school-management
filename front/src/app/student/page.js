"use client";

import { useEffect, useState } from "react";
import { Award, ClipboardList, Wallet, Calendar, User, BookOpen } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

function authHeaders() {
  const token = sessionStorage.getItem("accessToken");
  return { Authorization: `Bearer ${token}` };
}

export default function StudentHomePage() {
  const [student,    setStudent]    = useState(null);
  const [grades,     setGrades]     = useState([]);
  const [attendance, setAttendance] = useState([]);
  const [payments,   setPayments]   = useState([]);
  const [loading,    setLoading]    = useState(true);

  useEffect(() => {
    const h = authHeaders();
    Promise.all([
      fetch(`${API}/students/me`,        { headers: h }).then(r => r.ok ? r.json() : null),
      fetch(`${API}/grades/me`,          { headers: h }).then(r => r.ok ? r.json() : []),
      fetch(`${API}/attendances/me`,     { headers: h }).then(r => r.ok ? r.json() : []),
      fetch(`${API}/student-payments/me`,{ headers: h }).then(r => r.ok ? r.json() : []),
    ]).then(([s, g, a, p]) => {
      setStudent(s);
      setGrades(g ?? []);
      setAttendance(a ?? []);
      setPayments(p ?? []);
    }).finally(() => setLoading(false));
  }, []);

  const gpa = grades.length
    ? (grades.reduce((acc, g) => acc + g.score, 0) / grades.length).toFixed(1)
    : "—";

  const presentCount = attendance.filter(a => a.isPresent).length;
  const attendancePct = attendance.length
    ? Math.round((presentCount / attendance.length) * 100)
    : "—";

  const totalPaid = payments.reduce((acc, p) => acc + (p.paidAmount ?? 0), 0);

  const stats = [
    { icon: Award,         label: "المعدل العام",      value: gpa,             unit: "/ 100", color: "#d4af37" },
    { icon: ClipboardList, label: "نسبة الحضور",       value: attendancePct,   unit: "%",     color: "#4ade80" },
    { icon: BookOpen,      label: "عدد المواد",         value: grades.length,   unit: "مادة",  color: "#60a5fa" },
    { icon: Wallet,        label: "إجمالي المدفوعات",  value: `${totalPaid}$`, unit: "",      color: "#f472b6" },
  ];

  if (loading) {
    return (
      <div style={{ textAlign: "center", paddingTop: "4rem", color: "var(--gray)" }}>
        <div className="spin" style={{ display: "inline-block", marginBottom: "1rem" }}>
          <Award size={32} color="var(--gold)" />
        </div>
        <p>جاري تحميل بياناتك...</p>
      </div>
    );
  }

  return (
    <div>
      {/* ── Profile Card ── */}
      <div className="stu-profile-card">
        <div className="stu-profile-avatar">
          {student?.imagePath
            ? <img src={`https://localhost:7045${student.imagePath}`} alt="صورة الطالب" />
            : <User size={48} color="var(--gold)" />}
        </div>
        <div className="stu-profile-info">
          <h1 className="stu-profile-name">
            {student ? `${student.firstName} ${student.lastName}` : "—"}
          </h1>
          <p className="stu-profile-class">
            <BookOpen size={14} style={{ display: "inline", marginLeft: "4px" }} />
            {student?.className ?? "—"}
          </p>
          <p className="stu-profile-city" style={{ color: "var(--gray)", fontSize: "0.85rem", marginTop: "0.3rem" }}>
            {student?.city ?? ""}
          </p>
        </div>
      </div>

      {/* ── Stats ── */}
      <div className="dash-stats-grid" style={{ marginTop: "2rem" }}>
        {stats.map(({ icon: Icon, label, value, unit, color }) => (
          <div key={label} className="dash-stat-card">
            <div className="dash-stat-icon" style={{ background: `${color}18` }}>
              <Icon size={24} color={color} />
            </div>
            <div className="dash-stat-info">
              <div className="dash-stat-value" style={{ color }}>
                {value} <span style={{ fontSize: "0.85rem", fontWeight: 400 }}>{unit}</span>
              </div>
              <div className="dash-stat-label">{label}</div>
            </div>
          </div>
        ))}
      </div>

      {/* ── Recent Grades ── */}
      {grades.length > 0 && (
        <div style={{ marginTop: "2.5rem" }}>
          <h2 style={{ fontSize: "1.1rem", color: "var(--gold)", marginBottom: "1rem" }}>
            آخر العلامات
          </h2>
          <div className="stu-table-wrap">
            <table className="stu-table">
              <thead>
                <tr>
                  <th>المادة</th>
                  <th>النوع</th>
                  <th>العلامة</th>
                </tr>
              </thead>
              <tbody>
                {grades.slice(0, 5).map((g) => (
                  <tr key={g.id}>
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
