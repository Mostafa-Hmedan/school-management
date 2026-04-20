"use client";

import { useEffect, useState } from "react";
import { ClipboardList, CheckCircle, XCircle, Calendar } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function StudentAttendancePage() {
  const [records, setRecords] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    fetch(`${API}/attendances/me`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : [])
      .then(d => setRecords(d ?? []))
      .finally(() => setLoading(false));
  }, []);

  const total   = records.length;
  const present = records.filter(r => r.isPresent).length;
  const absent  = total - present;
  const pct     = total ? Math.round((present / total) * 100) : 0;

  if (loading) return <div className="stu-loading">جاري تحميل سجل الحضور...</div>;

  return (
    <div>
      <h1 className="dash-page-title">سجل حضوري</h1>

      {/* Stats */}
      <div className="dash-stats-grid" style={{ marginBottom: "2rem" }}>
        <div className="dash-stat-card">
          <div className="dash-stat-icon"><ClipboardList size={24} color="var(--gold)" /></div>
          <div className="dash-stat-info">
            <div className="dash-stat-value">{total}</div>
            <div className="dash-stat-label">إجمالي الحصص</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(74,222,128,.12)" }}>
            <CheckCircle size={24} color="#4ade80" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#4ade80" }}>{present}</div>
            <div className="dash-stat-label">حاضر</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(248,113,113,.12)" }}>
            <XCircle size={24} color="#f87171" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#f87171" }}>{absent}</div>
            <div className="dash-stat-label">غائب</div>
          </div>
        </div>
        {/* Progress bar card */}
        <div className="dash-stat-card" style={{ flexDirection: "column", alignItems: "flex-start", gap: "0.8rem" }}>
          <div style={{ display: "flex", justifyContent: "space-between", width: "100%" }}>
            <span style={{ color: "var(--gray)", fontSize: "0.85rem" }}>نسبة الحضور</span>
            <span style={{ color: pct >= 75 ? "#4ade80" : "#f87171", fontWeight: 700 }}>{pct}%</span>
          </div>
          <div className="stu-progress-track">
            <div
              className="stu-progress-fill"
              style={{
                width: `${pct}%`,
                background: pct >= 75 ? "linear-gradient(90deg,#166534,#4ade80)" : "linear-gradient(90deg,#7f1d1d,#f87171)"
              }}
            />
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--gray)" }}>
            {pct >= 75 ? "✓ نسبة ممتازة" : "⚠ نسبة منخفضة — انتبه!"}
          </div>
        </div>
      </div>

      {/* Table */}
      {records.length === 0 ? (
        <div className="stu-empty">لا توجد سجلات حضور</div>
      ) : (
        <div className="stu-table-wrap">
          <table className="stu-table">
            <thead>
              <tr>
                <th>#</th>
                <th>التاريخ</th>
                <th>المدرّس</th>
                <th>ملاحظات</th>
                <th>الحالة</th>
              </tr>
            </thead>
            <tbody>
              {[...records]
                .sort((a, b) => new Date(b.date) - new Date(a.date))
                .map((r, i) => (
                  <tr key={r.attendanceId ?? i}>
                    <td>{i + 1}</td>
                    <td>
                      <span style={{ display: "flex", alignItems: "center", gap: "4px" }}>
                        <Calendar size={13} />
                        {r.date ? new Date(r.date).toLocaleDateString("ar") : "—"}
                      </span>
                    </td>
                    <td>{r.teacherName ?? "—"}</td>
                    <td style={{ color: "var(--gray)", fontSize: "0.85rem" }}>{r.notes ?? "—"}</td>
                    <td>
                      {r.isPresent ? (
                        <span className="stu-status-badge pass">
                          <CheckCircle size={13} style={{ display: "inline", marginLeft: "3px" }} />
                          حاضر
                        </span>
                      ) : (
                        <span className="stu-status-badge fail">
                          <XCircle size={13} style={{ display: "inline", marginLeft: "3px" }} />
                          غائب
                        </span>
                      )}
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
