"use client";

import { useEffect, useState } from "react";
import { Wallet, CreditCard, Calendar, TrendingDown } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function StudentPaymentsPage() {
  const [payments, setPayments] = useState([]);
  const [loading,  setLoading]  = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    fetch(`${API}/student-payments/me`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : [])
      .then(d => setPayments(d ?? []))
      .finally(() => setLoading(false));
  }, []);

  const totalAmount    = payments.reduce((a, p) => a + (p.totalAmount    ?? 0), 0);
  const totalPaid      = payments.reduce((a, p) => a + (p.paidAmount     ?? 0), 0);
  const totalRemaining = payments.reduce((a, p) => a + (p.remainingAmount ?? 0), 0);

  if (loading) return <div className="stu-loading">جاري تحميل المدفوعات...</div>;

  return (
    <div>
      <h1 className="dash-page-title">سجل مدفوعاتي</h1>

      {/* Summary */}
      <div className="dash-stats-grid" style={{ marginBottom: "2rem" }}>
        <div className="dash-stat-card">
          <div className="dash-stat-icon"><Wallet size={24} color="var(--gold)" /></div>
          <div className="dash-stat-info">
            <div className="dash-stat-value">
              {totalAmount.toLocaleString()}<span style={{ fontSize: "0.8rem", fontWeight: 400 }}> $</span>
            </div>
            <div className="dash-stat-label">إجمالي الرسوم</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(74,222,128,.12)" }}>
            <CreditCard size={24} color="#4ade80" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#4ade80" }}>
              {totalPaid.toLocaleString()}<span style={{ fontSize: "0.8rem", fontWeight: 400 }}> $</span>
            </div>
            <div className="dash-stat-label">المدفوع</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(248,113,113,.12)" }}>
            <TrendingDown size={24} color="#f87171" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: totalRemaining > 0 ? "#f87171" : "#4ade80" }}>
              {totalRemaining.toLocaleString()}<span style={{ fontSize: "0.8rem", fontWeight: 400 }}> $</span>
            </div>
            <div className="dash-stat-label">المتبقي</div>
          </div>
        </div>
        <div className="dash-stat-card">
          <div className="dash-stat-icon" style={{ background: "rgba(96,165,250,.12)" }}>
            <CreditCard size={24} color="#60a5fa" />
          </div>
          <div className="dash-stat-info">
            <div className="dash-stat-value" style={{ color: "#60a5fa" }}>{payments.length}</div>
            <div className="dash-stat-label">عدد الدفعات</div>
          </div>
        </div>
      </div>

      {/* Table */}
      {payments.length === 0 ? (
        <div className="stu-empty">لا توجد مدفوعات مسجلة</div>
      ) : (
        <div className="stu-table-wrap">
          <table className="stu-table">
            <thead>
              <tr>
                <th>#</th>
                <th>التاريخ</th>
                <th>إجمالي الرسوم</th>
                <th>المدفوع</th>
                <th>المتبقي</th>
                <th>الحالة</th>
              </tr>
            </thead>
            <tbody>
              {[...payments]
                .sort((a, b) => new Date(b.paymentDate ?? 0) - new Date(a.paymentDate ?? 0))
                .map((p, i) => (
                  <tr key={p.id ?? i}>
                    <td>{i + 1}</td>
                    <td>
                      <span style={{ display: "flex", alignItems: "center", gap: "4px" }}>
                        <Calendar size={13} />
                        {p.paymentDate ? new Date(p.paymentDate).toLocaleDateString("ar") : "—"}
                      </span>
                    </td>
                    <td style={{ color: "var(--gold)", fontWeight: 700 }}>
                      {(p.totalAmount ?? 0).toLocaleString()} $
                    </td>
                    <td style={{ color: "#4ade80", fontWeight: 700 }}>
                      {(p.paidAmount ?? 0).toLocaleString()} $
                    </td>
                    <td style={{ color: (p.remainingAmount ?? 0) > 0 ? "#f87171" : "#4ade80", fontWeight: 700 }}>
                      {(p.remainingAmount ?? 0).toLocaleString()} $
                    </td>
                    <td>
                      <span className={`stu-status-badge ${(p.remainingAmount ?? 0) <= 0 ? "pass" : "fail"}`}>
                        {(p.remainingAmount ?? 0) <= 0 ? "مسدّد ✓" : "متبقي"}
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
