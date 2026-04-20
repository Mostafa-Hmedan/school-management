"use client";
import { useState, useEffect } from "react";
import { Users, GraduationCap, School, BookOpen } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

async function fetchCount(endpoint, token) {
  try {
    const res = await fetch(`${API}/${endpoint}?pageSize=1&pageNumber=1`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) return 0;
    const data = await res.json();
    return data.totalCount ?? 0;
  } catch {
    return 0;
  }
}
export default function DashboardPage() {
  const [stats, setStats] = useState({
    students: 0,
    teachers: 0,
    classes: 0,
    subjects: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    if (!token) return;

    Promise.all([
      fetchCount("students", token),
      fetchCount("teachers", token),
      fetchCount("classes", token),
      fetchCount("subjects", token),
    ]).then(([students, teachers, classes, subjects]) => {
      setStats({ students, teachers, classes, subjects });
      setLoading(false);
    });
  }, []);

  const cards = [
    { label: "الطلاب", value: stats.students, icon: Users },
    { label: "الأساتذة", value: stats.teachers, icon: GraduationCap },
    { label: "الفصول", value: stats.classes, icon: School },
    { label: "المواد", value: stats.subjects, icon: BookOpen },
  ];

  return (
    <>
      <h2 className="dash-page-title">نظرة عامة</h2>
      <div className="dash-stats-grid">
        {cards.map(({ label, value, icon: Icon }) => (
          <div key={label} className="dash-stat-card">
            <div className="dash-stat-icon">
              <Icon size={24} color="var(--gold)" />
            </div>
            <div className="dash-stat-info">
              <div className="dash-stat-value">{loading ? "—" : value}</div>
              <div className="dash-stat-label">{label}</div>
            </div>
          </div>
        ))}
      </div>
    </>
  );
}
