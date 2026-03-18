# Database Backup & Rollback Guide

**Project:** Student Assessment Tracker  
**Database:** `StudentAssessmentTrackerDev` on `(localdb)\mssqllocaldb`  
**Last Updated:** March 18, 2026

---

## Table of Contents

1. [Overview](#overview)
2. [Current Migration History](#current-migration-history)
3. [Option A — EF Core Migration Rollback (Schema Changes)](#option-a--ef-core-migration-rollback-schema-changes)
4. [Option B — Full Database Backup & Restore (Schema + Data)](#option-b--full-database-backup--restore-schema--data)
5. [Option C — Nuclear Reset (Drop & Recreate)](#option-c--nuclear-reset-drop--recreate)
6. [Quick Decision Guide](#quick-decision-guide)
7. [Common Scenarios & Solutions](#common-scenarios--solutions)
8. [Prerequisites](#prerequisites)

---

## Overview

The project uses **SQL Server LocalDB** (`(localdb)\mssqllocaldb`) with **Entity Framework Core 8** migrations for schema management. There are two layers of protection available:

| Layer | Protects | Tool |
|---|---|---|
| EF Core Migrations | Database schema (tables, columns, indexes) | `dotnet ef` CLI |
| SQL Server Backup/Restore | Schema **and** data | SSMS or T-SQL |

> **Best Practice:** Always take a full `.bak` backup before any risky operation (adding/removing columns, bulk data changes, testing destructive queries).

---

## Current Migration History

These are all applied migrations in chronological order:

| # | Migration Name | Date Applied | Description |
|---|---|---|---|
| 1 | `InitialCreate` | 2026-03-03 | Created `Students` and `Teachers` tables with base columns |
| 2 | `AddStudentUniqueIdAndPassportNo` | 2026-03-04 | Added `StudentUniqueId` and `IdPassportNo` columns + unique index to `Students` |

**Migration files location:**
```
StudentAssessmentTrackerAPI/
  Infrastructure/
    Data/
      Migrations/
        20260303130753_InitialCreate.cs
        20260304125258_AddStudentUniqueIdAndPassportNo.cs
        ApplicationDbContextModelSnapshot.cs
```

---

## Option A — EF Core Migration Rollback (Schema Changes)

Use this when: you ran a bad migration or want to undo a schema change.

> **Warning:** Rolling back a migration does NOT restore data deleted during that migration. Use Option B if you need to preserve data.

### Prerequisites
```powershell
# Ensure EF Core tools are installed
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
```

### Step 1 — Check Current Migration State
```powershell
# Navigate to the API project folder
cd "C:\Users\Developer.03\Desktop\Student-Assessment-Tracker\StudentAssessmentTrackerAPI"

# List all migrations and their applied status
dotnet ef migrations list
```

Expected output:
```
20260303130753_InitialCreate (applied)
20260304125258_AddStudentUniqueIdAndPassportNo (applied)
```

### Step 2 — Roll Back to a Specific Migration

**Roll back to `InitialCreate` (undoes `AddStudentUniqueIdAndPassportNo`):**
```powershell
dotnet ef database update InitialCreate
```
This will drop the `StudentUniqueId` and `IdPassportNo` columns from the `Students` table.

**Roll back everything (empty database, tables dropped):**
```powershell
dotnet ef database update 0
```

### Step 3 — Reapply Migrations (bring back to latest)
```powershell
dotnet ef database update
```

### Adding a New Migration (after fixing your code)
```powershell
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Removing the Last (unapplied) Migration
```powershell
# Only works if the migration has NOT been applied to the database yet
dotnet ef migrations remove
```

---

## Option B — Full Database Backup & Restore (Schema + Data)

Use this when: you want to protect or recover actual data records (students, teachers, assessments).

### Connect to the Database

Use one of the following tools:
- **Visual Studio** → View → SQL Server Object Explorer → `(localdb)\mssqllocaldb`
- **SSMS (SQL Server Management Studio)** → Connect to `(localdb)\mssqllocaldb`

---

### Creating a Backup

**Via T-SQL (run in SSMS or VS Query window):**
```sql
-- Create the backup folder first if it doesn't exist
BACKUP DATABASE StudentAssessmentTrackerDev
TO DISK = 'C:\Backups\StudentAssessmentTrackerDev_20260318.bak'
WITH FORMAT, INIT, NAME = 'StudentAssessmentTrackerDev Full Backup';
```

**Via SSMS GUI:**
1. Right-click `StudentAssessmentTrackerDev` → Tasks → Back Up
2. Backup type: `Full`
3. Destination: Add a file path (e.g., `C:\Backups\StudentAssessmentTrackerDev.bak`)
4. Click OK

> **Tip:** Include the date in the filename (e.g., `_20260318`) so you can identify backups easily.

---

### Restoring from a Backup

**Via T-SQL:**
```sql
-- Disconnect all active connections first
ALTER DATABASE StudentAssessmentTrackerDev SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

-- Restore the database
RESTORE DATABASE StudentAssessmentTrackerDev
FROM DISK = 'C:\Backups\StudentAssessmentTrackerDev_20260318.bak'
WITH REPLACE, RECOVERY;

-- Re-enable multi-user access
ALTER DATABASE StudentAssessmentTrackerDev SET MULTI_USER;
```

**Via SSMS GUI:**
1. Right-click Databases → Restore Database
2. Device → `...` → Add → select your `.bak` file
3. Check `Overwrite the existing database (WITH REPLACE)`
4. Click OK

---

### Recommended Backup Schedule

| When | Action |
|---|---|
| Before running a new migration | Take a full `.bak` backup |
| Before bulk insert/update/delete operations | Take a full `.bak` backup |
| After a successful sprint / feature completion | Take a full `.bak` backup |
| Before sharing the database with a teammate | Take a full `.bak` backup |

---

## Option C — Nuclear Reset (Drop & Recreate)

Use this when: the database is completely broken and you want a clean fresh start. **All data will be lost.**

```powershell
cd "C:\Users\Developer.03\Desktop\Student-Assessment-Tracker\StudentAssessmentTrackerAPI"

# Drop the entire database
dotnet ef database drop --force

# Recreate it by applying all migrations from scratch
dotnet ef database update
```

This rebuilds the database to the current schema state with no data.

---

## Quick Decision Guide

```
Something went wrong with the database...
│
├─ Did you run a bad migration (wrong columns, wrong types)?
│   └─ YES → Option A: dotnet ef database update <PreviousMigrationName>
│
├─ Did you accidentally delete or corrupt data?
│   └─ YES → Option B: Restore from .bak backup
│
├─ Did you run both a bad migration AND lose data?
│   └─ YES → Option B: Restore from .bak backup (restores both schema and data)
│
├─ Is the database just completely broken and no backup exists?
│   └─ YES → Option C: Drop and recreate (you will lose all data)
│
└─ Do you just want to see what migrations are applied?
    └─ YES → dotnet ef migrations list
```

---

## Common Scenarios & Solutions

### Scenario 1: "I ran `dotnet ef database update` and something broke"
```powershell
# Roll back to the previous migration
dotnet ef database update InitialCreate
# Fix your migration code, then reapply
dotnet ef database update
```

### Scenario 2: "I accidentally deleted all students/teachers via the API"
Restore from your most recent `.bak` backup using Option B.  
If no backup exists, the data is unrecoverable — this is why backups matter.

### Scenario 3: "I want to test a risky database change safely"
1. Take a full backup first:
   ```sql
   BACKUP DATABASE StudentAssessmentTrackerDev
   TO DISK = 'C:\Backups\before_risky_change.bak'
   WITH FORMAT, INIT;
   ```
2. Make your changes.
3. If something breaks, restore using Option B.

### Scenario 4: "The API won't start — database connection error"
Check that LocalDB is running:
```powershell
# List LocalDB instances
sqllocaldb info

# Start the instance if stopped
sqllocaldb start MSSQLLocalDB

# Check instance info
sqllocaldb info MSSQLLocalDB
```

Then verify the connection string in `appsettings.Development.json`:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentAssessmentTrackerDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### Scenario 5: "I want to seed fresh test data after a reset"
After Option C (nuclear reset), reapply migrations then manually insert test data via:
- Postman (use the existing `StudentAssessmentTracker.postman_collection.json`)
- SSMS SQL scripts
- A future database seeder class in `Infrastructure/Data/`

---

## Prerequisites

| Tool | Purpose | Install |
|---|---|---|
| .NET 8 SDK | Run `dotnet ef` commands | Pre-installed with project |
| EF Core CLI Tools | Migration commands | `dotnet tool install --global dotnet-ef` |
| SSMS (optional) | GUI for backups/restores | [Download from Microsoft](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) |
| SQL Server LocalDB | The database engine itself | Installed with Visual Studio |

---

> **Remember:** A backup you never took cannot save you. When in doubt, back up first.
