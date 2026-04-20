import Link from "next/link";
import { BookOpen, Users, Award, GraduationCap, Star, LogIn } from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

async function getTeachers() {
  try {
    const res = await fetch(`${API}/teachers?pageSize=6&pageNumber=1`, {
      next: { revalidate: 3600 },
    });
    if (!res.ok) return [];
    const data = await res.json();
    return data.items ?? [];
  } catch {
    return [];
  }
}

const STATS = [
  { icon: <Users size={52} color="var(--gold)" />, value: "+500", label: "طالب" },
  { icon: <BookOpen size={52} color="var(--gold)" />, value: "+30", label: "مادة دراسية" },
  { icon: <Award size={52} color="var(--gold)" />, value: "+50", label: "أستاذ متخصص" },
  { icon: <Star size={52} color="var(--gold)" />, value: "15+", label: "سنة خبرة" },
];

const FEATURES = [
  { icon: <BookOpen size={36} color="var(--gold)" />, title: "مناهج متطورة", desc: "مناهج حديثة تواكب التطورات العلمية والتكنولوجية" },
  { icon: <Users size={36} color="var(--gold)" />, title: "كوادر مؤهلة", desc: "أساتذة متخصصون بخبرات طويلة في مجالاتهم" },
  { icon: <Award size={36} color="var(--gold)" />, title: "نتائج متميزة", desc: "سجل حافل بالإنجازات والتفوق الأكاديمي" },
];

export default async function HomePage() {
  const teachers = await getTeachers();

  return (
    <main>
      <nav className="navbar">
        <div className="navbar-brand">
          <GraduationCap size={28} color="var(--gold)" />
          مدرسة النور
        </div>
        <Link href="/login" className="btn-gold">
          <LogIn size={16} />
          تسجيل الدخول
        </Link>
      </nav>

      <section className="hero">
        <GraduationCap size={64} color="var(--gold)" className="hero-icon" />
        <h1>مدرسة النور</h1>
        <p>نسعى إلى بناء جيل واعٍ متعلم، نوفر بيئة تعليمية متكاملة بأحدث الأساليب والكفاءات التدريسية</p>
        <Link href="/login" className="btn-gold btn-gold-lg">
          ابدأ الآن
        </Link>
      </section>

      <section className="stats">
        <div className="stats-grid">
          {STATS.map((stat) => (
            <div key={stat.label}>
              <div className="stat-icon">{stat.icon}</div>
              <div className="stat-value">{stat.value}</div>
              <div className="stat-label">{stat.label}</div>
            </div>
          ))}
        </div>
      </section>

      <section className="section">
        <h2 className="section-title">لماذا مدرسة النور؟</h2>
        <div className="divider" />
        <p className="section-subtitle">نقدم تجربة تعليمية متميزة تجمع بين الأصالة والحداثة</p>
        <div className="features-grid">
          {FEATURES.map((f) => (
            <div key={f.title} className="card">
              <div className="feature-icon">{f.icon}</div>
              <h3 className="feature-title">{f.title}</h3>
              <p className="feature-desc">{f.desc}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="section-alt">
        <div className="section-inner">
          <h2 className="section-title">كادرنا التدريسي</h2>
          <div className="divider" />
          <p className="section-subtitle">نخبة من الأساتذة المتخصصين والمؤهلين</p>

          {teachers.length === 0 ? (
            <p className="section-subtitle">لا يوجد أساتذة حالياً</p>
          ) : (
            <div className="teachers-grid">
              {teachers.map((teacher) => (
                <div key={teacher.id} className="card">
                  <div className="teacher-avatar">
                    {teacher.imagePath
                      ? <img src={teacher.imagePath} alt={teacher.firstName} />
                      : <Users size={32} color="var(--gold)" />
                    }
                  </div>
                  <h3 className="teacher-name">{teacher.firstName} {teacher.lastName}</h3>
                  {teacher.subjectName && (
                    <p className="teacher-subject">{teacher.subjectName}</p>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </section>

      <footer className="footer">
        <GraduationCap size={20} color="var(--gold)" className="footer-icon" />
        <p>© 2025 مدرسة النور — جميع الحقوق محفوظة</p>
      </footer>
    </main>
  );
}
