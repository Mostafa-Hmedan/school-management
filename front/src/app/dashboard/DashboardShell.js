"use client";

import { useState, useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import Link from "next/link";
import {
  GraduationCap,
  LayoutDashboard,
  Users,
  BookOpen,
  School,
  ClipboardList,
  Award,
  UserPlus,
  ChevronRight,
  ChevronLeft,
  LogOut,
  Wallet,
  WalletCards,
  Briefcase,
  HandCoins,
  ChevronDown,
  Calendar,
  Clock,
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

const NAV_GROUPS = [
  {
    title: "الرئيسية",
    items: [
      { href: "/dashboard", label: "لوحة التحكم", icon: LayoutDashboard },
      { href: "/dashboard/my-schedule", label: "جدولي الأسبوعي", icon: Calendar }
    ]
  },
  {
    title: "الأكاديمية والطلاب",
    items: [
      { href: "/dashboard/students", label: "الطلاب", icon: Users },
      { href: "/dashboard/classes", label: "الصفوف", icon: School },
      { href: "/dashboard/subjects", label: "المواد", icon: BookOpen },
      { href: "/dashboard/attendance", label: "الحضور", icon: ClipboardList },
      { href: "/dashboard/grades", label: "الدرجات", icon: Award },
      { href: "/dashboard/enrollments", label: "التسجيلات", icon: UserPlus },
      { href: "/dashboard/timetable-builder", label: "الجدول الدراسي", icon: Calendar },
    ]
  },
  {
    title: "الكوادر الإدارية",
    items: [
      { href: "/dashboard/teachers", label: "الأساتذة", icon: GraduationCap },
      { href: "/dashboard/teacher-availability", label: "أوقات الأساتذة", icon: Clock },
      { href: "/dashboard/employees", label: "الموظفين", icon: Briefcase },
    ]
  },
  {
    title: "الإدارة المالية",
    items: [
      { href: "/dashboard/student-payments", label: "مقبوضات الطلاب", icon: Wallet },
      { href: "/dashboard/teacher-payments", label: "مدفوعات الأساتذة", icon: WalletCards },
      { href: "/dashboard/employee-payments", label: "مدفوعات الموظفين", icon: HandCoins },
    ]
  }
];

export default function DashboardShell({ children }) {
  const [collapsed, setCollapsed] = useState(false);
  const [user, setUser] = useState(null);
  const router = useRouter();
  const pathname = usePathname();

  function isActive(href) {
    return href === "/dashboard"
      ? pathname === "/dashboard"
      : pathname.startsWith(href);
  }

  const [expandedGroups, setExpandedGroups] = useState(() => {
    const initials = { 0: true };
    NAV_GROUPS.forEach((g, i) => {
      if (g.items.some((item) => item.href === "/dashboard" ? pathname === "/dashboard" : pathname.startsWith(item.href))) {
        initials[i] = true;
      }
    });
    return initials;
  });

  function toggleGroup(idx) {
    if (collapsed) setCollapsed(false);
    setExpandedGroups((p) => ({ ...p, [idx]: !p[idx] }));
  }

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    if (!token) {
      router.replace("/login");
      return;
    }
    const stored = sessionStorage.getItem("user");
    if (stored) {
      const u = JSON.parse(stored);
      setUser(u);
      // حماية: الطالب والأستاذ لا يدخلون لوحة الأدمن
      const role = (u.role ?? "").toLowerCase();
      if (role === "student") { router.replace("/student"); return; }
      if (role === "teacher") { router.replace("/teacher"); return; }
    }
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

  const currentLabel =
    NAV_GROUPS.flatMap((g) => g.items).find((item) => isActive(item.href))?.label ?? "لوحة التحكم";

  return (
    <div className="dash-layout">
      {/* ── Sidebar ── */}
      <aside className={`dash-sidebar${collapsed ? " collapsed" : ""}`}>
        <div className="dash-sidebar-header">
          <div className="dash-brand">
            <GraduationCap size={22} color="var(--gold)" />
            <span className="dash-brand-text">مدرسة النور</span>
          </div>
          <button
            className="dash-toggle"
            onClick={() => setCollapsed((c) => !c)}
            aria-label="تبديل الشريط الجانبي"
          >
            {collapsed ? <ChevronLeft size={18} /> : <ChevronRight size={18} />}
          </button>
        </div>

        <nav className="dash-nav">
          {NAV_GROUPS.map((group, idx) => {
            const isExpanded = expandedGroups[idx];
            return (
            <div key={idx} className="dash-nav-group">
              {!collapsed && (
                <div className="dash-nav-group-title" onClick={() => toggleGroup(idx)}>
                  <span>{group.title}</span>
                  <ChevronDown size={14} className={`dash-chevron${isExpanded ? " expanded" : ""}`} />
                </div>
              )}
              {collapsed ? (
                group.items.map(({ href, label, icon: Icon }) => (
                  <Link key={href} href={href} className={`dash-nav-link${isActive(href) ? " active" : ""}`} title={label}>
                    <span className="dash-nav-icon"><Icon size={20} /></span>
                  </Link>
                ))
              ) : (
                <div className={`dash-nav-group-items${isExpanded ? " expanded" : ""}`}>
                  {group.items.map(({ href, label, icon: Icon }) => (
                    <Link key={href} href={href} className={`dash-nav-link${isActive(href) ? " active" : ""}`} title={label}>
                      <span className="dash-nav-icon"><Icon size={20} /></span>
                      <span className="dash-nav-label">{label}</span>
                    </Link>
                  ))}
                </div>
              )}
            </div>
            );
          })}
        </nav>

        <div className="dash-sidebar-footer">
          <div className="dash-avatar">
            <Users size={15} color="var(--gold)" />
          </div>
          <div className="dash-user-info">
            <div className="dash-user-name">
              {user?.firstName} {user?.lastName}
            </div>
            <div className="dash-user-role">مدير</div>
          </div>
          <button
            className="btn-logout"
            onClick={logout}
            title="تسجيل الخروج"
          >
            <LogOut size={17} />
          </button>
        </div>
      </aside>

      {/* ── Main ── */}
      <div className={`dash-main${collapsed ? " sidebar-collapsed" : ""}`}>
        <header className="dash-topbar">
          <span className="dash-topbar-title">{currentLabel}</span>
          <span className="dash-topbar-user">{user?.email}</span>
        </header>
        <main className="dash-content">{children}</main>
      </div>
    </div>
  );
}
