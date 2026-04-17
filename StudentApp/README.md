# Student Assessment Tracker — Angular Frontend

Angular 21 SPA frontend for the Student Assessment Tracker system. Communicates with the ASP.NET Core 8 backend via a REST API.

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| Angular | 21.1 | Framework (standalone components, zoneless) |
| TypeScript | ~5.9 | Language |
| RxJS | ~7.8 | Async/reactive state |
| DataTables.net | ^2.3 | Paginated/sortable tables |
| DataTables Buttons | ^3.2 | CSV export button |
| Vitest | ^4.0 | Unit test runner |
| Angular CLI | ^21.1.2 | Build tooling |

## Prerequisites

- Node.js 18+
- The backend API running at `http://localhost:5000` (see root `README.md`)

## Development

```bash
npm install       # install dependencies
npm start         # serve at http://localhost:4200 with API proxy
```

`npm start` runs `ng serve` which proxies all `/api` requests to `http://localhost:5000` via `proxy.conf.json`.

## Build

```bash
npm run build     # production build → dist/StudentApp/browser/
```

Copy the output to the API's `wwwroot/` to serve the SPA from the .NET host:

```powershell
Copy-Item dist/StudentApp/browser/* ../StudentAssessmentTrackerAPI/wwwroot/ -Force
```

## Unit Tests

```bash
ng test
```

Runs with [Vitest](https://vitest.dev/) (configured in `angular.json`). No Karma or Jest.

## Project Structure

```
src/app/
├── app.config.ts                 ← Bootstrap: router, HttpClient + authInterceptor
├── app.routes.ts                 ← Route definitions (see Routes section)
├── app.ts                        ← Root component (dual-session state watcher)
│
├── components/                   ← 10 standalone UI components
│   ├── login-form.component.ts          /login          Teacher login
│   ├── signup-form.component.ts         /register       Teacher registration
│   ├── student-list.component.ts        /               Student list (DataTables)
│   ├── student-detail.component.ts      /detail/:id     Student profile + assessments
│   ├── student-form.component.ts        /create, /edit/:id  Create/Edit student
│   ├── student-login.component.ts       /student/login  Student login + activation
│   ├── student-activate.component.ts    (superseded by dual-mode StudentLoginComponent)
│   ├── student-dashboard.component.ts   /student/dashboard  Student self-service
│   ├── admin-login.component.ts         /admin/login    Admin login
│   └── admin-dashboard.component.ts     /admin/dashboard  Admin panel (tabs)
│
├── core/
│   ├── guards/
│   │   ├── auth.guard.ts                Teachers only → /login
│   │   ├── guest.guard.ts               Block auth teachers from /login, /register
│   │   ├── student-auth.guard.ts        Students only → /student/login
│   │   ├── student-guest.guard.ts       Block auth students from /student/login
│   │   └── admin.guard.ts               Admin only (localStorage check)
│   ├── interceptors/
│   │   └── auth.interceptor.ts          Inject JWT; handle 401 → redirect to login
│   ├── models/
│   │   ├── student.model.ts             Student, Assessment, Submission DTOs
│   │   └── teacher.model.ts             Teacher, Subject DTOs
│   └── services/
│       ├── http/                        9 API services (one per backend controller)
│       │   ├── student-api.service.ts
│       │   ├── teacher-api.service.ts
│       │   ├── grade-api.service.ts
│       │   ├── subject-api.service.ts
│       │   ├── student-assessment-api.service.ts
│       │   ├── assessment-submission-api.service.ts
│       │   ├── report-api.service.ts
│       │   ├── admin-api.service.ts
│       │   └── class-group-api.service.ts
│       └── state/                       3 reactive BehaviorSubject state services
│           ├── teacher-state.service.ts         (localStorage-backed)
│           ├── student-auth-state.service.ts    (localStorage-backed)
│           └── student-state.service.ts         (in-memory)
│
└── features/                     Business logic layer
    ├── students/services/
    │   ├── student-business.service.ts       Student CRUD → updates StudentStateService
    │   └── student-auth-business.service.ts  Activate, login, logout
    └── teachers/services/
        └── teacher-business.service.ts       Login, register, logout
```

## Routes

| Path | Component | Guard |
|---|---|---|
| `/` | `StudentListComponent` | `authGuard` |
| `/create` | `StudentFormComponent` | `authGuard` |
| `/edit/:id` | `StudentFormComponent` | `authGuard` |
| `/detail/:id` | `StudentDetailComponent` | `authGuard` |
| `/login` | `LoginFormComponent` | `guestGuard` |
| `/register` | `SignUpFormComponent` | `guestGuard` |
| `/student/login` | `StudentLoginComponent` | `studentGuestGuard` |
| `/student/dashboard` | `StudentDashboardComponent` | `studentAuthGuard` |
| `/admin/login` | `AdminLoginComponent` | `adminGuestGuard` |
| `/admin/dashboard` | `AdminDashboardComponent` | `adminAuthGuard` |
| `/**` | redirect → `/` | — |

## Authentication

Two independent JWT sessions coexist in `localStorage`:

| Key | Holds | Used by |
|---|---|---|
| `sat_teacher_token` | Teacher JWT | `TeacherStateService`, `authInterceptor` |
| `sat_current_teacher` | Teacher profile JSON | `TeacherStateService` |
| `sat_student_token` | Student JWT | `StudentAuthStateService`, `authInterceptor` |
| `sat_current_student` | Student profile JSON | `StudentAuthStateService` |
| `admin_token` | Admin JWT | `AdminApiService`, `admin.guard.ts` |
| `admin_info` | Admin profile JSON | `AdminDashboardComponent` |

The `authInterceptor` injects the appropriate token per request (admin for `/api/admins`, else teacher → student fallback). On any `401` it clears the offending session and redirects to the matching login page.

## Adding a New Feature

1. **API service** — add a class in `core/services/http/` and export it from `core/services/http/index.ts`
2. **State** (if needed) — add a `BehaviorSubject`-based service in `core/services/state/`
3. **Business logic** — add a service in the matching `features/<domain>/services/` folder
4. **Component** — add a standalone component in `components/` and register the route in `app.routes.ts`
5. **Guard** — add a functional guard in `core/guards/` if the route needs protection

## CSS / Styling

- Global styles: `src/styles.scss`
- DataTables default theme CSS is loaded globally via `angular.json` (`datatables.net-dt/css/dataTables.dataTables.css`)
- All component styles are inline SCSS (no separate `.scss` files per component)
