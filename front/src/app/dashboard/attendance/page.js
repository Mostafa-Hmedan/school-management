"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Plus, Trash2, Edit, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, ClipboardList, AlertCircle, Check
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

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
  try { return new Date(d).toLocaleDateString("ar-IQ"); } catch { return d; }
}

export default function AttendancePage() {
  const [attendances, setAttendances] = useState([]);
  const [studentsList, setStudentsList] = useState([]);
  const [teachersList, setTeachersList] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form
  const [form, setForm] = useState({
    date: new Date().toISOString().split("T")[0],
    isPresent: true,
    notes: "",
    studentId: "",
    teacherId: ""
  });

  function setFieldValue(k, v) { setForm((p) => ({ ...p, [k]: v })); }

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  const loadDependencies = useCallback(async () => {
    try {
      const p1 = fetch(`${API}/students?pageSize=1000`, { headers: authHdr() }).then(r => r.json());
      const p2 = fetch(`${API}/teachers?pageSize=1000`, { headers: authHdr() }).then(r => r.json());
      const [r1, r2] = await Promise.all([p1, p2]);
      
      setStudentsList(Array.isArray(r1) ? r1 : r1.items ?? r1.data ?? []);
      setTeachersList(Array.isArray(r2) ? r2 : r2.items ?? r2.data ?? []);
    } catch { }
  }, []);

  const loadAttendances = useCallback(async (p = page) => {
    setLoading(true);
    try {
      const res = await fetch(`${API}/attendances?pageNumber=${p}&pageSize=${pageSize}`, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? d;
      
      setAttendances(Array.isArray(items) ? items : []);
    } catch {
      toast("فشل تحميل سجل الحضور", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => { loadDependencies(); }, [loadDependencies]);
  useEffect(() => { loadAttendances(page); }, [page, loadAttendances]);

  function openAddModal() {
    setForm({
      date: new Date().toISOString().split("T")[0],
      isPresent: true,
      notes: "",
      studentId: "",
      teacherId: ""
    });
    setModal("add");
  }

  function openEditModal(item) {
    setSelected(item);
    setForm({
      date: item.date || "",
      isPresent: item.isPresent ?? true,
      notes: item.notes || "",
      studentId: studentsList.find(s => `${s.firstName} ${s.lastName}` === item.studentName)?.id || "",
      teacherId: teachersList.find(t => `${t.firstName} ${t.lastName}` === item.teacherName)?.id || ""
    });
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    let body;
    if (modal === "add") {
      body = {
        Date: form.date,
        IsPresent: form.isPresent,
        Notes: form.notes,
        StudentId: parseInt(form.studentId),
        TeacherId: parseInt(form.teacherId)
      };
    } else {
      // The update endpoint expects only IsPresent and Notes
      body = {
        IsPresent: form.isPresent,
        Notes: form.notes
      };
    }
    
    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/attendances` : `${API}/attendances/${selected.attendanceId}`;
      
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
      
      toast(modal === "add" ? "تم تسجيل الحضور بنجاح" : "تم التعديل بنجاح");
      setModal(null);
      setSelected(null);
      loadAttendances(page);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذا السجل؟")) return;
    try {
      const res = await fetch(`${API}/attendances/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ", "error"); return; }
      toast("تم حذف السجل بنجاح");
      loadAttendances();
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
          <h1 className="stu-title"><ClipboardList size={22} /> سجل الحضور والغياب</h1>
          <p className="stu-subtitle">إدارة ومتابعة سجلات الحضور اليومية</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadAttendances()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> تسجيل حضور جديد</button>
        </div>
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : attendances.length === 0 ? (
          <div className="stu-empty"><ClipboardList size={48} color="#333" /><p>لا توجد سجلات بعد</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>التاريخ</th>
                <th>الطالب</th>
                <th>الحالة</th>
                <th>مسجل بواسطة</th>
                <th>ملاحظات</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {attendances.map((item) => (
                <tr key={item.attendanceId}>
                  <td dir="ltr" style={{textAlign: "right"}}>{formatDate(item.date)}</td>
                  <td style={{fontWeight: 'bold'}}>{item.studentName}</td>
                  <td>
                    {item.isPresent 
                      ? <span className="stu-badge" style={{color:'#6fcf6f', borderColor: '#2d5a2d', backgroundColor: '#1a2e1a'}}>حاضر</span> 
                      : <span className="stu-badge" style={{color:'#f87171', borderColor: '#5a2d2d', backgroundColor: '#2e1a1a'}}>غائب</span>}
                  </td>
                  <td className="stu-muted">{item.teacherName}</td>
                  <td className="stu-muted">{item.notes || "—"}</td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-edit" onClick={() => openEditModal(item)}><Edit size={14} /></button>
                      <button className="stu-btn-del" onClick={() => handleDelete(item.attendanceId)}><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div className="stu-pagination">
        <button className="stu-page-btn" onClick={() => setPage(p => p - 1)} disabled={page === 1}><ChevronRight size={16} /></button>
        <span className="stu-page-info">صفحة <strong>{page}</strong></span>
        <button className="stu-page-btn" onClick={() => setPage(p => p + 1)} disabled={attendances.length < pageSize}><ChevronLeft size={16} /></button>
      </div>

      {modal && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "تسجيل الحضور" : "تعديل السجل"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-form-grid" style={{marginBottom: "1rem"}}>
                  
                  {modal === "add" && (
                     <>
                      <div className="stu-fg" style={{gridColumn: '1 / -1'}}>
                        <label>التاريخ *</label>
                        <input type="date" value={form.date} onChange={e => setFieldValue("date", e.target.value)} required />
                      </div>
                      <div className="stu-fg">
                        <label>الطالب *</label>
                        <select value={form.studentId} onChange={e => setFieldValue("studentId", e.target.value)} required>
                          <option value="">— اختر الطالب —</option>
                          {studentsList.map(s => <option key={s.id} value={s.id}>{s.firstName} {s.lastName}</option>)}
                        </select>
                      </div>
                      <div className="stu-fg">
                        <label>الاستاذ الدفتر *</label>
                        <select value={form.teacherId} onChange={e => setFieldValue("teacherId", e.target.value)} required>
                          <option value="">— اختر الاستاذ —</option>
                          {teachersList.map(t => <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>)}
                        </select>
                      </div>
                    </>
                  )}
                  
                  <div className="stu-fg" style={{gridColumn: '1 / -1'}}>
                    <label>الحالة *</label>
                    <select value={form.isPresent ? "true" : "false"} onChange={e => setFieldValue("isPresent", e.target.value === "true")} required>
                      <option value="true">حاضر</option>
                      <option value="false">غائب</option>
                    </select>
                  </div>
                  <div className="stu-fg" style={{gridColumn: '1 / -1'}}>
                    <label>ملاحظات</label>
                    <input type="text" value={form.notes} onChange={e => setFieldValue("notes", e.target.value)} placeholder="مثال: غياب بعذر طبي..." />
                  </div>

                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : (modal === "add" ? <Plus size={16} /> : <Edit size={16} />)}
                  {saving ? "جارٍ الحفظ..." : "حفظ السجل"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
