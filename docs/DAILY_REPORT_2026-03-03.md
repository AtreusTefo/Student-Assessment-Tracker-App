# Daily Development Report
**Date:** March 3, 2026  
**Project:** Student Assessment Tracker  
**Developer:** Development Team  

---

## 📋 What I Did Today

### 1. Postman Collection Debugging (3 Issues)
Investigated and resolved three separate bugs that prevented Postman from working
with the API, covering collection import failures, hanging requests, and misleading
response output.

### 2. Angular Frontend Proxy Fix
Diagnosed and fixed a proxy misconfiguration that caused the Angular frontend to
return HTML instead of JSON for all API calls, crashing the UI with a parse error.

### 3. Database Migration — In-Memory → SQL Server LocalDB
Planned and executed a full database provider swap from EF Core In-Memory
(volatile, lost data on restart) to SQL Server LocalDB for development and SQL
Server for production. Included package updates, new connection strings, EF Core
migrations, and auto-migration on startup.

### 4. Port Conflict Resolution
Resolved a startup failure caused by a stale `dotnet` process still holding port
5000 after the previous session.

---

## ✅ What Was Completed

### Postman
- ✅ Fixed empty collection import (invalid `_postman_id` UUID)
- ✅ Fixed hanging "Sending request..." (localhost IPv6 vs IPv4 mismatch)
- ✅ Fixed Delete endpoint appearing to return no body (HTTP 204 → 200 OK)

### Angular Frontend
- ✅ Corrected `proxy.conf.json` target to `http://127.0.0.1:5000`
- ✅ Added missing `proxyConfig` entry to `angular.json` (proxy was never loaded)
- ✅ Verified: `http://localhost:4200/api/students` now correctly proxies to API

### Database
- ✅ Replaced `Microsoft.EntityFrameworkCore.InMemory` with `SqlServer` + `Design` + `Tools` packages
- ✅ Created `appsettings.Development.json` with LocalDB connection string
- ✅ Updated `ApplicationDbContext.cs` with SQL Server column types (`decimal(5,2)`, unique indexes, `GETUTCDATE()` defaults)
- ✅ Updated `Program.cs`: `UseSqlServer()` with `EnableRetryOnFailure(5, 10s)`, auto-migrate on startup
- ✅ Ran `dotnet ef migrations add InitialCreate` — migration files created
- ✅ Ran `dotnet ef database update` — `StudentAssessmentTrackerDev` database created in LocalDB
- ✅ Verified data persistence: created record survived API restart

### Misc
- ✅ Resolved port 5000 already-in-use error
- ✅ API confirmed running on `http://127.0.0.1:5000` (IPv4 only)

---

## 🚧 Challenges Faced

### 1. **Postman: Empty Collection After Import**
**Problem:** The Postman collection imported but showed "This collection is empty" — no
requests were visible.

**Root Cause:** `_postman_id` in the JSON file was `"student-assessment-api"` — a
human-readable slug, not a valid UUID. Postman silently discards items in collections
with non-UUID IDs during import.

**Solution:** Changed `_postman_id` to a proper UUID format:
`"a3f2c1d4-b5e6-4789-abcd-ef1234567890"`

**Impact:** High — blocked all Postman testing.

---

### 2. **Postman: Requests Hang at "Sending request..."**
**Problem:** Every request in Postman spun indefinitely on "Sending request..." and
never returned a response or error.

**Root Cause (layered):**
1. The collection's URL `host` field was `["{{base_url}}"]` — using a Postman
   environment variable as the hostname. When no environment is selected,
   `{{base_url}}` is unresolved, causing a silent hang.
2. Even after switching to `localhost`, Windows 11 resolves `localhost` to IPv6
   `::1` first. The API was only bound to `127.0.0.1` (IPv4), so the connection was
   refused, but Postman timed out silently rather than showing an error.

**Solution:**
- Replaced all variable-based URLs with hardcoded `http://127.0.0.1:5000` using the
  proper Postman URL structure (`protocol`, `host: ["127","0","0","1"]`, `port`, `path`).
- Updated `launchSettings.json` to explicitly bind to `http://127.0.0.1:5000` only,
  removing any ambiguity about IPv4 vs IPv6.

**Impact:** High — blocked all Postman testing.

