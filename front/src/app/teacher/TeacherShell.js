"use client";

import { useState, useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import Link from "next/link";
import {
  GraduationCap,
  LayoutDashboard,
  Calendar,
  Award,
  Clock,
  LogOut,
  ChevronRight,
  ChevronLeft,
  User,
} from "lucide-react";

const NAV = [
  { href: "/teacher",              label: "الرئيسية",    icon: LayoutDashboard },
  { href: "/teacher/timetable",    label: "جدولي",       icon: Calendar },
  { href: "/teacher/grades",       label: "العلامات",    icon: Award },
  { href: "/teacher/availability", label: "أوقاتي",      icon: Clock },
];

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function TeacherShell({ children }) {
  const [collapsed, setCollapsed] = useState(false);
  const [teacher,   setTeacher]   = useState(null);
  const router   = useRouter();
  const pathname = usePathname();

  function isActive(href) {
    return href === "/teacher"
      ? pathname === "/teacher"
      : pathname.startsWith(href);
  }

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    if (!token) { router.replace("/login"); return; }

    fetch(`${API}/teachers/me`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : null)
      .then(d => { if (d) setTeacher(d); })
      .catch(() => {});
  }, [router]);

  async function logout() {
    const token = sessionStorage.getItem("accessToken");
    if (token) {
      await fetch(`${API}/auth/logout`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      }).catch(() => {});
    }
    sessionStorage.clear();
    router.replace("/login");
  }

  const currentLabel = NAV.find(n => isActive(n.href))?.label ?? "بوابة الأستاذ";

  return (
    <div className="dash-layout">
      <aside className={`dash-sidebar${collapsed ? " collapsed" : ""}`}>
        <div className="dash-sidebar-header">
          <div className="dash-brand">
            <GraduationCap size={22} color="var(--gold)" />
            <span className="dash-brand-text">بوابة الأستاذ</span>
          </div>
          <button
            className="dash-toggle"
            onClick={() => setCollapsed(c => !c)}
            aria-label="طي الشريط"
          >
            {collapsed ? <ChevronLeft size={18} /> : <ChevronRight size={18} />}
          </button>
        </div>

        <nav className="dash-nav">
          {NAV.map(({ href, label, icon: Icon }) => (
            <Link
              key={href}
              href={href}
              className={`dash-nav-link${isActive(href) ? " active" : ""}`}
              title={label}
            >
              <span className="dash-nav-icon"><Icon size={20} /></span>
              <span className="dash-nav-label">{label}</span>
            </Link>
          ))}
        </nav>

        <div className="dash-sidebar-footer">
          <div className="dash-avatar">
            <User size={15} color="var(--gold)" />
          </div>
          <div className="dash-user-info">
            <div className="dash-user-name">
              {teacher ? `${teacher.firstName} ${teacher.lastName}` : "جاري التحميل..."}
            </div>
            <div className="dash-user-role">{teacher?.specialization ?? "أستاذ"}</div>
          </div>
          <button className="btn-logout" onClick={logout} title="تسجيل الخروج">
            <LogOut size={17} />
          </button>
        </div>
      </aside>

      <div className={`dash-main${collapsed ? " sidebar-collapsed" : ""}`}>
        <header className="dash-topbar">
          <span className="dash-topbar-title">{currentLabel}</span>
          <span className="dash-topbar-user">
            {teacher ? `${teacher.firstName} ${teacher.lastName}` : ""}
          </span>
        </header>
        <main className="dash-content">{children}</main>
      </div>
    </div>
  );
}
