# CompressionForce – Playwright end-to-end tests

One spec per screen of **CompressionForce.Web** (ASP.NET Core MVC, SRC-51 Compression Force Control System).
The tests drive the real UI in Chromium and are written against the markup in `CompressionForce.Web/Views`.

## Setup

```bash
cd e2e
npm install
npx playwright install chromium
cp .env.example .env      # then edit .env
```

`.env`:

| Variable | Meaning |
|---|---|
| `BASE_URL` | Where the app runs, default `https://localhost:7111` (`launchSettings.json`, profile `https`) |
| `E2E_USER` / `E2E_PASSWORD` | An **active** account that has already changed its first-login password. Use an Admin-group account to cover every screen |

> The seeded default admin is forced to change its password at first login (`Program.cs`), so log in once by hand first.
> Use a test database: the suite never saves/deletes recipes, batches, users or settings, but it does log in (audit entries) and loads data.

## Run

```bash
# 1) start the app (needs PostgreSQL per appsettings.json)
dotnet run --project CompressionForce.Web --launch-profile https

# 2) in another terminal
cd e2e
npm test              # everything
npm run test:anon     # pages reachable without login
npm run test:auth     # pages behind the login guard
npm run test:ui       # Playwright UI mode
npm run report        # open the HTML report
```

## Layout

| Project | Files | Notes |
|---|---|---|
| `setup` | `auth.setup.ts` | logs in once, saves `.auth/user.json` |
| `anonymous` | `*.anon.spec.ts` | Login, Register, Error page, login guard |
| `authenticated` | `*.spec.ts` | all other screens, reuse the saved session |

| Spec | Screens covered |
|---|---|
| `login.anon.spec.ts` | `/Account/Login`, login guard / 401 |
| `register.anon.spec.ts` | `/Account/Register` (validation only, never creates an account) |
| `error-page.anon.spec.ts` | `/Home/Error?code=…` |
| `layout-and-home.spec.ts` | `/Home/Welcome`, top bar, sidebar (all 12 links), user menu, PLC status card |
| `sign-out.spec.ts` | Sign Out (own login, so the shared session survives) |
| `help-and-access-denied.spec.ts` | `/Home/HelpMenu`, `/Home/AccessDenied` |
| `change-password.spec.ts` | `/Account/ChangePassword` (validation + wrong old password only) |
| `diagnostics.spec.ts` | `/Home/Diagnostics` |
| `signal-and-manual-mode.spec.ts` | `/Home/Signal`, `/Home/ManualMode` |
| `recipe.spec.ts` | `/Recipe/RecipeParameter`, `AddRecipe`, `EditRecipe`, `RemoveRecipe` (stops before Save/Delete) |
| `batch.spec.ts` | `/Home/Batch` (dialogs opened and cancelled) |
| `auto-and-operation-mode.spec.ts` | `/Home/AutoMode`, `/Home/OperationMode` |
| `report-table-graph.spec.ts` | `/Report/Report`, `/Home/Table`, `/Home/Graph` |
| `alarm-autotare-calibration.spec.ts` | `/Home/Alarm`, `/Home/AutoTare`, `/Home/Calibration` |
| `audit-trail-and-access-management.spec.ts` | `/AuditTrail/AuditTrail`, `/Home/AccessManagement` |

## Behaviour worth knowing

* If the test account's group lacks a screen's privilege the app redirects to `/Home/AccessDenied`;
  those tests are **skipped** (not failed) with the reason.
* Print/PDF buttons (Report, Audit Trail, Alarm) are not clicked: Rotativa uses the bundled Windows `wkhtmltopdf.exe`.
* Wrong-password tests on the login page use a random non-existent user name so a real account is never locked out.
* Tests that need data are **skipped** on an empty database: Add Batch dialog and Remove Recipe (need a recipe),
  Report view tabs (need a batch with force data). Create a recipe and a batch to cover them.
* Login redirects to `/` (default route `Home/Welcome`), so the home URL is matched as `/` or `/Home/Welcome`.
* `Home/Table` and `Home/Graph` redirect to the Report page (by design); that is what is tested.
* The sidebar "Sign Out" button only exists below 861px wide (the user menu covers desktop widths).
* Screens that depend on PLC data assert structure and "no script errors", not machine values.