---

### 3. **Postman: DELETE Returns "No Body" / "204 No Content"**
**Problem:** The DELETE `/api/students/{id}` endpoint returned a 204 status with no
response body in Postman, which confused testing (no confirmation the right record
was deleted).

**Root Cause:** The controller called `return NoContent()`, which correctly produces
HTTP 204. HTTP 204 is defined as having no message body by the spec — Postman was
behaving correctly.

**Solution:** Changed to `return Ok(new { message = $"Student with ID {id} successfully deleted" })`
and updated `[ProducesResponseType]` from `Status204NoContent` to `Status200OK`.

**Files Changed:**
- `StudentAssessmentTrackerAPI/Presentation/Controllers/StudentsController.cs`

**Impact:** Low — cosmetic for testing, but improves API consumer experience.

---

### 4. **Angular Frontend: "Unexpected token '<', <!doctype... is not valid JSON"**
**Problem:** Every page in the Angular app immediately crashed with a JSON parse
error. The browser console showed the full HTML of the Angular index page being
returned instead of JSON from the API.

**Root Cause (two bugs):**
1. `proxy.conf.json` targeted `https://localhost:5001` — a non-existent address
   (the API was on `http://127.0.0.1:5000`, and HTTPS was never configured). All
   `/api` requests silently failed at the proxy layer.
2. `angular.json` had **no `proxyConfig` entry** in the `serve > configurations >
   development` section — meaning Angular's dev server **never loaded the proxy
   file at all**. The proxy had been non-functional since the project was created.

**Consequence:** Because no proxy was active, all `/api/...` fetch calls from the
Angular app hit `http://localhost:4200/api/...`, which the Angular dev server
answered with its own `index.html` fallback. The HTTP service tried to parse that
HTML as JSON and crashed.

**Solution:**
```json
// proxy.conf.json — corrected target
{
  "/api": {
    "target": "http://127.0.0.1:5000",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```
```json
// angular.json — added missing proxyConfig
"serve": {
  "configurations": {
    "development": {
      "proxyConfig": "proxy.conf.json"   ← THIS WAS MISSING
    }
  }
}
```

**Files Changed:**
- `StudentApp/proxy.conf.json`
- `StudentApp/angular.json`

**Impact:** Critical — the Angular frontend had never successfully communicated with
the backend since the project was created.

---

### 5. **In-Memory Database: Data Lost on Every Restart**
**Problem:** All student/teacher records vanished every time the API was restarted
because EF Core's In-Memory provider stores data only in RAM.

**Root Cause:** `UseInMemoryDatabase("StudentDb")` was used for convenience during
early development but is not suitable for a working application.

**Solution:** Full migration to SQL Server LocalDB for development:

| Step | Change |
|------|--------|
| `.csproj` | Replaced `InMemory 8.0.0` with `SqlServer 8.0.0` + `Design 8.0.0` + `Tools 8.0.0` |
| `appsettings.Development.json` | New file — LocalDB connection string |
| `appsettings.json` | Added `ConnectionStrings:DefaultConnection` base entry |
| `ApplicationDbContext.cs` | SQL Server column types: `decimal(5,2)`, unique email indexes, `GETUTCDATE()` defaults |
| `Program.cs` | `UseSqlServer()` with `EnableRetryOnFailure(5 retries, 10s)`, `CommandTimeout(60)`, auto-migrate block on startup |
| `launchSettings.json` | Added `ASPNETCORE_ENVIRONMENT=Development` to profile |

**EF Core Commands Run:**
```powershell
# Install EF Core CLI (global)
dotnet tool install --global dotnet-ef --version 8.0.0

# Create migration
dotnet ef migrations add InitialCreate --output-dir Infrastructure/Data/Migrations

# Apply to database
dotnet ef database update
```

**Database Created:** `StudentAssessmentTrackerDev` on `(localdb)\mssqllocaldb`  
**Tables Created:** `Students`, `Teachers`, `__EFMigrationsHistory`

**Persistence Verified:** Created Alice Smith (ID: 1), restarted API → Alice still
present ✅

**Impact:** Critical — this was a fundamental production-readiness issue.

---

### 6. **Port 5000 Already In Use**
**Problem:** Running `dotnet run` failed immediately with:
```
System.IO.IOException: Failed to bind to address http://127.0.0.1:5000: address already in use
```

