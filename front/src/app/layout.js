import "./globals.css";

export const metadata = {
  title: "مدرسة النور",
  description: "منظومة إدارة المدرسة",
};

export default function RootLayout({ children }) {
  return (
    <html lang="ar" dir="rtl">
      <body>{children}</body>
    </html>
  );
}
