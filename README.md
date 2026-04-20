# 🏫 School Management System

A full-stack school management system built with **ASP.NET Core 9** and **Next.js 16**, featuring JWT authentication, role-based access control, and a fully Arabic RTL dashboard.

---

## 📸 Screenshots

> _Add screenshots here after deploying_

---

## ✨ Features

### 🔐 Authentication & Security
- JWT Authentication with **Refresh Token rotation** (stored in database)
- **ASP.NET Identity** for user management (Admin / Teacher / Student roles)
- **Rate Limiting** — Fixed Window per IP (prevents brute-force)
- **FluentValidation** with Arabic error messages
- CORS configured for frontend origin

### 🎓 Academic Management
- **Students** — Full CRUD with profile image upload, search by name, pagination
- **Teachers** — Full CRUD with profile image, subject assignments
- **Classes** — Manage school classes with student/teacher counts
- **Subjects** — Curriculum subject management
- **Enrollments** — Enroll students into classes
- **Attendance** — Track daily student attendance per class
- **Grades** — Record and view student grades per subject

### 📅 Timetable System
- **Timetable Builder** — Assign subjects and teachers to class time slots
- **Teacher Availability** — Define available days/hours per teacher
- **Conflict Detection** — Automatic detection of scheduling conflicts
- **My Schedule** — Personal weekly schedule view

### 💰 Financial Management
- **Student Payments** — Track tuition fees, paid/remaining amounts
- **Teacher Payments** — Salary payment records
- **Employee Payments** — Staff payroll management

### 🖥️ Dashboard
- Collapsible sidebar with grouped navigation (4 categories)
- Overview stats (student count, teacher count, classes, subjects)
- Responsive Arabic RTL layout
- Toast notifications for all operations

---

## 🛠️ Tech Stack

### Backend
| Technology | Purpose |
|------------|---------|
| ASP.NET Core 9 | Web API framework |
| Entity Framework Core 9 | ORM (Code First) |
| SQL Server | Database |
| ASP.NET Identity | User management & roles |
| JWT Bearer | Authentication |
| FluentValidation | Request validation |
| In-Memory Cache | Caching with invalidation |
| Rate Limiter | IP-based rate limiting |
| Scalar / Swagger UI | API documentation |
| API Versioning | URL segment versioning (`/api/v1/`) |

### Frontend
| Technology | Purpose |
|------------|---------|
| Next.js 16 (App Router) | React framework |
| React 19 | UI library |
| Lucide React | Icons |
| Tailwind CSS 4 | Utility CSS |
| Custom CSS (globals.css) | Theming & components |

---

## 📁 Project Structure

```
schoole/
├── Back/                          # ASP.NET Core 9 API
│   ├── Controllers/V1/            # 14 API controllers
│   ├── Entities/                  # 16 database entities
│   ├── Services/                  # Business logic layer
│   ├── Interfaces/                # Service contracts
│   ├── Requestes/                 # Request DTOs + validation
│   ├── Responses/                 # Response DTOs with ToDto()
│   ├── Validators/                # FluentValidation rules
│   ├── Data/                      # DbContext + EF configurations
│   ├── Migrations/                # EF Core migrations
│   └── DependencyInjection.cs     # All service registrations
│
└── front/                         # Next.js 16 App
    └── src/app/
        ├── dashboard/
        │   ├── students/          # Student CRUD
        │   ├── teachers/          # Teacher CRUD
        │   ├── classes/           # Class management
        │   ├── subjects/          # Subject management
        │   ├── attendance/        # Attendance tracking
        │   ├── grades/            # Grade management
        │   ├── enrollments/       # Enrollment management
        │   ├── employees/         # Employee management
        │   ├── timetable-builder/ # Visual timetable builder
        │   ├── teacher-availability/
        │   ├── student-payments/
        │   ├── teacher-payments/
        │   ├── employee-payments/
        │   └── my-schedule/       # Personal schedule view
        ├── login/                 # Login page
        └── page.js                # Public landing page
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) (or SQL Server Express)

---

### Backend Setup

**1. Clone the repository**
```bash
git clone https://github.com/YOUR_USERNAME/school-management.git
cd school-management/Back
```

**2. Configure the database**

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SchoolDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "SchoolApi",
    "Audience": "SchoolClient",
    "ExpiryMinutes": 60
  }
}
```