**Root Cause:** A previous `dotnet run` process from an earlier session was still
running in a background terminal and had not been stopped.

**Solution:**
```powershell
# Find which process holds port 5000
netstat -ano | findstr :5000

# Kill all dotnet processes (quick approach)
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force

# Wait 2 seconds for OS TIME_WAIT to clear, then run
Start-Sleep -Seconds 2
dotnet run
```

**Impact:** Medium — blocked development startup until resolved.

---

## 📊 Daily Stats

| Metric | Count |
|---|---|
| Issues Resolved | 6 |
| Files Modified | 10 |
| New Files Created | 4 (appsettings.Development.json, appsettings.Production.json, launchSettings.json, migration files) |
| Database Provider Changed | In-Memory → SQL Server LocalDB |
| Build Errors | 0 |
| Runtime Issues After Fixes | 0 |

---

## 📁 Files Changed Today

| File | Change |
|---|---|
| `docs/StudentAssessmentTracker.postman_collection.json` | Fixed UUID, localhost→127.0.0.1, string URLs |
| `StudentAssessmentTrackerAPI/Presentation/Controllers/StudentsController.cs` | Delete: NoContent→Ok with message |
| `StudentApp/proxy.conf.json` | Corrected target to `http://127.0.0.1:5000` |
| `StudentApp/angular.json` | Added `proxyConfig: "proxy.conf.json"` to serve.development |
| `StudentAssessmentTrackerAPI/StudentAssessmentTracker.csproj` | Replaced InMemory with SqlServer packages |
| `StudentAssessmentTrackerAPI/appsettings.json` | Added ConnectionStrings section |
| `StudentAssessmentTrackerAPI/appsettings.Development.json` | Created — LocalDB connection string |
| `StudentAssessmentTrackerAPI/appsettings.Production.json` | Created — prod connection string placeholder |
| `StudentAssessmentTrackerAPI/Infrastructure/Data/ApplicationDbContext.cs` | SQL Server column types, indexes, defaults |
| `StudentAssessmentTrackerAPI/Program.cs` | UseSqlServer, retry policy, auto-migrate block |
| `StudentAssessmentTrackerAPI/Properties/launchSettings.json` | Development + Production profiles, IPv4 binding |
| `StudentAssessmentTrackerAPI/Infrastructure/Data/Migrations/` | InitialCreate migration files |

---

## 🔄 Current State After Today

```
StudentAssessmentTracker/
├── StudentAssessmentTrackerAPI/
│   ├── Properties/launchSettings.json    ← IPv4 binding, env profiles
│   ├── appsettings.json                  ← ConnectionStrings base
│   ├── appsettings.Development.json      ← LocalDB connection string
│   ├── appsettings.Production.json       ← Prod SQL Server placeholder
│   ├── Infrastructure/Data/
│   │   ├── ApplicationDbContext.cs       ← SQL Server types + indexes
│   │   └── Migrations/                   ← InitialCreate (applied ✅)
│   └── Program.cs                        ← UseSqlServer + auto-migrate
└── StudentApp/
    ├── proxy.conf.json                   ← Target: http://127.0.0.1:5000
    └── angular.json                      ← proxyConfig now active
```

**API:** Running on `http://127.0.0.1:5000` (IPv4 only)  
**Frontend:** `http://localhost:4200` — proxy active, API calls succeed  
**Database:** SQL Server LocalDB `StudentAssessmentTrackerDev` — data persists  
**Postman:** Collection working at `http://127.0.0.1:5000`, all 5 endpoints tested  

---

## 🔄 Next Steps (Recommendations)

1. **Git commit** all today's changes with a descriptive message
2. **Push to GitHub** to backup the SQL Server migration files
3. **Set up `appsettings.Production.json`** with a real SQL Server connection string before deploying
4. **Add seed data** for initial teachers/students in the migration
5. **Configure JWT authentication** to replace the demo token in TeachersController
6. **Set up production profile** for Angular build (`ng build --configuration production`)

---

**Report Generated:** March 3, 2026  
**Total Development Time:** ~4 hours  
**Overall Status:** ✅ All 6 issues resolved — API, frontend, database, and Postman all fully functional
