"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { GraduationCap, Mail, Lock, LogIn, Eye, EyeOff } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function LoginPage() {
  const router = useRouter();
  const [form, setForm] = useState({ email: "", password: "" });
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await fetch(`${API}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
      });

      const data = await res.json();

      if (!res.ok) {
        setError(data.detail ?? "فشل تسجيل الدخول");
        return;
      }

      // حفظ الـ Token في sessionStorage
      sessionStorage.setItem("accessToken", data.token.accessToken);
      sessionStorage.setItem("refreshToken", data.token.refreshToken);
      sessionStorage.setItem("user", JSON.stringify({
        email: data.email,
        firstName: data.firstName,
        lastName: data.lastName,
        role: data.role,
      }));

      // توجيه حسب الدور
      const role = (data.role ?? "").toLowerCase();
      if (role === "student") {
        router.push("/student");
      } else if (role === "teacher") {
        router.push("/teacher");
      } else {
        router.push("/dashboard");
      }
    } catch {
      setError("حدث خطأ في الاتصال بالسيرفر");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="login-page">
      <div className="login-card">

        <div className="login-header">
          <GraduationCap size={48} color="var(--gold)" />
          <h1 className="login-title">مدرسة النور</h1>
          <p className="login-subtitle">سجّل دخولك للمتابعة</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">

          <div className="input-group">
            <label className="input-label">البريد الإلكتروني</label>
            <div className="input-wrapper">
              <Mail size={18} color="var(--gray)" className="input-icon" />
              <input
                type="email"
                className="input-field"
                placeholder="example@school.com"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                required
              />
            </div>
          </div>

          <div className="input-group">
            <label className="input-label">كلمة المرور</label>
            <div className="input-wrapper">
              <Lock size={18} color="var(--gray)" className="input-icon" />
              <input
                type={showPassword ? "text" : "password"}
                className="input-field"
                placeholder="••••••••"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                required
              />
              <button
                type="button"
                className="input-toggle"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword
                  ? <EyeOff size={18} color="var(--gray)" />
                  : <Eye size={18} color="var(--gray)" />
                }
              </button>
            </div>
          </div>

          {error && <p className="login-error">{error}</p>}

          <button type="submit" className="btn-gold login-btn" disabled={loading}>
            {loading ? "جارٍ الدخول..." : (
              <>
                <LogIn size={18} />
                تسجيل الدخول
              </>
            )}
          </button>

        </form>

        <Link href="/" className="login-back">
          العودة إلى الصفحة الرئيسية
        </Link>

      </div>
    </main>
  );
}
