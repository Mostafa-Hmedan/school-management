"use client";

import { useEffect, useState } from "react";
import { Award, User, BookOpen, Calendar } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function TeacherGradesPage() {
  const [grades,  setGrades]  = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    fetch(`${API}/grades/my-given`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : [])
      .then(d => setGrades(d ?? []))
      .finally(() => setLoading(false));
  }, []);

  const totalGrades = grades.length;
  const avgScore = totalGrades
    ? (grades.reduce((a, g) => a + g.score, 0) / totalGrades).toFixed(1)
    : "—";
    
  const passedCount = grades.filter(g => g.score >= 50).length;
  const failedCount = totalGrades - passedCount;

  if (loading) return <div className="stu-loading">جاري التحميل...</div>;

  return (
    <div>
      <h1 className="dash-page-title">العلامات التي أدخلتها</h1>

      {/* Summary */}
      <div className="dash-stats-grid" style={{ marginBottom: "2rem" }}>
        <div className="dash-stat-card">
          <div className="dash-stat-icon"><Award size={24} color="var(--gold)" /></div>
          <div className="dash-stat-info">
            <div className="dash-stat-value">{totalGrades}</div>
            <div className="dash-stat-label">إجمالي العلامات المسجلة</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(96,165,250,.12)" }}>
            <Award size={24} color="#60a5fa" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#60a5fa" }}>{avgScore}</div>
            <div className="dash-stat-label">متوسط درجات الطلاب</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(74,222,128,.12)" }}>
            <Award size={24} color="#4ade80" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#4ade80" }}>{passedCount}</div>
            <div className="dash-stat-label">علامة نجاح {`>`} (=50)</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(248,113,113,.12)" }}>
            <Award size={24} color="#f87171" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#f87171" }}>{failedCount}</div>
            <div className="dash-stat-label" >علامة رسوب {`<`} (50)</div>
          </div>
        </div>
      </div>

      {grades.length === 0 ? (
        <div className="stu-empty">لا توجد علامات مسجلة بعد</div>
      ) : (
        <div className="stu-table-wrap">
          <table className="stu-table">
            <thead>
              <tr>
                <th>#</th>
                <th>اسم الطالب</th>
                <th>المادة</th>
                <th>تاريخ الإدخال</th>
                <th>نوع التقييم</th>
                <th>العلامة</th>
                <th>الحالة</th>
              </tr>
            </thead>
            <tbody>
              {grades.map((g, i) => (
                <tr key={g.id}>
                  <td>{i + 1}</td>
                  <td>
                    <span style={{ display: "flex", alignItems: "center", gap: "5px" }}>
                       <User size={14} color="var(--gray)" />
                       {g.studentName}
                    </span>
                  </td>
                  <td>
                    <span style={{ display: "flex", alignItems: "center", gap: "5px" }}>
                      <BookOpen size={14} color="var(--gray)" />
                      {g.subjectName}
                    </span>
                  </td>
                  <td>
                    <span style={{ display: "flex", alignItems: "center", gap: "4px" }}>
                      <Calendar size={13} color="var(--gray)" />
                      {g.dateGiven ? new Date(g.dateGiven).toLocaleDateString("ar") : "—"}
                    </span>
                  </td>
                  <td><span className="stu-badge">{g.gradeType}</span></td>
                  <td>
                    <span className="stu-score" style={{ color: g.score >= 50 ? "#4ade80" : "#f87171" }}>
                      {g.score} / 100
                    </span>
                  </td>
                  <td>
                    <span className={`stu-status-badge ${g.score >= 50 ? "pass" : "fail"}`}>
                      {g.score >= 50 ? "ناجح" : "راسب"}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
