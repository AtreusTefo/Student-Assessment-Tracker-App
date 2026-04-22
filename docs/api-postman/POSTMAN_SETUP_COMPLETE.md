# Postman Testing Setup

## Status: Ready for Testing

The Postman collection covers the full multi-role API. Three JWT roles are supported - Admin, Teacher, and Student - each with dedicated login and protected endpoints.

---

## What Is Included

### Collection (StudentAssessmentTracker.postman_collection.json)
- All Admin endpoints (login, create/update/delete teachers and students, bulk import, audit log)
- All Teacher endpoints (activate, login, forgot-password, profile management)
- All Student endpoints (activate, login, forgot-password, profile)
- Assessment endpoints (CRUD and bulk create)
- Report endpoints (CSV and PDF export)
- Assessment Submission endpoints (upload, download, delete)
- Grade and Subject lookup endpoints
- Class Group endpoints

### Environment (StudentAssessmentTracker.postman_environment.json)

| Variable | Value | Set by |
|---|---|---|
| base_url | http://localhost:5000 | Pre-configured |
| adminToken | (empty) | Auto-saved on admin login |
| teacherToken | (empty) | Auto-saved on teacher login |
| studentToken | (empty) | Auto-saved on student login |

---

## Seed Admin Account

The database is seeded with a default admin on first startup:

| Field | Value |
|---|---|
| Email | admin@tracker.local |
| Password | Admin@1234 |

---

## Database

- Engine: SQL Server LocalDB
- Database name: StudentAssessmentTrackerDev
- Connection: (localdb)\mssqllocaldb
- Migrations: Applied automatically on startup (19 migrations as of April 22, 2026)

---

## Swagger UI Alternative

If you prefer not to use Postman, Swagger UI provides full interactive documentation:

1. Start the API with dotnet run.
2. Navigate to http://localhost:5000/swagger.
3. Run a login endpoint to get a token.
4. Click Authorize (top right), enter Bearer token.
5. Test any endpoint directly from the browser.

---
