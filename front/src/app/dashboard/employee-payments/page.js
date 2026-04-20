"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Plus, Trash2, Edit, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, HandCoins, AlertCircle, Check
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";
const IMG = "https://localhost:7045";

function getToken() {
  return typeof window !== "undefined" ? sessionStorage.getItem("accessToken") : null;
}

function authHdr(json = true) {
  const h = { Authorization: `Bearer ${getToken()}` };
  if (json) h["Content-Type"] = "application/json";
  return h;
}

function formatDate(d) {
  if (!d) return "—";
  try { return new Date(d).toLocaleDateString("ar-IQ", { year: "numeric", month: "short", day: "numeric" }); } catch { return d; }
}

export default function EmployeePaymentsPage() {
  const [payments, setPayments] = useState([]);
  const [employeesList, setEmployeesList] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form
  const [form, setForm] = useState({
    employeeId: "",
    totalAmount: "",
    paidAmount: "",
    paymentDate: new Date().toISOString().slice(0, 16)
  });

  function setFieldValue(k, v) { setForm((p) => ({ ...p, [k]: v })); }

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  const loadDependencies = useCallback(async () => {
    try {
      const p1 = fetch(`${API}/employees?pageNumber=1&pageSize=1000`, { headers: authHdr() }).then(r => r.json());
      const [r1] = await Promise.all([p1]);
      const list = Array.isArray(r1) ? r1 : (r1.items || r1.Items || r1.data || r1.Data || []);
      setEmployeesList(list);
    } catch (err) {
      console.error("Failed to load employees for dropdown", err);
    }
  }, []);

  const loadPayments = useCallback(async (p = page) => {
    setLoading(true);
    try {
      const res = await fetch(`${API}/employee-payments?pageNumber=${p}&pageSize=${pageSize}`, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? d;
      
      setPayments(Array.isArray(items) ? items : []);
    } catch {
      toast("فشل تحميل مدفوعات الموظفين", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => { loadDependencies(); }, [loadDependencies]);
  useEffect(() => { loadPayments(page); }, [page, loadPayments]);

  function openAddModal() {
    setForm({
      employeeId: "",
      totalAmount: "",
      paidAmount: "",
      paymentDate: new Date().toISOString().slice(0, 16)
    });
    setModal("add");
  }

  function openEditModal(item) {
    setSelected(item);
    setForm({
      employeeId: employeesList.find(e => `${e.firstName} ${e.lastName}` === item.employeeName)?.id || item.employeeId || "",
      totalAmount: item.totalAmount || "",
      paidAmount: item.paidAmount || "",
      paymentDate: item.paymentDate ? item.paymentDate.slice(0, 16) : new Date().toISOString().slice(0, 16)
    });
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    let body;
    if (modal === "add") {
      body = {
        EmployeeId: parseInt(form.employeeId),
        TotalAmount: parseFloat(form.totalAmount),
        PaidAmount: parseFloat(form.paidAmount),
        PaymentDate: new Date(form.paymentDate).toISOString()
      };
    } else {
      body = {
        TotalAmount: parseFloat(form.totalAmount),
        PaidAmount: parseFloat(form.paidAmount),
        PaymentDate: new Date(form.paymentDate).toISOString()
      };
    }
    
    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/employee-payments` : `${API}/employee-payments/${selected.id}`;
      
      const res = await fetch(endpoint, {
        method,
        headers: authHdr(),
        body: JSON.stringify(body)
      });
      
      if (!res.ok) { 
        let errStr = "حدث خطأ أثناء الحفظ";
        try {
          const err = await res.json();
          if (err.errors) errStr = Object.values(err.errors).flatMap(x => x).join(" | ");
          else if (err.detail) errStr = err.detail;
          else if (err.title) errStr = err.title;
        } catch {}
        toast(errStr, "error"); 
        return; 
      }
      
      toast(modal === "add" ? "تم إضافة الدفعة للموظف بنجاح" : "تم تعديل الدفعة بنجاح");
      setModal(null);
      setSelected(null);
      loadPayments(page);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذه الدفعة؟")) return;
    try {
      const res = await fetch(`${API}/employee-payments/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ", "error"); return; }
      toast("تم حذف الدفعة بنجاح");
      loadPayments();
    } catch {
      toast("خطأ في الاتصال", "error");
    }
  }

  return (
    <div className="stu-page">
      <div className="stu-toast-stack">
        {toasts.map((t) => (
          <div key={t.id} className={`stu-toast stu-toast-${t.type}`}>
            {t.type === "success" ? <Check size={15} /> : <AlertCircle size={15} />}
            {t.msg}
          </div>
        ))}
      </div>

      <div className="stu-header">
        <div>
          <h1 className="stu-title"><HandCoins size={22} /> مدفوعات الموظفين</h1>
          <p className="stu-subtitle">إدارة مستحقات ومدفوعات الموظفين وسجلات الرواتب</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadPayments()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> تسليم دفعة لموظف</button>
        </div>
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : payments.length === 0 ? (
          <div className="stu-empty"><HandCoins size={48} color="#333" /><p>لا توجد مدفوعات مسجلة</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>الموظف</th>
                <th>المسمى الوظيفي</th>
                <th>المبلغ الكلي</th>
                <th>كم استلم</th>
                <th>كم بقي</th>
                <th>تاريخ ووقت الدفعة</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {payments.map((item) => {
                const emp = employeesList.find(e => e.id === item.employeeId);
                return (
                <tr key={item.id}>
                  <td>
                    <div className="stu-td-profile" style={{display: 'flex', alignItems: 'center', gap: '10px'}}>
                      <div className="stu-avatar">
                        {emp?.imagePath 
                          ? <img src={`${IMG}${emp.imagePath}`} alt={item.employeeName} style={{width: '35px', height: '35px', borderRadius: '50%', objectFit: 'cover'}} /> 
                          : <span style={{width: '35px', height: '35px', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--gold)', color: '#fff'}}>{item.employeeName?.[0]}</span>}
                      </div>
                      <span style={{fontWeight: 'bold'}}>{item.employeeName}</span>
                    </div>
                  </td>
                  <td><span className="stu-badge">{item.jobTitle || "—"}</span></td>
                  <td><span className="stu-badge">{item.totalAmount}</span></td>
                  <td><span className="stu-badge" style={{color:'#6fcf6f', borderColor: '#2d5a2d', backgroundColor: '#1a2e1a'}}>{item.paidAmount}</span></td>
                  <td>
                    <span className="stu-badge" style={{color: item.remainingAmount > 0 ? '#f87171' : '#6fcf6f'}}>
                        {item.remainingAmount}
                    </span>
                  </td>
                  <td dir="ltr" style={{textAlign: "right"}} className="stu-muted">
                    {formatDate(item.paymentDate)}
                  </td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-edit" onClick={() => openEditModal(item)}><Edit size={14} /></button>
                      <button className="stu-btn-del" onClick={() => handleDelete(item.id)}><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      <div className="stu-pagination">
        <button className="stu-page-btn" onClick={() => setPage(p => p - 1)} disabled={page === 1}><ChevronRight size={16} /></button>
        <span className="stu-page-info">صفحة <strong>{page}</strong></span>
        <button className="stu-page-btn" onClick={() => setPage(p => p + 1)} disabled={payments.length < pageSize}><ChevronLeft size={16} /></button>
      </div>

      {modal && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "تسليم دفعة لموظف" : "تعديل دفعة الموظف"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-form-grid" style={{marginBottom: "1rem"}}>
                  
                  {modal === "add" && (
                    <div className="stu-fg" style={{gridColumn: '1 / -1'}}>
                      <label>الموظف *</label>
                      <select value={form.employeeId} onChange={e => setFieldValue("employeeId", e.target.value)} required>
                        <option value="">— اختر الموظف —</option>
                        {employeesList.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName} - {e.jobTitle}</option>)}
                      </select>
                    </div>
                  )}

                  <div className="stu-fg">
                    <label>المبلغ الكلي المقرر للموظف *</label>
                    <input type="number" step="0.5" min="0" value={form.totalAmount} onChange={e => setFieldValue("totalAmount", e.target.value)} required placeholder="مثال: 500" />
                  </div>

                  <div className="stu-fg">
                    <label>المبلغ المدفوع (المُسلّم) *</label>
                    <input type="number" step="0.5" min="0" value={form.paidAmount} onChange={e => setFieldValue("paidAmount", e.target.value)} required placeholder="مثال: 200" />
                  </div>

                  <div className="stu-fg" style={{gridColumn: '1 / -1'}}>
                    <label>تاريخ ووقت الدفعة *</label>
                    <input type="datetime-local" value={form.paymentDate} onChange={e => setFieldValue("paymentDate", e.target.value)} required />
                  </div>

                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : (modal === "add" ? <Plus size={16} /> : <Edit size={16} />)}
                  {saving ? "جارٍ الحفظ..." : "حفظ الدفعة"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
