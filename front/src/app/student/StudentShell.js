"use client";

import { useState, useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import Link from "next/link";
import {
  GraduationCap,
  LayoutDashboard,
  BookOpen,
  ClipboardList,
  Award,
  Wallet,
  Calendar,
  LogOut,
  ChevronRight,
  ChevronLeft,
  User,
} from "lucide-react";

const NAV = [
  { href: "/student",           label: "الرئيسية",   icon: LayoutDashboard },
  { href: "/student/grades",    label: "علاماتي",    icon: Award },
  { href: "/student/timetable", label: "جدولي",      icon: Calendar },
  { href: "/student/attendance",label: "حضوري",      icon: ClipboardList },
  { href: "/student/payments",  label: "مدفوعاتي",  icon: Wallet },
];

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

export default function StudentShell({ children }) {
  const [collapsed, setCollapsed] = useState(false);
  const [student, setStudent] = useState(null);
  const router  = useRouter();
  const pathname = usePathname();

  function isActive(href) {
    return href === "/student"
      ? pathname === "/student"
      : pathname.startsWith(href);
  }

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    if (!token) { router.replace("/login"); return; }

    fetch(`${API}/students/me`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((r) => r.ok ? r.json() : null)
      .then((data) => { if (data) setStudent(data); })
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

  const currentLabel = NAV.find((n) => isActive(n.href))?.label ?? "بوابة الطالب";

  return (
    <div className="dash-layout">
      {/* ── Sidebar ── */}
      <aside className={`dash-sidebar${collapsed ? " collapsed" : ""}`}>
        <div className="dash-sidebar-header">
          <div className="dash-brand">
            <GraduationCap size={22} color="var(--gold)" />
            <span className="dash-brand-text">بوابة الطالب</span>
          </div>
          <button
            className="dash-toggle"
            onClick={() => setCollapsed((c) => !c)}
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
              {student ? `${student.firstName} ${student.lastName}` : "جاري التحميل..."}
            </div>
            <div className="dash-user-role">{student?.className ?? "طالب"}</div>
          </div>
          <button className="btn-logout" onClick={logout} title="تسجيل الخروج">
            <LogOut size={17} />
          </button>
        </div>
      </aside>

      {/* ── Main ── */}
      <div className={`dash-main${collapsed ? " sidebar-collapsed" : ""}`}>
        <header className="dash-topbar">
          <span className="dash-topbar-title">{currentLabel}</span>
          <span className="dash-topbar-user">
            {student ? `${student.firstName} ${student.lastName}` : ""}
          </span>
        </header>
        <main className="dash-content">{children}</main>
      </div>
    </div>
  );
}
