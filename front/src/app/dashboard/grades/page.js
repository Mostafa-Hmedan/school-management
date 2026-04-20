"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Plus, Trash2, Edit, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, Award, AlertCircle, Check
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

export default function GradesPage() {
  const [grades, setGrades] = useState([]);
  const [studentsList, setStudentsList] = useState([]);
  const [teachersList, setTeachersList] = useState([]);
  const [subjectsList, setSubjectsList] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form
  const [form, setForm] = useState({
    score: "",
    gradeType: "Midterm",
    studentId: "",
    teacherId: "",
    subjectId: ""
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
      const p3 = fetch(`${API}/subjects?pageSize=100`, { headers: authHdr() }).then(r => r.json());
      const [r1, r2, r3] = await Promise.all([p1, p2, p3]);
      
      setStudentsList(Array.isArray(r1) ? r1 : r1.items ?? r1.data ?? []);
      setTeachersList(Array.isArray(r2) ? r2 : r2.items ?? r2.data ?? []);
      setSubjectsList(Array.isArray(r3) ? r3 : r3.items ?? r3.data ?? []);
    } catch { }
  }, []);

  const loadGrades = useCallback(async (p = page) => {
    setLoading(true);
    try {
      const res = await fetch(`${API}/grades?pageNumber=${p}&pageSize=${pageSize}`, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? d;
      
      setGrades(Array.isArray(items) ? items : []);
    } catch {
      toast("فشل تحميل الدرجات", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => { loadDependencies(); }, [loadDependencies]);
  useEffect(() => { loadGrades(page); }, [page, loadGrades]);

  function openAddModal() {
    setForm({
      score: "",
      gradeType: "Midterm",
      studentId: "",
      teacherId: "",
      subjectId: ""
    });
    setModal("add");
  }

  function openEditModal(item) {
    setSelected(item);
    setForm({
      score: item.score || "",
      gradeType: item.gradeType || "",
      studentId: studentsList.find(s => `${s.firstName} ${s.lastName}` === item.studentName)?.id || "",
      teacherId: teachersList.find(t => `${t.firstName} ${t.lastName}` === item.teacherName)?.id || "",
      subjectId: subjectsList.find(s => s.subjectName === item.subjectName)?.id || ""
    });
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    let body;
    if (modal === "add") {
      body = {
        Score: parseFloat(form.score),
        GradeType: form.gradeType,
        StudentId: parseInt(form.studentId),
        TeacherId: parseInt(form.teacherId),
        SubjectId: parseInt(form.subjectId)
      };
    } else {
      // The update endpoint expects only Score and GradeType
      body = {
        Score: parseFloat(form.score),
        GradeType: form.gradeType
      };
    }
    
    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/grades` : `${API}/grades/${selected.id}`;
      
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
      
      toast(modal === "add" ? "تم إضافة الدرجة بنجاح" : "تم التعديل بنجاح");
      setModal(null);
      setSelected(null);
      loadGrades(page);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذه الدرجة؟")) return;
    try {
      const res = await fetch(`${API}/grades/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ", "error"); return; }
      toast("تم حذف الدرجة بنجاح");
      loadGrades();
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
          <h1 className="stu-title"><Award size={22} /> الدرجات والتقييمات</h1>
          <p className="stu-subtitle">إدارة درجات الطلاب والاختبارات</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadGrades()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> رصد درجة جديدة</button>
        </div>
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : grades.length === 0 ? (
          <div className="stu-empty"><Award size={48} color="#333" /><p>لا توجد درجات مسجلة</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>الطالب</th>
                <th>المادة</th>
                <th>الدرجة</th>
                <th>نوع التقييم</th>
                <th>تاريخ الرصد</th>
                <th>الأستاذ</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {grades.map((item) => (
                <tr key={item.id}>
                  <td style={{fontWeight: 'bold'}}>{item.studentName}</td>
                  <td><span className="stu-badge">{item.subjectName}</span></td>
                  <td>
                      <span className="stu-badge" style={{color: 'var(--gold)'}}>{item.score}</span>
                  </td>
                  <td>{item.gradeType}</td>
                  <td dir="ltr" style={{textAlign: "right"}} className="stu-muted">{formatDate(item.dateGiven)}</td>
                  <td className="stu-muted">{item.teacherName}</td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-edit" onClick={() => openEditModal(item)}><Edit size={14} /></button>
                      <button className="stu-btn-del" onClick={() => handleDelete(item.id)}><Trash2 size={14} /></button>
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
        <button className="stu-page-btn" onClick={() => setPage(p => p + 1)} disabled={grades.length < pageSize}><ChevronLeft size={16} /></button>
      </div>

      {modal && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "رصد درجة جديدة" : "تعديل الدرجة"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-form-grid" style={{marginBottom: "1rem"}}>
                  
                  {modal === "add" && (
                    <>
                      <div className="stu-fg">
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
                      <div className="stu-fg" style={{gridColumn: '1 / -1'}}>
                        <label>الأستاذ *</label>
                        <select value={form.teacherId} onChange={e => setFieldValue("teacherId", e.target.value)} required>
                          <option value="">— اختر الأستاذ —</option>
                          {teachersList.map(t => <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>)}
                        </select>
                      </div>
                    </>
                  )}
                  
                  <div className="stu-fg">
                    <label>نوع التقييم *</label>
                    <select value={form.gradeType} onChange={e => setFieldValue("gradeType", e.target.value)} required>
                      <option value="Midterm">امتحان نصفي (Midterm)</option>
                      <option value="Final">امتحان نهائي (Final)</option>
                      <option value="Quiz">اختبار قصير (Quiz)</option>
                      <option value="Assignment">واجب (Assignment)</option>
                      <option value="Participation">مشاركة (Participation)</option>
                    </select>
                  </div>
                  <div className="stu-fg">
                    <label>الدرجة *</label>
                    <input type="number" step="0.5" min="0" value={form.score} onChange={e => setFieldValue("score", e.target.value)} required placeholder="مثال: 95.5" />
                  </div>

                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : (modal === "add" ? <Plus size={16} /> : <Edit size={16} />)}
                  {saving ? "جارٍ الحفظ..." : "حفظ الدرجة"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
