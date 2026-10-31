# STDMS - Student & Teacher Data Management System

## Project overview

STDMS is a server-rendered school-record management website built with ASP.NET Core MVC and Razor. It manages students, departments, courses, instructors, enrollments, attendance, and instructor office assignments. Entity Framework Core persists the records to SQL Server.

The live MVC application is configured to run locally at `http://localhost:5265` (or `https://localhost:7176` when using the HTTPS launch profile).

> The project also contains a separate Next.js app under `app/`, plus a duplicate set of generated Next.js files under `Views/app/`. These currently render a placeholder and are not connected to the MVC routes or SQL Server database. The functioning STDMS website is the ASP.NET MVC application, not the Next.js placeholder.

## Contents

- [Features and pages](#features-and-pages)
- [Sign-in and authorization](#sign-in-and-authorization)
- [User interface and design](#user-interface-and-design)
- [Backend architecture](#backend-architecture)
- [Database and data model](#database-and-data-model)
- [Seed data](#seed-data)
- [Technology stack](#technology-stack)
- [Run the MVC application](#run-the-mvc-application)
- [Project structure](#project-structure)
- [Current limitations and recommended work](#current-limitations-and-recommended-work)
- [Validation performed](#validation-performed)

## Features and pages

The shared navigation is responsive and exposes sections according to the signed-in role. It includes:

| Page | Route | Purpose |
|---|---|---|
| Dashboard | `/` or `/Home/Index` | Displays school totals, recently added students, today's attendance summary, and recent enrollments. |
| Students | `/Student` | Searchable, department-filtered, paginated student directory with role-appropriate create, edit, and delete actions. Students have a name, email, date of birth, and department. |
| Departments | `/Department` | Manages academic department records. A department is associated with its students. |
| Courses | `/Course` | Manages courses and their credit values. |
| Instructors | `/Instructor` | Manages instructor names and hire dates. |
| Enrollments | `/Enrollment` | Associates students with courses and stores an optional grade; supports search by student/course/grade, course filtering, and pagination. |
| Attendance | `/Attendance` | Records attendance and provides course/date filters, present/absent summary metrics, and paginated results. Instructors only see their assigned courses. |
| Office Assignments | `/OfficeAssignment` | Manages one office location per instructor. The list, details, create, edit, and delete workflows are implemented. |
| Staff accounts | `/Admin/Users` | Administrator-only staff account provisioning and instructor/course assignment management. |
| Sign in | `/Account/Login` | Staff sign-in. |
| Sign up | `/Account/Register` | Self-service account creation; new accounts start with read-only (`ReadOnlyStaff`) access. |

Most sections follow the standard MVC CRUD pattern: list records, view one record, create, edit, and delete. Create and edit forms post to MVC actions and use anti-forgery validation. Validation rules are defined on the model properties with data annotations.

The student directory supports server-side search by name/email, department filtering, and ten-record pages. Filters are retained when moving between pages. The attendance list supports server-side course/date filters, present/absent counts, a present-rate summary, and 25-record pages. Enrollments can be searched by student, course, or grade, filtered by course, and browsed in 15-record pages.

The dashboard is populated by `HomeController` using live database queries. It shows counts for students, instructors, courses, departments, and enrollments; five latest student records; five latest enrollments; and attendance counts for the current date. "Latest" is currently approximated by descending database ID because the entities do not store creation timestamps.

Instructor accounts are routed directly to their scoped attendance list when they visit the application home page; they are not shown the school-wide dashboard.

## User interface and design

### Shared layout

`Views/Shared/_Layout.cshtml` supplies the page shell:

- Responsive Bootstrap navigation for the main modules
- Shared content area and footer
- Shared success, error, and informational flash messages using `TempData`
- Font Awesome icons
- Bootstrap JavaScript and stylesheet

Bootstrap, Inter, and Font Awesome are self-hosted under `wwwroot/` (no CDN or network access needed). Dashboard-specific styles and responsive breakpoints are in `wwwroot/css/site.css`.

### Dashboard

`Views/Home/Index.cshtml` presents:

- A blue gradient welcome banner with "Add student" and "New course" shortcuts
- Four summary cards for students, instructors, courses, and departments
- A recent-students table with department, email, and a details link
- An attendance panel with today's record count and calculated present percentage
- Quick-access links for enrollments, office assignments, and attendance
- A recent-enrollments list with student, course, and grade
- Empty-state messages when the relevant list has no records

The layout adapts the stat cards and content columns to narrower screens. The attendance percentage is shown as 0 when there are no attendance records today.

### Other screens

The module pages are Razor views under `Views/<Module>/`. Index views generally render record tables and actions; create/edit views render forms; details and delete views show a selected record. Styling primarily uses Bootstrap classes. The shared validation partial is `Views/Shared/_ValidationScriptsPartial.cshtml`.

## Backend architecture

The ASP.NET application starts in `Program.cs`:

1. Registers MVC controllers with views.
2. Registers `ApplicationDbContext` with the SQL Server provider.
3. Registers module services with dependency injection.
4. Builds the application pipeline, enables static files and routing, adds request logging, and maps the default MVC route.
5. Creates a database scope and calls `DbInitializer.Initialize`, which applies migrations and conditionally inserts seed data.

The typical request flow is:

```text
Browser
  -> ASP.NET Core MVC route
  -> controller action
  -> service (for most modules)
  -> Entity Framework Core DbContext
  -> SQL Server LocalDB
  -> Razor view rendered as HTML
```

The dashboard directly queries `ApplicationDbContext`. Other controllers generally call their service interfaces. The student controller also reads department choices from the context for its forms. Attendance forms load student and course choices for their dropdowns.

### Routes and form handling

Controllers are in `Controllers/`. Most controllers have explicit module routes such as `/Student`, `/Course`, and `/Attendance`. The default route in `Program.cs` is `{controller=Home}/{action=Index}/{id?}`.

POST create/edit/delete actions use `[ValidateAntiForgeryToken]`. Model validation runs server-side through ASP.NET MVC model binding and the data annotations on the model classes. Services use asynchronous Entity Framework Core operations for database access.

## Sign-in and authorization

The MVC application uses ASP.NET Core Identity with SQL Server-backed accounts and role claims. A fallback authorization policy requires authentication for MVC endpoints unless an action explicitly allows anonymous access. The public surface is sign-in, self-service sign-up, and the access-denied page.

**2026 redesign:** the site uses an editorial academic theme — flat deep green (`#1E5B44`) and brass (`#9A6B2F`) on warm paper (`#F6F4EE`), Fraunces serif headings with Inter body text, a fixed sidebar shell (collapsing to an off-canvas menu on mobile), a sticky glass topbar, and a split-screen auth experience. On screens ≤640 px, data tables reflow into labelled record cards (via `data-label` cell attributes) instead of scrolling sideways, inputs use 16 px text to prevent iOS focus zoom, and buttons meet comfortable tap heights. `/Account/Register` lets a new user create an account; for safety it always starts with the least-privilege `ReadOnlyStaff` role and signs the user in immediately — an administrator can promote the account afterwards. Password rules are unchanged (12+ characters with upper/lower/digit/symbol).

| Role | Access |
|---|---|
| Administrator | Full record management, staff-account provisioning, and instructor-course assignment management. |
| AcademicAdministrator | View and manage student, department, course, instructor, enrollment, attendance, and office-assignment records. |
| Instructor | Attendance records only, limited to courses assigned by an administrator and students enrolled in those courses. |
| ReadOnlyStaff | Read-only access to school records and the dashboard. |

Identity requires a 12-character password with uppercase, lowercase, numeric, and non-alphanumeric characters. New accounts are locked for 15 minutes after five failed sign-in attempts. Cookies are HTTP-only, Secure, SameSite Lax, and have an eight-hour sliding expiration. POST endpoints use antiforgery validation.

The initial administrator is provisioned from environment configuration, not a source-controlled password:

```powershell
$env:BootstrapAdmin__Email = "admin@example.org"
$env:BootstrapAdmin__Password = "<set a unique strong password in your environment>"
dotnet run --launch-profile https
```

Do not commit real credentials or place them in tracked settings files. The first run creates the configured account and required roles. If neither bootstrap setting is present, role setup proceeds but startup logs a warning and no administrator account is created; if only one setting is provided, startup fails explicitly. Administrators can then create accounts at `/Admin/Users` and link each instructor login to one instructor profile. Instructor attendance access also requires explicit course assignments in the same admin section.

The Identity and instructor-course-assignment migration was generated and reviewed as additive. It adds Identity tables and the `InstructorCourseAssignments` table; it does not drop, rename, or rewrite the existing school tables. It was applied to the inspected LocalDB after review. The existing school records were preserved.

### Logging and errors

- `Middleware/RequestLoggingMiddleware.cs` logs request paths.
- `Filters/ActionLoggingFilter.cs` logs the MVC controller/action, HTTP method, and whether the request is authenticated. It deliberately does not log query strings, user names, or submitted action arguments.
- `Views/Shared/Error.cshtml` is the shared error page.
- `Views/Shared/NotFound.cshtml` provides a not-found page.
- The production pipeline configures an exception handler and HSTS. The launch profiles use the Development environment locally.

Review action-argument logging before using real student data: logged form arguments may contain personal information. Avoid enabling detailed development errors in a deployed environment.

## Database and data model

### Database

The connection string is configured in `appsettings.json` and currently targets SQL Server LocalDB with a database named `STDMSDB`. The application uses EF Core migrations and runs `Database.Migrate()` during startup. The migrations are in `Migrations/`.

`Data/ApplicationDbContext.cs` defines DbSets for the seven main school-record types, Identity users, and instructor-course assignments.

The current database was inspected read-only before the Identity work. It contained the seven expected school-record tables and five applied migration-history rows. The `InitialCreate`, `SeedData`, and `AddSeedAndFixes` migration `Up` methods are empty; the first migration creates the tables and the `sec` migration renames the original lowercase table names. After reviewing the additive SQL, startup applied `AddIdentityAndInstructorAssignments`; the school tables were not modified.

### Entities and fields

| Entity | Main fields | Meaning |
|---|---|---|
| `Student` | `Id`, `Name`, `Email`, `DateOfBirth`, `DepartmentId` | Student identity and department membership. Name is limited to 100 characters; email uses email validation. |
| `Department` | `Id`, `Name` | Academic department. Name is limited to 50 characters. Has a collection of students. |
| `Course` | `Id`, `Name`, `Credits` | Course name and credits. Name is limited to 100 characters; credits must be from 1 to 10. |
| `Instructor` | `Id`, `Name`, `HireDate` | Instructor identity and hire date. Name is limited to 100 characters. |
| `Enrollment` | `Id`, `StudentId`, `CourseId`, `Grade` | A student's enrollment in a course, with a grade limited to two characters. |
| `Attendance` | `Id`, `StudentId`, `CourseId`, `Date`, `IsPresent` | A student's attendance status in a course on a date. |
| `OfficeAssignment` | `InstructorId`, `Location` | One office location per instructor. Location is limited to 100 characters. `InstructorId` is the primary key and foreign key. |
| `ApplicationUser` | Identity account fields, optional `InstructorId` | Staff login and role account; instructor linkage is unique and nullable. |
| `InstructorCourseAssignment` | `InstructorId`, `CourseId` | Composite-key link that scopes instructor attendance access to assigned courses. |

### Relationships

- One department has many students. Deleting a department is currently configured to cascade-delete its students.
- Enrollments reference one student and one course; a student or course can have multiple enrollment rows.
- Attendance references one student and one course; records can be created for different dates.
- An office assignment has a one-to-one relationship with an instructor; the assignment uses `InstructorId` as its key.
- An instructor account may link to one instructor profile. Instructor-course assignments use a composite key and foreign keys to the instructor and course.

The model still does not define an academic term, class schedule, or attendance uniqueness rule.

## Seed data

`Seed/DbInitializer.cs` checks whether each table is empty and adds sample data for empty tables. It does not replace a table that already contains records.

| Type | Sample records |
|---|---|
| Departments | Computer Science; Business |
| Students | Ali Ahmed (`ali@example.com`, born 2000-05-20, Computer Science); Sara Mohamed (`sara@example.com`, born 1999-08-15, Business) |
| Courses | Database Systems (3 credits); Marketing 101 (2 credits) |
| Instructors | Dr. Amr Youssef (hired 2010-01-15); Dr. Mona Hassan (hired 2015-09-01) |
| Office assignments | Instructor 1: Building A - Room 101; Instructor 2: Building B - Room 202 |
| Enrollments | Student 1 in Course 1, grade A; Student 2 in Course 2, grade B |
| Attendance | Three historical records for the seeded students/courses; two present and one absent. Dates are relative to the day the initializer first inserts the records. |

These are demo records, not verified real-world school data. Since attendance rows are inserted only when the attendance table is empty, their relative dates are not refreshed on subsequent application starts.

### Bulk demo data (90+ students)

The local demo database was filled with a realistic volume using a one-off script (since removed; recoverable from Git history): 90 students (`student001@stdms.local` … `student090@stdms.local`), 6 extra departments, 18 instructors with office assignments, 18 courses, 2–3 enrollments per student, and six weekly attendance sessions per enrollment (~1,700 rows total).

## Technology stack

### Main website

- .NET 8 ASP.NET Core MVC
- Razor views and Tag Helpers
- ASP.NET Core Identity and Entity Framework Core 8.0.22
- SQL Server provider and SQL Server LocalDB
- Bootstrap 5.3.3 loaded from jsDelivr
- Font Awesome 6.4.0 and Google Fonts Inter loaded externally

The project file is `STDMS.csproj`. Nullable reference types are enabled.

### Separate Next.js placeholder

The repository also contains a Next.js 15.2.4 application, with React 19, in the root `app/` and `components/` directories. It has npm scripts for `dev`, `build`, and `start`. Its home page is a placeholder and does not use the MVC database or its controllers. `Views/app/` contains another copy of generated Next-related files and should not be confused with the MVC Razor `Views/` tree.

## Run the MVC application

Requirements:

- Windows with the .NET SDK compatible with the `net8.0` target
- SQL Server LocalDB installed and available

From the project directory:

```powershell
cd "C:\Users\Omar Fattouh\Desktop\STDMS"
dotnet restore STDMS.csproj
dotnet run --launch-profile http
```

Open `http://localhost:5265`. To use the HTTPS launch profile instead:

```powershell
dotnet run --launch-profile https
```

The configured HTTPS URL is `https://localhost:7176`. At startup, the application applies EF Core migrations and attempts to seed any empty tables.

The Next.js placeholder is a separate app and is not required to run the MVC website. Its npm package manifest is `package.json`; `npm run dev` uses the Next.js development server (normally port 3000).

## Project structure

```text
STDMS/
├── Contracts/       Service interfaces
├── Controllers/     ASP.NET MVC controllers
├── Data/            EF Core DbContext
├── Filters/         MVC filters, including action logging
├── Migrations/      EF Core database migrations and model snapshot
├── Middleware/      Request logging middleware
├── Models/          Database entities and dashboard view model
├── Properties/      Launch profiles
├── Seed/            Migration invocation, sample data, and Identity role/bootstrap setup
├── Services/        Data-access services
├── Views/           Razor views for MVC pages and shared layout
├── wwwroot/         Static CSS, JavaScript, libraries, and favicon
├── app/             Separate Next.js placeholder entry points
├── components/      Next.js UI components
├── Program.cs       ASP.NET dependency injection and request pipeline
├── STDMS.csproj     .NET project definition
├── appsettings.json SQL Server connection string and logging settings
└── package.json     Next.js dependency manifest and npm scripts
```

## Current limitations and recommended work

### Functional issue addressed during this improvement

- **Office Assignments index:** the original `/OfficeAssignment` route returned HTTP 500 because its index view was missing. A list view and instructor selector for creating assignments have now been added and verified with live records.

### Bugs fixed in the October 2026 pass

Found by smoke-testing every authenticated route and every create/edit form:

- **Add Student returned HTTP 500** (`GET /Student/Create`): the controller put a `List<Department>` in `ViewBag.Departments`, but the view binds `asp-items` to `IEnumerable<SelectListItem>`, throwing a `RuntimeBinderException`. The controller now supplies a `SelectList` via `PopulateDepartmentsAsync`, and both Student forms consume it the same way.
- **Enrollment dropdowns were empty** on Create and Edit: the controller set `ViewBag.StudentsList`/`CoursesList` while the views read `ViewBag.Students`/`Courses`. The names are now aligned, so students/courses can actually be selected.
- **Create forms silently refused to save** (Student, Enrollment, Attendance, Office Assignment, Department): non-nullable EF navigation properties (`Student.Department`, `Enrollment.Student`, …) were treated as implicitly required by MVC model validation, yet no form field supplies them — so every POST bounced back to the form with no visible error. Navigation properties are now marked `[ValidateNever]`; this does not change the EF model (`dotnet ef migrations has-pending-model-changes` still reports no changes).
- **Department pages had no browser title** (tab showed `- STDMS`): all five Department views now set `ViewData["Title"]`.
- **`/Home/Privacy` returned 404** although the view existed: a `Privacy()` action was added.
- **Every 404 returned an empty body**: `Views/Shared/NotFound.cshtml` existed but was never served. `UseStatusCodePagesWithReExecute("/Home/PageNotFound")` now renders it with a real 404 status.
- Success flash messages were added for Student and Enrollment create/edit/delete, matching Office Assignments.
- Build warnings dropped from 16 to 2 by initializing non-nullable model properties; the remaining two are `CS8981` in the generated `sec` migration, deliberately left untouched.

The smoke tests ran as throwaway Python scripts against the app on `http://localhost:5265` (credentials supplied via environment variables, creating and then deleting only temporary `Smoke…` records). The scripts were removed after the validation pass; they remain recoverable from Git history.

### Security and operations

- **Deployment account setup:** set `BootstrapAdmin:Email` and `BootstrapAdmin:Password` through the deployment environment. Without them the app intentionally creates no administrator; do not deploy without provisioning one.
- **Migration deployment:** the Identity migration has been applied to LocalDB and adds only auth and course-assignment tables. The app migrates automatically at startup; review migrations in a controlled environment before production rollout.
- **Account lifecycle:** administrators can create staff accounts and assign roles, but password reset, account disable, and role-change workflows are not yet implemented.
- **Security testing:** no automated authentication/authorization integration tests exist yet. Manually test each role, especially instructor course boundaries, before production use.
- **Sensitive data in logs:** the action filter no longer logs form values, query strings, or user names. The request middleware still logs URL paths; review logging policy and retention before production.
- **External static assets:** ~~Bootstrap, Font Awesome, and Google Fonts are served by external providers~~ — resolved: all CSS/JS/fonts (including jQuery validation) are self-hosted under `wwwroot/lib` and `wwwroot/fonts`, compressed, and cached; see the Lighthouse section.
- **Local-only database configuration:** the current connection string targets LocalDB. A deployment needs a managed SQL Server connection string and environment-specific secret/configuration handling.

### Data behavior and feature gaps

- **Department deletion:** cascade delete can remove all students assigned to that department. Confirm this behavior is intended and warn users before deleting a populated department.
- **"Recent" ordering:** students and enrollments have no creation timestamp; the dashboard sorts by ID, which may not reliably represent creation time after imports or manual database changes.
- **Attendance metric:** the dashboard shows the proportion present only among today's attendance records. It displays zero when no records exist; it does not indicate whether attendance has not yet been taken or there are no scheduled classes.
- **No attendance uniqueness constraint:** duplicate attendance records for the same student, course, and date are not prevented by the current model.
- **No term or timetable model:** course enrollments have no semester/year or schedule information.
- **Demo seeds:** sample data is hard-coded, and the initializer uses table emptiness checks. Production data setup should be deliberate and should not depend on demo IDs.
- **No tests found:** no C# test files were found in the project during inspection.
- **Build warnings:** the project builds with zero errors, but existing migration naming and model nullability warnings remain.

## Performance, accessibility & SEO (Lighthouse)

All CSS/JS/fonts are now **self-hosted** (`wwwroot/lib`, `wwwroot/fonts`) — the app no longer depends on CDNs, so it also works offline. Responses are Brotli/Gzip-compressed with long-lived caching for library/font assets, scripts are deferred, and the FontAwesome solid font was subset from 150 KB to ~3 KB covering only the icons the views use (the full `.ttf` is kept alongside it).

Measured with Lighthouse (Edge/Chromium, mobile emulation) after the optimization pass:

| Page | Performance | Accessibility | Best Practices | SEO |
|---|---|---|---|---|
| `/Account/Login` | 100 | 100 | 100 | 100 |
| `/Account/Register` | 100 | 100 | 100 | 100 |
| `/` (Dashboard) | 100 | 100 | 100 | 100 |
| `/Student` | 100 | 100 | 100 | 100 |
| `/Attendance` | 100 | 100 | 100 | 100 |

A full sweep of every page type (lists, create, edit, details, and delete pages for all seven entities, admin pages, and the student directory with 92 records) scores 100/100/100/100; the Next.js placeholder page scores 99/100/100/100 (the single performance point is React runtime main-thread time under throttled mobile emulation).

Dashboard also scores 100/100/100/100 with the desktop preset. Responsive behaviour was audited at 375 px, 768 px, 1024 px, and 1440 px with no page-level horizontal overflow; tables scroll inside their cards on small screens. Re-run via `npx lighthouse <url> --chrome-path=<Edge/Chrome>` (for authenticated pages, pass a session cookie with `--extra-headers`).

## Validation performed

- October 2026 pass: built with 0 errors (warnings down from 16 to the 2 pre-existing `CS8981` warnings in the generated `sec` migration). A route smoke test passed all 42 authenticated routes (previously: Student/Create 500, Enrollment Create/Edit empty dropdowns, Privacy 404, empty-body 404s), and create/search/delete round-trips passed for Students, Enrollments, Departments, Courses, Instructors, Office Assignments, and Attendance, cleaning up after themselves. Verified in the browser: login, dashboard, and the previously broken Student/Enrollment create forms.
- Built `STDMS.csproj` after the Identity, authorization, attendance-scope, and account-management changes: succeeded with zero errors and the existing compiler warnings.
- Built the latest searchable student directory changes: succeeded with zero warnings and zero errors.
- Built attendance filters, summaries, pagination, and corrected create/edit dropdowns: succeeded with zero warnings and zero errors.
- Built the enrollment search/course filter and pagination changes: succeeded with zero warnings and zero errors.
- `dotnet ef migrations has-pending-model-changes` reports no un-migrated model changes. `dotnet ef migrations list` confirms `AddIdentityAndInstructorAssignments` is applied on LocalDB.
- Generated and inspected the SQL for the pending migration. It creates Identity/assignment tables and indexes, with no drops or modifications to the existing school tables.
- Read-only inspection before the migration work found 2 departments, 2 students, 2 courses, 2 instructors, 2 office assignments, 2 enrollments, and 3 attendance records. The migration adds only new Identity/assignment structures; existing school tables and rows were not rewritten.
- Started the HTTPS app at `https://localhost:7176`; `/Account/Login` returned HTTP 200, while `/` and `/Admin/Users` redirected unauthenticated requests to sign-in.
- Verified the student list and filtered search URLs redirect unauthenticated requests to sign-in. Search/filter result behavior is server-side EF Core and was compile-checked; an authenticated browser test remains pending.
- Verified `/Attendance` and a course/date-filtered attendance URL both redirect unauthenticated requests to sign-in. Attendance work adds no database schema changes.
- Verified `/Enrollment` and a search/course-filtered enrollment URL both redirect unauthenticated requests to sign-in. Enrollment work adds no database schema changes.
- Startup seeded the four configured roles but logged that no bootstrap administrator was configured. Set the two bootstrap environment variables before attempting sign-in; role-specific authenticated workflows still need integration testing.
- Verified the Office Assignments list rendered sample instructor/location records and the create form rendered an instructor dropdown.
- Inspected the rendered dashboard: the counts and recent student/enrollment records came from the SQL Server database.
