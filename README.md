# SRC-51 Compression Force Control System

ASP.NET Core 8 MVC web HMI for a tablet-press compression force controller: recipes, batches, calibration, alarms, audit trail, reports and user/privilege management. Data is stored in PostgreSQL.

> **Status:** there is no PLC integration yet. Diagnostics, Auto/Operation/Manual Mode and Auto Tare show static or empty data, and the PLC status indicator reads "disconnected".

## Solution layout

| Project | Purpose |
|---|---|
| `CompressionForce.Web` | MVC app: controllers, Razor views, authorization, static files |
| `CompressionForce.Services` | Application services, DTOs, recipe validators, audit |
| `CompressionForce.Data` | EF Core `ApplicationDbContext`, entities, migrations |
| `CompressionForce.Domain` | Domain entities |
| `CompressionForce.Integrations` | PLC integration (placeholder) |

## Requirements

- .NET 8 SDK
- PostgreSQL (database name used in development: `51_STATION`)
- Windows for PDF export (Rotativa bundles `wkhtmltopdf` executables in `wwwroot/Rotativa`)

## Getting started

1. **Create the database** (empty, e.g. `51_STATION`).

2. **Set secrets.** Nothing sensitive is stored in the repo; use user-secrets (Development) or environment variables (production):

   ```bash
   cd CompressionForce.Web
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=51_STATION;Username=postgres;Password=<your password>"
   dotnet user-secrets set "DefaultAdmin:Password" "<initial admin password>"
   ```

   If `DefaultAdmin:Password` is not set, a random password is generated on first start and written once to the startup log.

3. **Apply migrations:**

   ```bash
   dotnet ef database update --project CompressionForce.Data --startup-project CompressionForce.Web
   ```

4. **Run:**

   ```bash
   dotnet run --project CompressionForce.Web --launch-profile http
   ```

   The app listens on http://localhost:5174. User-secrets are only loaded when `ASPNETCORE_ENVIRONMENT=Development`, which the launch profile sets.

## First login

On first start the app seeds the `Admin` and `User` groups and a default admin account (`DefaultAdmin:UserName` / `Email`, default `admin` / `admin@localhost`). That account must change its password at first login.

## Security model

- **Sessions:** session-based login with BCrypt password hashes; every route except login/register requires a session.
- **Password rule:** 8-15 characters with upper case, lower case, digit and special character (sign-up, change, admin-created and admin-reset passwords).
- **Sign-up:** self-registered accounts are inactive until an administrator approves them.
- **Privileges:** each page and API is guarded by `[RequirePrivilege]`. Members of the `Admin` group implicitly hold every privilege; other groups get privileges via Access Management.
- **Security settings** (Access Management): idle timeout, password expiry, max wrong attempts and lockout duration.
- **Audit trail:** logins, user changes, recipe and batch changes are recorded and can be exported to PDF.

## Main pages

Recipes (add / edit / print / remove), Batch (add / deactivate), Report (per-batch results), Calibration and Servo Calibration, Alarms (filter + PDF), Auto Tare, Diagnostics, Audit Trail, Access Management, Help.

## Known limitations

- No PLC communication (see status above).
- Batch Edit / View / Print buttons are disabled.
- Calibrate stores a new row on every press.
- Leftover scaffolding: empty `Class1.cs` files, `Login2.cshtml`, `Privacy.cshtml`, unused `Table`/`Graph`/`ViewBatch` views.
