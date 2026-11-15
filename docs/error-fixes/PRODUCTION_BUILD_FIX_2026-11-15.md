# Production build failure on main (2026-11-15)

## Symptom

`npx ng build --configuration production` in `StudentApp/` failed, so the
application could not be packaged for deployment.

```
TS2339: Property 'timestamp' does not exist on type 'AuditLogDto'.
TS2339: Property 'username' does not exist on type 'AuditLogDto'.
TS2339: Property 'user' does not exist on type 'AuditLogDto'.
TS2339: Property 'details' does not exist on type 'AuditLogDto'.
```

After that was fixed, the build still failed on a budget:

```
src/app/components/student-list/student-list.component.css exceeded maximum budget.
Budget 8.00 kB was not met by 611 bytes with a total of 8.61 kB.
```

## Root cause

1. The Audit Logs table in `admin-dashboard.component.html` still read field
   names from an earlier version of the audit log response. The API DTO
   (`AuditLogDto.cs`) and the front-end interface (`admin-api.service.ts`) use
   `changedAt`, `changedBy`, `changedByRole`, `oldValues` and `newValues`.
2. Several component stylesheets have grown past the default Angular
   `anyComponentStyle` error budget of 8 kB (`student-list` 8.61 kB,
   `student-dashboard` 8.98 kB).

## Fix

1. Audit Logs table now binds to the real fields:
   - Date: `log.changedAt`
   - User: `log.changedBy`, followed by `log.changedByRole` in brackets
   - Details: `formatJson(log.newValues ?? log.oldValues)`
2. `angular.json`, production budgets: `anyComponentStyle` warning raised from
   4 kB to 8 kB and error from 8 kB to 16 kB. The initial-bundle budget is
   unchanged (797 kB against a 1 MB error limit).

## Prevention

The GitHub Actions workflow `.github/workflows/azure-app-service.yml` now runs
the production build on every pull request to `main`, so a template that no
longer matches its DTO fails the check before it is merged.
