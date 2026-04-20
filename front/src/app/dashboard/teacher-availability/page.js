"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Clock, Plus, Trash2, Edit, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, GraduationCap, AlertCircle, Check
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

const DAYS = [
  { id: 0, name: "الأحد" },
  { id: 1, name: "الاثنين" },
  { id: 2, name: "الثلاثاء" },
  { id: 3, name: "الأربعاء" },
  { id: 4, name: "الخميس" },
  { id: 5, name: "الجمعة" },
  { id: 6, name: "السبت" }
];

function getToken() {
  return typeof window !== "undefined" ? sessionStorage.getItem("accessToken") : null;
}

function authHdr(json = true) {
  const h = { Authorization: `Bearer ${getToken()}` };
  if (json) h["Content-Type"] = "application/json";
  return h;
}

export default function TeacherAvailabilityPage() {
  const [teachers, setTeachers] = useState([]);
  const [selectedTeacherId, setSelectedTeacherId] = useState("");
  const [availabilities, setAvailabilities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form State
  const [form, setForm] = useState({
    dayOfWeek: 0,
    startTime: "08:00",
    endTime: "14:00"
  });

  function setFieldValue(k, v) { setForm((p) => ({ ...p, [k]: v })); }

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  const loadTeachers = useCallback(async () => {
    try {
      const res = await fetch(`${API}/teachers?pageSize=1000`, { headers: authHdr() });
      const d = await res.json();
      setTeachers(Array.isArray(d) ? d : d.items ?? d.data ?? []);
    } catch { }
  }, []);

  const loadAvailabilities = useCallback(async (teacherId) => {
    if (!teacherId) {
      setAvailabilities([]);
      return;
    }
    setLoading(true);
    try {
      const res = await fetch(`${API}/teacher-availability/${teacherId}?pageSize=100`, { headers: authHdr() });
      const d = await res.json();
      setAvailabilities(d.items || []);
    } catch {
      toast("فشل تحميل أوقات التوفر", "error");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadTeachers(); }, [loadTeachers]);
  useEffect(() => { if (selectedTeacherId) loadAvailabilities(selectedTeacherId); }, [selectedTeacherId, loadAvailabilities]);

  function openAddModal() {
    if (!selectedTeacherId) {
      toast("يرجى اختيار أستاذ أولاً", "error");
      return;
    }
    setForm({ dayOfWeek: 0, startTime: "08:00", endTime: "14:00" });
    setModal("add");
  }

  function openEditModal(item) {
    setSelected(item);
    setForm({
      dayOfWeek: item.dayOfWeek,
      startTime: item.startTime.substring(0, 5),
      endTime: item.endTime.substring(0, 5)
    });
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    // Convert HH:mm to TimeSpan format HH:mm:ss
    const body = {
      DayOfWeek: parseInt(form.dayOfWeek),
      StartTime: form.startTime + ":00",
      EndTime: form.endTime + ":00"
    };

    if (modal === "add") body.TeacherId = parseInt(selectedTeacherId);

    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/teacher-availability` : `${API}/teacher-availability/${selected.id}`;
      
      const res = await fetch(endpoint, {
        method,
        headers: authHdr(),
        body: JSON.stringify(body)
      });
      
      if (!res.ok) { 
        toast("خطأ في البيانات أو تضارب في المواعيد", "error");
        return; 
      }
      
      toast(modal === "add" ? "تمت الإضافة بنجاح" : "تم التعديل بنجاح");
      setModal(null);
      loadAvailabilities(selectedTeacherId);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من الحذف؟")) return;
    try {
      const res = await fetch(`${API}/teacher-availability/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ", "error"); return; }
      toast("تم الحذف بنجاح");
      loadAvailabilities(selectedTeacherId);
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
          <h1 className="stu-title"><Clock size={22} /> أوقات توفر الأساتذة</h1>
          <p className="stu-subtitle">تحديد الفترات الزمنية المتاحة لكل أستاذ أسبوعياً</p>
        </div>
        <div className="stu-header-actions">
          <button className="btn-gold" onClick={openAddModal} disabled={!selectedTeacherId}>
            <Plus size={16} /> إضافة وقت توفر
          </button>
        </div>
      </div>

      <div className="stu-search-bar" style={{ maxWidth: 500 }}>
        <GraduationCap size={16} className="stu-search-icon" />
        <select 
          className="stu-search-input" 
          value={selectedTeacherId} 
          onChange={(e) => setSelectedTeacherId(e.target.value)}
          style={{ paddingRight: '2.5rem' }}
        >
          <option value="">— اختر الأستاذ لعرض أوقاته —</option>
          {teachers.map(t => <option key={t.id} value={t.id}>{t.firstName} {t.lastName} ({t.subjectName})</option>)}
        </select>
      </div>

      <div className="stu-table-wrap">
        {!selectedTeacherId ? (
          <div className="stu-empty">
            <GraduationCap size={48} color="#333" />
            <p>يرجى اختيار أستاذ لعرض وإدارة أوقات فراغه</p>
          </div>
        ) : loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : availabilities.length === 0 ? (
          <div className="stu-empty"><Clock size={48} color="#333" /><p>لا توجد فترات زمنية محددة لهذا الأستاذ</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>اليوم</th>
                <th>من الساعة</th>
                <th>إلى الساعة</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {availabilities.map((item) => (
                <tr key={item.id}>
                  <td><span className="stu-badge">{DAYS.find(d => d.id === item.dayOfWeek)?.name}</span></td>
                  <td dir="ltr" style={{ textAlign: 'right' }}>{item.startTime.substring(0, 5)}</td>
                  <td dir="ltr" style={{ textAlign: 'right' }}>{item.endTime.substring(0, 5)}</td>
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

      {modal && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "إضافة فترة توفر" : "تعديل فترة التوفر"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-fg">
                  <label>اليوم *</label>
                  <select value={form.dayOfWeek} onChange={e => setFieldValue("dayOfWeek", e.target.value)} required>
                    {DAYS.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                  </select>
                </div>
                <div className="stu-form-grid">
                  <div className="stu-fg">
                    <label>وقت البداية *</label>
                    <input type="time" value={form.startTime} onChange={e => setFieldValue("startTime", e.target.value)} required />
                  </div>
                  <div className="stu-fg">
                    <label>وقت النهاية *</label>
                    <input type="time" value={form.endTime} onChange={e => setFieldValue("endTime", e.target.value)} required />
                  </div>
                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : <Check size={16} />}
                  {saving ? "جارٍ الحفظ..." : "حفظ الفترة الزمنية"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
