import TeacherShell from "./TeacherShell";

export const metadata = {
  title: "بوابة الأستاذ | مدرسة النور",
  description: "بوابة الأستاذ لعرض الجدول والعلامات وأوقات التوافر",
};

export default function TeacherLayout({ children }) {
  return <TeacherShell>{children}</TeacherShell>;
}
