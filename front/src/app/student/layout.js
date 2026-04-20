import StudentShell from "./StudentShell";

export const metadata = {
  title: "بوابة الطالب | مدرسة النور",
  description: "بوابة الطالب لعرض الجدول والدرجات والحضور والمدفوعات",
};

export default function StudentLayout({ children }) {
  return <StudentShell>{children}</StudentShell>;
}
