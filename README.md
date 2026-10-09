# SchoolDesk

School management portfolio application built with **ASP.NET MVC 5**, **C#**, **.NET Framework 4.8** and **SQL Server**. This is the classic System.Web MVC framework, not ASP.NET Core.

## Requirements

- Windows 10 or Windows 11.
- .NET Framework 4.8 runtime (normally included on Windows 11).
- A supported .NET SDK, version 8 or newer, for command-line restore/build.
- IIS Express and SQL Server Express LocalDB. Install them through Visual Studio Installer: **ASP.NET and web development** workload, then ensure **IIS Express** and **SQL Server Express LocalDB** are selected under Individual components.
- Internet access for the first NuGet restore. The interface uses local CSS/JavaScript and no CDN.

Visual Studio is useful for editing and debugging. VS Code also works as an editor. This SDK-style classic MVC library is launched using the included IIS Express script; do not use `dotnet run`, and do not expect the standard web-project F5 launch profile.

## First run

Extract the archive to a normal writable folder such as `C:\Projects\SchoolDesk`. Open PowerShell in that folder (the folder containing this README and `scripts`). Then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Setup.ps1 -Demo
powershell -ExecutionPolicy Bypass -File .\scripts\Run.ps1
```

The execution-policy override applies only to those PowerShell processes. Setup starts LocalDB, creates the dedicated `SchoolDeskDemo` database, applies versioned SQL migrations, creates fictional records, generates unique passwords and a local JWT signing key, restores NuGet packages and builds the app.

Open **http://localhost:5080**. Keep the Run terminal open. Stop with Ctrl+C.

Find the generated demo login details in:

```text
SchoolDesk\App_Data\demo-credentials.txt
```

There are accounts for Admin, two Teachers, Accountant, Parent and Student. The local credential file and JWT secret are ignored by Git. Setup does not reset existing accounts or overwrite existing database records. Subsequent launches need only `Run.ps1`; after code changes rerun `Setup.ps1` to rebuild.

To use a different port:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Run.ps1 -Port 5081
```

## Included features

- Cookie authentication; PBKDF2-SHA256 password hashing; account lockout; per-IP login throttling; password change; sign-out with session/token invalidation.
- Five roles with server-side checks. Teacher access is scoped to assigned classes; parents and students see only linked student records.
- Admissions: add/edit students, unique admission numbers, guardian details, optional Parent/Student account links, archive without deleting history, search and pagination.
- Classes/sections: create classes, assign/reassign teachers, view enrolment counts.
- Attendance: class/date selection, four statuses, transactional updates, attendance history.
- Fees: create invoices, record partial payments, reject overpayment, unique payment references, balances, printable invoices and payment history.
- Exams: class assessments, subject field, maximum marks, transactional result entry, scored results reports.
- Users: create accounts with selected roles, enable/disable access. Teachers are accounts assigned to classes; there is no separate HR/payroll module.
- Dashboard summaries, printable reports, administrative audit log, responsive interface with locally bundled CSS and JavaScript.
- JWT API: token issuance, scoped paginated student listing, token revocation; issuer/audience/signature/algorithm/expiry checks.

## Database and migrations

The database is reproducible from `database/migrations` and `scripts/Seed-Demo.ps1`; no SQL Server binary database file is included. Migration checksums prevent silently changing an already-applied migration. Add a new numbered migration for a schema change. The setup script uses transactions and does not drop tables or reset a database.

Setup and demo seeding deliberately target the isolated **SchoolDeskDemo** LocalDB database. Hosting requires a separately configured SQL Server database and a reviewed deployment process. The included setup script is for local Windows development.

## API

See [docs/API.md](docs/API.md) and the [OpenAPI specification](docs/openapi.json). The API is a small school API within this MVC app; the independent Driving Licence Services API is a separate future project.

## Verify locally

With the app running and demo data seeded:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Test-Local.ps1
```

The smoke tests use fictional accounts and create no business records. They check login, role-specific student scopes, a teacher accessing another class, a parent opening another student's invoice, JWT rejection and revocation. Sign-out revokes the test user's other existing sessions/tokens. See `docs/VALIDATION.md` for the exact checks executed before packaging and the required remaining Windows checks.

## GitHub

Before the first commit, verify that `SchoolDesk/App_Data/local.settings.config`, `demo-credentials.txt`, `bin`, `obj`, `.mdf`, `.ldf` and `.bak` files are absent from the staged files. Keep the source, migrations, seed script, README and documentation. Do not upload local or production secrets.

Add screenshots from the running app and the public demo URL to this README after verifying them. Describe this as a personal portfolio project; company experience remains separate.

## Scope

This release covers the workflows listed above. It does not include online payments, email delivery/password recovery, file uploads, a timetable, payroll, multi-school tenancy, JWT refresh tokens or a separate subject catalogue. Subjects are entered on assessments. There are no claims of a completed security audit or a production deployment.

## Troubleshooting

- `dotnet is not recognized`: install a supported .NET SDK and open a new PowerShell window.
- `SqlLocalDB is not recognized`: install SQL Server Express LocalDB, then restart PowerShell.
- Missing IIS Express: install the Visual Studio web workload or IIS Express separately from Microsoft.
- NuGet restore errors: check Internet/DNS access and retry setup.
- Port in use: pass another port to Run.ps1.
- Login fails: use the exact email and password from the private credential file; after 5 wrong attempts wait 15 minutes.
- HTTP 500: keep it local, copy the error text without secret config contents, and inspect setup/build output. Do not set remote production debug mode to true.

MIT licensed source; third-party dependencies retain their own licences. See THIRD_PARTY_NOTICES.md.
