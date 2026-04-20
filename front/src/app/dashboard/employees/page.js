"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Plus, Trash2, Edit, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, Briefcase, AlertCircle, Check, Image as ImageIcon
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

export default function EmployeesPage() {
  const [employees, setEmployees] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form State
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    phone: "",
    city: "",
    jobTitle: "",
    salary: ""
  });
  const [imageFile, setImageFile] = useState(null);
  const [preview, setPreview] = useState(null);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  function setFieldValue(k, v) { setForm(p => ({ ...p, [k]: v })); }

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts(p => [...p, { id, msg, type }]);
    setTimeout(() => setToasts(p => p.filter(t => t.id !== id)), 3500);
  }

  const loadEmployees = useCallback(async (p = page) => {
    setLoading(true);
    try {
      const res = await fetch(`${API}/employees?pageNumber=${p}&pageSize=${pageSize}`, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? d;
      
      setEmployees(Array.isArray(items) ? items : []);
      setTotal(Array.isArray(d) ? items.length : (d.totalCount ?? d.count ?? items.length));
    } catch {
      toast("فشل تحميل الموظفين", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => { loadEmployees(page); }, [page, loadEmployees]);

  function pickImg(e) {
    const file = e.target.files[0];
    if (file) {
      setImageFile(file);
      setPreview(URL.createObjectURL(file));
    }
  }

  function openAddModal() {
    setForm({ firstName: "", lastName: "", phone: "", city: "", jobTitle: "", salary: "" });
    setImageFile(null);
    setPreview(null);
    setModal("add");
  }

  function openEditModal(item) {
    setSelected(item);
    setForm({
      firstName: item.firstName || "",
      lastName: item.lastName || "",
      phone: item.phone || "",
      city: item.city || "",
      jobTitle: item.jobTitle || "",
      salary: item.salary || ""
    });
    setImageFile(null);
    setPreview(item.imagePath ? `${IMG}${item.imagePath}` : null);
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    const formData = new FormData();
    formData.append("firstName", form.firstName);
    formData.append("lastName", form.lastName);
    formData.append("phone", form.phone);
    formData.append("city", form.city);
    formData.append("jobTitle", form.jobTitle);
    if (form.salary) formData.append("salary", form.salary);
    if (imageFile) formData.append("image", imageFile);

    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/employees` : `${API}/employees/${selected.id}`;
      
      const res = await fetch(endpoint, {
        method,
        headers: authHdr(false), // No Content-Type for FormData
        body: formData
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
      
      toast(modal === "add" ? "تم إضافة الموظف بنجاح" : "تم تعديل الموظف بنجاح");
      setModal(null);
      setSelected(null);
      loadEmployees(page);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذا الموظف؟")) return;
    try {
      const res = await fetch(`${API}/employees/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ أثناء الحذف", "error"); return; }
      toast("تم حذف الموظف بنجاح");
      loadEmployees();
    } catch {
      toast("خطأ في الاتصال", "error");
    }
  }

  return (
    <div className="stu-page">
      <div className="stu-toast-stack">
        {toasts.map(t => (
          <div key={t.id} className={`stu-toast stu-toast-${t.type}`}>
            {t.type === "success" ? <Check size={15} /> : <AlertCircle size={15} />}
            {t.msg}
          </div>
        ))}
      </div>

      <div className="stu-header">
        <div>
          <h1 className="stu-title"><Briefcase size={22} />  الموظفين والإداريين</h1>
          <p className="stu-subtitle">إدارة الكوادر والموظفين (إداريين، أمان، نظافة وغيرهم)</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadEmployees()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> إضافة موظف</button>
        </div>
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : employees.length === 0 ? (
          <div className="stu-empty"><Briefcase size={48} color="#333" /><p>لا يوجد موظفون مضافون بعد</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>الموظف</th>
                <th>المسمى الوظيفي</th>
                <th>رقم الهاتف</th>
                <th>المدينة</th>
                <th>الراتب الأساسي</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {employees.map(item => (
                <tr key={item.id}>
                  <td>
                    <div className="stu-td-profile">
                      <div className="stu-avatar">
                        {item.imagePath ? <img src={`${IMG}${item.imagePath}`} alt={item.firstName} /> 
                          : <span>{item.firstName?.[0]}{item.lastName?.[0]}</span>}
                      </div>
                      <div>
                        <div className="stu-td-name">{item.firstName} {item.lastName}</div>
                      </div>
                    </div>
                  </td>
                  <td><span className="stu-badge">{item.jobTitle || '—'}</span></td>
                  <td dir="ltr" style={{textAlign: "right"}}>{item.phone || '—'}</td>
                  <td>{item.city || '—'}</td>
                  <td><span className="stu-badge" style={{color: 'var(--gold)'}}>{item.salary ? item.salary : '—'}</span></td>
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
        <span className="stu-page-info">صفحة <strong>{page}</strong> من <strong>{totalPages}</strong></span>
        <button className="stu-page-btn" onClick={() => setPage(p => p + 1)} disabled={page === totalPages || employees.length < pageSize}><ChevronLeft size={16} /></button>
      </div>

      {modal && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "إضافة موظف جديد" : "تعديل الموظف"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-form-grid" style={{marginBottom: "1rem"}}>
                  
                  <div className="stu-fg img-fg" style={{gridColumn: '1 / -1', display: 'flex', gap: '1rem', alignItems: 'center'}}>
                    <div className="stu-avatar large">
                      {preview ? <img src={preview} alt="Preview" /> : <ImageIcon size={24} color="#777" />}
                    </div>
                    <label className="stu-upload-btn">
                      اختر صورة
                      <input type="file" accept="image/*" hidden onChange={pickImg} />
                    </label>
                  </div>

                  <div className="stu-fg">
                    <label>الاسم الأول *</label>
                    <input value={form.firstName} onChange={e => setFieldValue("firstName", e.target.value)} required />
                  </div>

                  <div className="stu-fg">
                    <label>الاسم الأخير *</label>
                    <input value={form.lastName} onChange={e => setFieldValue("lastName", e.target.value)} required />
                  </div>

                  <div className="stu-fg">
                    <label>رقم الهاتف</label>
                    <input dir="ltr" style={{textAlign: "right"}} value={form.phone} onChange={e => setFieldValue("phone", e.target.value)} />
                  </div>

                  <div className="stu-fg">
                    <label>المدينة</label>
                    <input value={form.city} onChange={e => setFieldValue("city", e.target.value)} />
                  </div>

                  <div className="stu-fg">
                    <label>المسمى الوظيفي *</label>
                    <input placeholder="مثال: إداري، منظف، حارس..." value={form.jobTitle} onChange={e => setFieldValue("jobTitle", e.target.value)} required />
                  </div>

                  <div className="stu-fg">
                    <label>الراتب الأساسي</label>
                    <input type="number" step="0.5" value={form.salary} onChange={e => setFieldValue("salary", e.target.value)} />
                  </div>

                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : (modal === "add" ? <Plus size={16} /> : <Edit size={16} />)}
                  {saving ? "جارٍ الحفظ..." : "حفظ الموظف"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
