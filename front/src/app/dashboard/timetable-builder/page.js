"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Calendar, Plus, Trash2, Edit, X,
  Loader2, School, AlertCircle, Check, Clock
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

export default function TimetableBuilderPage() {
  const [classes, setClasses] = useState([]);
  const [teachers, setTeachers] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [selectedClassId, setSelectedClassId] = useState("");
  const [schedules, setSchedules] = useState([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form State
  const [form, setForm] = useState({
    subjectId: "",
    teacherId: "",
    dayOfWeek: 0,
    startTime: "08:00",
    endTime: "09:00"
  });

  function setFieldValue(k, v) { setForm((p) => ({ ...p, [k]: v })); }

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 4500);
  }

  const loadDependencies = useCallback(async () => {
    try {
      const p1 = fetch(`${API}/classes?pageSize=1000`, { headers: authHdr() }).then(r => r.json());
      const p2 = fetch(`${API}/teachers?pageSize=1000`, { headers: authHdr() }).then(r => r.json());
      const p3 = fetch(`${API}/subjects?pageSize=1000`, { headers: authHdr() }).then(r => r.json());
      const [resClasses, resTeachers, resSubjects] = await Promise.all([p1, p2, p3]);
      
      setClasses(Array.isArray(resClasses) ? resClasses : resClasses.items ?? resClasses.data ?? []);
      setTeachers(Array.isArray(resTeachers) ? resTeachers : resTeachers.items ?? resTeachers.data ?? []);
      setSubjects(Array.isArray(resSubjects) ? resSubjects : resSubjects.items ?? resSubjects.data ?? []);
    } catch { }
  }, []);

  const loadSchedules = useCallback(async (classId) => {
    if (!classId) {
      setSchedules([]);
      return;
    }
    setLoading(true);
    try {
      const res = await fetch(`${API}/timetable/class/${classId}?pageSize=100`, { headers: authHdr() });
      const d = await res.json();
      setSchedules(d.items || []);
    } catch {
      toast("فشل تحميل الجدول الدراسي", "error");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadDependencies(); }, [loadDependencies]);
  useEffect(() => { if (selectedClassId) loadSchedules(selectedClassId); }, [selectedClassId, loadSchedules]);

  function openAddModal() {
    if (!selectedClassId) {
      toast("يرجى اختيار الصف أولاً", "error");
      return;
    }
    setForm({ subjectId: "", teacherId: "", dayOfWeek: 0, startTime: "08:00", endTime: "09:00" });
    setModal("add");
  }

  function openEditModal(item) {
    setSelected(item);
    setForm({
      subjectId: item.subjectId,
      teacherId: item.teacherId,
      dayOfWeek: item.dayOfWeek,
      startTime: item.startTime.substring(0, 5),
      endTime: item.endTime.substring(0, 5)
    });
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    const body = {
      ClassId: parseInt(selectedClassId),
      SubjectId: parseInt(form.subjectId),
      TeacherId: parseInt(form.teacherId),
      DayOfWeek: parseInt(form.dayOfWeek),
      StartTime: form.startTime + ":00",
      EndTime: form.endTime + ":00"
    };

    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/timetable` : `${API}/timetable/${selected.id}`;
      
      const res = await fetch(endpoint, {
        method,
        headers: authHdr(),
        body: JSON.stringify(body)
      });
      
      if (!res.ok) { 
        let errMsg = "حدث خطأ أثناء الحفظ";
        try {
          const errData = await res.json();
          errMsg = errData.detail || errData.title || errMsg;
        } catch {}
        toast(errMsg, "error");
        return; 
      }
      
      toast(modal === "add" ? "تمت إضافة الحصة للجدول بنجاح" : "تم تعديل الحصة بنجاح");
      setModal(null);
      loadSchedules(selectedClassId);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذه الحصة من الجدول؟")) return;
    try {
      const res = await fetch(`${API}/timetable/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ", "error"); return; }
      toast("تم حذف الحصة بنجاح");
      loadSchedules(selectedClassId);
    } catch {
      toast("خطأ في الاتصال", "error");
    }
  }

  // Helper to filter and sort schedules by day
  const getSchedulesForDay = (dayId) => {
    return schedules
      .filter(s => s.dayOfWeek === dayId)
      .sort((a, b) => a.startTime.localeCompare(b.startTime));
  };

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
          <h1 className="stu-title"><Calendar size={22} /> منشئ الجداول الدراسية</h1>
          <p className="stu-subtitle">بناء وتوزيع الحصص الأسبوعية لكل صف وشعبة</p>
        </div>
        <div className="stu-header-actions">
          <button className="btn-gold" onClick={openAddModal} disabled={!selectedClassId}>
            <Plus size={16} /> إضافة حصة للجدول
          </button>
        </div>
      </div>

      <div className="stu-search-bar tt-select-bar">
        <School size={16} className="stu-search-icon" />
        <select
          className="stu-search-input tt-class-select"
          value={selectedClassId}
          onChange={(e) => setSelectedClassId(e.target.value)}
        >
          <option value="">— اختر الصف لمعاينة جدوله —</option>
          {classes.map(c => <option key={c.id} value={c.id}>الصف: {c.classNumber}</option>)}
        </select>
      </div>

      <div className="timetable-grid-view">
        {!selectedClassId ? (
          <div className="stu-empty">
            <School size={48} color="#333" />
            <p>يرجى اختيار صف لعرض أو بناء الجدول الدراسي الخاص به</p>
          </div>
        ) : loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : (
          <div className="t-days-container">
            {DAYS.map(day => (
              <div key={day.id} className="t-day-column">
                <div className="t-day-header">{day.name}</div>
                <div className="t-day-slots">
                  {getSchedulesForDay(day.id).length === 0 ? (
                    <div className="t-empty-slot">لا يوجد حصص</div>
                  ) : (
                    getSchedulesForDay(day.id).map(slot => (
                      <div key={slot.id} className="t-slot-card">
                        <div className="t-slot-time">
                          <Clock size={12} /> {slot.startTime.substring(0, 5)} - {slot.endTime.substring(0, 5)}
                        </div>
                        <div className="t-slot-subject">{slot.subjectName}</div>
                        <div className="t-slot-teacher">{slot.teacherName}</div>
                        <div className="t-slot-actions">
                          <button onClick={() => openEditModal(slot)} title="تعديل"><Edit size={12} /></button>
                          <button onClick={() => handleDelete(slot.id)} title="حذف"><Trash2 size={12} /></button>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {modal && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card tt-modal" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "إضافة حصة دراسية" : "تعديل حصة دراسية"}</span>
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
                    <label>المادة *</label>
                    <select value={form.subjectId} onChange={e => setFieldValue("subjectId", e.target.value)} required>
                      <option value="">— اختر المادة —</option>
                      {subjects.map(s => <option key={s.id} value={s.id}>{s.subjectName}</option>)}
                    </select>
                  </div>
                  <div className="stu-fg">
                    <label>الأستاذ *</label>
                    <select value={form.teacherId} onChange={e => setFieldValue("teacherId", e.target.value)} required>
                      <option value="">— اختر الأستاذ —</option>
                      {teachers.map(t => <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>)}
                    </select>
                  </div>
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

                <div className="conflict-notice">
                  * سيقوم النظام تلقائياً بالتحقق من عدم تضارب وقت الأستاذ أو الصف مع حصص أخرى.
                </div>

                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : <Check size={16} />}
                  {saving ? "جارٍ الحفظ..." : "تأكيد إضافة الحصة"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}