**3. Apply migrations and seed data**
```bash
dotnet ef database update
```
> This creates the database and seeds a default **Admin** account:
> - Email: `admin@school.com`
> - Password: `Admin@123`

**4. Run the API**
```bash
dotnet run
```
API will be available at: `https://localhost:7045`  
Scalar UI (API docs): `https://localhost:7045/scalar`

---

### Frontend Setup

**1. Navigate to frontend folder**
```bash
cd ../front
```

**2. Install dependencies**
```bash
npm install
```

**3. Configure environment**

Create `.env.local`:
```env
NEXT_PUBLIC_API_URL=https://localhost:7045/api/v1
```

**4. Run the development server**
```bash
npm run dev
```
App will be available at: `http://localhost:3000`

---

## 🔌 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/login` | Login and receive JWT + RefreshToken |
| POST | `/api/v1/auth/logout` | Logout and revoke token |
| POST | `/api/v1/auth/refresh` | Refresh access token |
| GET | `/api/v1/auth/me` | Get current user info |

### Core Resources (all require Admin role)
| Resource | Base URL |
|----------|----------|
| Students | `/api/v1/students` |
| Teachers | `/api/v1/teachers` |
| Classes | `/api/v1/classes` |
| Subjects | `/api/v1/subjects` |
| Attendance | `/api/v1/attendance` |
| Grades | `/api/v1/grades` |
| Enrollments | `/api/v1/enrollments` |
| Employees | `/api/v1/employees` |
| Timetable | `/api/v1/timetable` |
| Teacher Availability | `/api/v1/teacher-availability` |
| Student Payments | `/api/v1/student-payments` |
| Teacher Payments | `/api/v1/teacher-payments` |
| Employee Payments | `/api/v1/employee-payments` |

All list endpoints support: `?pageNumber=1&pageSize=10`

---

## 🗄️ Database Schema

Key relationships:
- `Student` → `Class` (many-to-one)
- `Teacher` ↔ `Subject` (many-to-many via `StudentTeacher`)
- `ClassSchedule` → `Class` + `Subject` + `Teacher`
- `Enrollment` → `Student` + `Class`
- `Attendance` → `Student` + `Class`
- `StudentGrade` → `Student` + `Subject`
- `TeacherAvailability` → `Teacher` (days/times)
- All users linked to `AppUser` (ASP.NET Identity)

---

## 🎨 Design System

| Variable | Color | Usage |
|----------|-------|-------|
| `--gold` | `#d4af37` | Primary actions, titles |
| `--black` | `#0a0a0a` | Background |
| `--black-card` | `#1a1a1a` | Cards, tables |
| `--purple` | `#2d1b69` | Accents |
| `--gray` | `#9a9a9a` | Secondary text |

- Full **RTL** (Right-to-Left) Arabic layout
- All CSS in a single `globals.css` — no CSS-in-JS
- Custom scrollbar, hover effects, animations

---

## 🔒 Security Features

- JWT tokens expire after 60 minutes
- Refresh tokens are rotated on each use (old token revoked)
- Rate limiting: 100 requests/minute per IP on all endpoints
- Passwords hashed with ASP.NET Identity (PBKDF2)
- CORS restricted to frontend origin only
- `[Authorize(Roles = "Admin")]` on all write operations

---

## 👤 Author

**Mostafa Al-Hmedan**  
[GitHub](https://github.com/YOUR_USERNAME) · [LinkedIn](https://linkedin.com/in/YOUR_PROFILE)

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
