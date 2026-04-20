"use client";

import { useEffect, useState } from "react";
import { Award, TrendingUp } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function StudentGradesPage() {
  const [grades,  setGrades]  = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    fetch(`${API}/grades/me`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : [])
      .then(d => setGrades(d ?? []))
      .finally(() => setLoading(false));
  }, []);

  const gpa = grades.length
    ? (grades.reduce((a, g) => a + g.score, 0) / grades.length).toFixed(1)
    : null;

  const passed  = grades.filter(g => g.score >= 50).length;
  const failed  = grades.length - passed;

  // Group by subject
  const grouped = grades.reduce((acc, g) => {
    const key = g.subjectName ?? "غير محدد";
    if (!acc[key]) acc[key] = [];
    acc[key].push(g);
    return acc;
  }, {});

  if (loading) return <div className="stu-loading">جاري التحميل...</div>;

  return (
    <div>
      <h1 className="dash-page-title">علاماتي الدراسية</h1>

      {/* Summary */}
      <div className="dash-stats-grid" style={{ marginBottom: "2rem" }}>
        <div className="dash-stat-card">
          <div className="dash-stat-icon"><Award size={24} color="var(--gold)" /></div>
          <div className="dash-stat-info">
            <div className="dash-stat-value">{gpa ?? "—"}</div>
            <div className="dash-stat-label">المعدل العام</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(74,222,128,.12)" }}>
            <TrendingUp size={24} color="#4ade80" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#4ade80" }}>{passed}</div>
            <div className="dash-stat-label">مادة ناجح</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(248,113,113,.12)" }}>
            <Award size={24} color="#f87171" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#f87171" }}>{failed}</div>
            <div className="dash-stat-label">مادة راسب</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon"><Award size={24} color="#60a5fa" /></div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#60a5fa" }}>{grades.length}</div>
            <div className="dash-stat-label">إجمالي العلامات</div>
          </div>
        </div>
      </div>

      {grades.length === 0 ? (
        <div className="stu-empty">لا توجد علامات مسجلة بعد</div>
      ) : (
        Object.entries(grouped).map(([subject, items]) => {
          const avg = (items.reduce((a, g) => a + g.score, 0) / items.length).toFixed(1);
          return (
            <div key={subject} style={{ marginBottom: "2rem" }}>
              <div className="stu-subject-header">
                <span className="stu-subject-name">{subject}</span>
                <span className="stu-subject-avg">
                  المعدل: <strong style={{ color: avg >= 50 ? "#4ade80" : "#f87171" }}>{avg}</strong>
                </span>
              </div>
              <div className="stu-table-wrap">
                <table className="stu-table">
                  <thead>
                    <tr>
                      <th>#</th>
                      <th>نوع التقييم</th>
                      <th>المدرس</th>
                      <th>التاريخ</th>
                      <th>العلامة</th>
                      <th>الحالة</th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.map((g, i) => (
                      <tr key={g.id}>
                        <td>{i + 1}</td>
                        <td><span className="stu-badge">{g.gradeType}</span></td>
                        <td>{g.teacherName}</td>
                        <td>{g.dateGiven ? new Date(g.dateGiven).toLocaleDateString("ar") : "—"}</td>
                        <td>
                          <span className="stu-score" style={{ color: g.score >= 50 ? "#4ade80" : "#f87171" }}>
                            {g.score} / 100
                          </span>
                        </td>
                        <td>
                          <span className={`stu-status-badge ${g.score >= 50 ? "pass" : "fail"}`}>
                            {g.score >= 50 ? "ناجح ✓" : "راسب ✗"}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          );
        })
      )}
    </div>
  );
}
