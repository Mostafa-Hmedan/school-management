import DashboardShell from "./DashboardShell";

export const metadata = {
  title: "لوحة تحكم المدرسة",
};

export default function DashboardLayout({ children }) {
  return <DashboardShell>{children}</DashboardShell>;
}
