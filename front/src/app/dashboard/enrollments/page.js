"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Plus, Trash2, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, UserPlus, AlertCircle, Check
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

export default function EnrollmentsPage() {
  const [enrollments, setEnrollments] = useState([]);
  const [studentsList, setStudentsList] = useState([]);
  const [subjectsList, setSubjectsList] = useState([]);
  const [classesList, setClassesList] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false); // only "add" is valid for enrollments
  const [toasts, setToasts] = useState([]);

  // Form
  const [form, setForm] = useState({
    studentId: "",
    subjectId: "",
    classId: ""
  });

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  function setFieldValue(k, v) { setForm((p) => ({ ...p, [k]: v })); }

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  const loadDependencies = useCallback(async () => {
    try {
      const p1 = fetch(`${API}/students?pageSize=1000`, { headers: authHdr() }).then(r => r.json());
      const p2 = fetch(`${API}/subjects?pageSize=100`, { headers: authHdr() }).then(r => r.json());
      const p3 = fetch(`${API}/classes?pageSize=100`, { headers: authHdr() }).then(r => r.json());
      const [r1, r2, r3] = await Promise.all([p1, p2, p3]);
      
      setStudentsList(Array.isArray(r1) ? r1 : r1.items ?? r1.data ?? []);
      setSubjectsList(Array.isArray(r2) ? r2 : r2.items ?? r2.data ?? []);
      setClassesList(Array.isArray(r3) ? r3 : r3.items ?? r3.data ?? []);
    } catch { }
  }, []);

  const loadEnrollments = useCallback(async (p = page) => {
    setLoading(true);
    try {
      const res = await fetch(`${API}/enrollments?pageNumber=${p}&pageSize=${pageSize}`, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? d;
      
      setEnrollments(Array.isArray(items) ? items : []);
      setTotal(Array.isArray(d) ? items.length : (d.totalCount ?? d.count ?? items.length));
    } catch {
      toast("فشل تحميل التسجيلات", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => { loadDependencies(); }, [loadDependencies]);
  useEffect(() => { loadEnrollments(page); }, [page, loadEnrollments]);

  function openAddModal() {
    setForm({
      studentId: "",
      subjectId: "",
      classId: ""
    });
    setModalOpen(true);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    const body = {
      StudentId: parseInt(form.studentId),
      SubjectId: parseInt(form.subjectId),
      ClassId: parseInt(form.classId)
    };
    
    try {
      const res = await fetch(`${API}/enrollments`, {
        method: "POST",
        headers: authHdr(),
        body: JSON.stringify(body)
      });
      
      if (!res.ok) { 
        let errStr = "حدث خطأ أثناء التسجيل";
        try {
          const err = await res.json();
          if (err.errors) errStr = Object.values(err.errors).flatMap(x => x).join(" | ");
          else if (err.detail) errStr = err.detail;
          else if (err.title) errStr = err.title;
        } catch {}
        toast(errStr, "error"); 
        return; 
      }
      
      toast("تم تسجيل الطالب بنجاح");
      setModalOpen(false);
      loadEnrollments(page);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من إلغاء تسجيل هذا الطالب؟ سيتم مسح ارتباطه بالمادة.")) return;
    try {
      const res = await fetch(`${API}/enrollments/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ", "error"); return; }
      toast("تم إلغاء التسجيل بنجاح");
      loadEnrollments(page);
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
          <h1 className="stu-title"><UserPlus size={22} /> تسجيلات الطلاب</h1>
          <p className="stu-subtitle">تسجيل الطلاب في المواد الدراسية والفصول</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadEnrollments()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> تسجيل طالب</button>
        </div>
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : enrollments.length === 0 ? (
          <div className="stu-empty"><UserPlus size={48} color="#333" /><p>لا توجد تسجيلات بعد</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>رقم</th>
                <th>الطالب</th>
                <th>المادة</th>
                <th>الفصل</th>
                <th>تاريخ التسجيل</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {enrollments.map((item, i) => (
                <tr key={item.id}>
                  <td className="stu-td-num">{(page - 1) * pageSize + i + 1}</td>
                  <td style={{fontWeight: 'bold'}}>{item.studentName}</td>
                  <td><span className="stu-badge">{item.subjectName}</span></td>
                  <td><span className="stu-badge" style={{color: 'var(--gold)'}}>{item.className}</span></td>
                  <td dir="ltr" style={{textAlign: "right"}} className="stu-muted">{formatDate(item.enrollmentDate)}</td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-del" onClick={() => handleDelete(item.id)} title="إلغاء التسجيل"><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {totalPages > 1 && (
        <div className="stu-pagination">
          <button className="stu-page-btn" onClick={() => setPage(p => p - 1)} disabled={page === 1}><ChevronRight size={16} /></button>
          <span className="stu-page-info">صفحة <strong>{page}</strong> من <strong>{totalPages}</strong></span>
          <button className="stu-page-btn" onClick={() => setPage(p => p + 1)} disabled={page === totalPages}><ChevronLeft size={16} /></button>
        </div>
      )}

      {modalOpen && (
        <div className="stu-modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">تسجيل طالب في مادة</span>
              <button className="stu-modal-close" onClick={() => setModalOpen(false)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-form-grid" style={{marginBottom: "1rem"}}>
                  
                  <div className="stu-fg" style={{gridColumn: '1 / -1'}}>
                    <label>الطالب *</label>
                    <select value={form.studentId} onChange={e => setFieldValue("studentId", e.target.value)} required>
                      <option value="">— اختر الطالب —</option>
                      {studentsList.map(s => <option key={s.id} value={s.id}>{s.firstName} {s.lastName}</option>)}
                    </select>
                  </div>
                  
                  <div className="stu-fg">
                    <label>المادة *</label>
                    <select value={form.subjectId} onChange={e => setFieldValue("subjectId", e.target.value)} required>
                      <option value="">— اختر المادة —</option>
                      {subjectsList.map(s => <option key={s.id} value={s.id}>{s.subjectName}</option>)}
                    </select>
                  </div>
                  
                  <div className="stu-fg">
                    <label>الفصل *</label>
                    <select value={form.classId} onChange={e => setFieldValue("classId", e.target.value)} required>
                      <option value="">— اختر الفصل —</option>
                      {classesList.map(c => <option key={c.id} value={c.id}>{c.classNumber}</option>)}
                    </select>
                  </div>

                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : <Plus size={16} />}
                  {saving ? "جارٍ الحفظ..." : "تسجيل الطالب"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
