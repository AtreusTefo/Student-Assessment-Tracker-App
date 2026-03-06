# ORM Developer Report
## Object-Relational Mappers: EF Core, EF6 & Dapper

**Project:** Student Assessment Tracker  
**Stack:** ASP.NET Core 8.0 / SQL Server  
**Date:** March 6, 2026  

---

## Table of Contents

1. [What is an ORM?](#1-what-is-an-orm)
2. [Entity Framework Core](#2-entity-framework-core)
3. [Entity Framework 6 â€” Legacy](#3-entity-framework-6--legacy)
4. [Dapper](#4-dapper)
5. [EF Core vs Dapper](#5-ef-core-vs-dapper)
6. [Database Support](#6-database-support)
7. [Architecture Diagram](#7-architecture-diagram)
8. [Summary](#8-summary)

---

## 1. What is an ORM?

**ORM** stands for **Object-Relational Mapper** â€” a library that bridges C# classes and relational database tables. Instead of writing raw SQL, you work with regular C# objects and the ORM translates operations into SQL automatically.

- **Object** â†’ C# classes (`Student`, `Teacher`)
- **Relational** â†’ SQL database tables
- **Mapper** â†’ the component that links them together

### Without ORM vs With ORM

```csharp
// Without ORM â€” raw ADO.NET (manual, repetitive, error-prone)
var cmd = new SqlCommand("SELECT * FROM Students WHERE Id = @id", conn);
cmd.Parameters.AddWithValue("@id", 1);
var reader = cmd.ExecuteReader();
var student = new Student { Id = (int)reader["Id"], FirstName = (string)reader["FirstName"] };

// With ORM â€” Entity Framework Core
var student = await _context.Students.FindAsync(1);  // fully mapped automatically
```

**ORMs provide:** automatic SQL generation, result mapping, schema creation, change tracking, and built-in SQL injection protection.

---

## 2. Entity Framework Core

EF Core is Microsoft's official **full ORM** for ASP.NET Core. This project uses **EF Core 8.0** with SQL Server.

### NuGet Packages (`StudentAssessmentTracker.csproj`)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore"           Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design"    Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools"     Version="8.0.0" />
```

### DbContext â€” The Database Session

`ApplicationDbContext.cs` manages the database connection and all entity operations. One instance is created per HTTP request via the DI container.

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }  // â†’ Students table
    public DbSet<Teacher> Teachers { get; set; }  // â†’ Teachers table
}
```

### Entities â€” C# Classes as Tables

The `Student` class in `Domain/Entities/Student.cs` maps directly to the `Students` table. Each property becomes a column â€” EF Core generates the `CREATE TABLE` SQL automatically.

### Fluent API â€” Schema Configuration

`OnModelCreating()` in `ApplicationDbContext.cs` configures column types and constraints:

```csharp
entity.HasKey(e => e.Id);                                            // PRIMARY KEY
entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);     // NOT NULL, NVARCHAR(50)
entity.HasIndex(e => e.Email).IsUnique();                            // UNIQUE INDEX
entity.Property(e => e.Assessment1).HasColumnType("decimal(5,2)");   // DECIMAL(5,2)
entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");// DEFAULT GETUTCDATE()
```

### LINQ Queries â€” Write C#, Get SQL

```csharp
// EF Core translates LINQ into SQL automatically
_context.Students
    .Where(s => s.Grade == "A")       // WHERE Grade = 'A'
    .OrderBy(s => s.LastName)         // ORDER BY LastName
    .ToListAsync();
```

No SQL strings in the code â€” compile-time safe and database-portable.

### Change Tracking

EF Core snapshots loaded entities. On `SaveChangesAsync()`, it generates `UPDATE` for only the changed columns. Use `AsNoTracking()` on read-only queries â€” as done in `StudentRepository.GetAllAsync()` â€” for better performance.

### Migrations â€” Schema Version Control

```bash
dotnet ef migrations add InitialCreate   # creates a versioned .cs migration file
dotnet ef database update                # applies it â†’ CREATE TABLE in SQL Server
```

Migrations track schema history in source control, keeping every developer's database in sync.

### Registration in `Program.cs`

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), null)));
```

---

## 3. Entity Framework 6 â€” Legacy

EF6 is the older ORM built for classic **ASP.NET** (Windows-only, .NET Framework). It is in **maintenance mode** â€” bug fixes only, no new features.

| | EF6 (Legacy) | EF Core (This Project) |
|---|---|---|
| **Framework** | .NET Framework 4.x | .NET 5 / 6 / 7 / 8 |
| **Platform** | Windows only | Cross-platform |
| **Namespace** | `System.Data.Entity` | `Microsoft.EntityFrameworkCore` |
| **Config file** | `Web.config` (XML) | `appsettings.json` (JSON) |
| **DI support** | Manual | Native ASP.NET Core DI |
| **Migration CLI** | Package Manager Console (Visual Studio only) | `dotnet ef` (any terminal) |
| **Status** | Maintenance only | Actively developed |

```csharp
// EF6 â€” reads connection string from Web.config
using System.Data.Entity;
public class SchoolDbContext : DbContext
{
    public SchoolDbContext() : base("name=SchoolDb") { }
    public DbSet<Student> Students { get; set; }
}

// EF6 migrations â€” run in Visual Studio Package Manager Console
// Enable-Migrations â†’ Add-Migration InitialCreate â†’ Update-Database
```

**Why EF Core was a full rewrite:** EF6 was tightly coupled to `System.Data` in .NET Framework and could not be ported cross-platform. Microsoft rebuilt it from scratch as EF Core.

---

## 4. Dapper

Dapper is a **micro-ORM** created by the Stack Overflow team. It does one thing: maps SQL query results to C# objects. You write all the SQL yourself.

> **This project does not use Dapper** â€” it uses EF Core exclusively.

```csharp
using Dapper;
await using var conn = new SqlConnection(connectionString);

// READ â€” maps rows to Student objects by matching column names to property names
var students = await conn.QueryAsync<Student>("SELECT * FROM Students ORDER BY LastName");
var student  = await conn.QueryFirstOrDefaultAsync<Student>(
                   "SELECT * FROM Students WHERE Id = @Id", new { Id = 1 });

// WRITE â€” INSERT, UPDATE, DELETE
await conn.ExecuteAsync("UPDATE Students SET Grade = @Grade WHERE Id = @Id",
                        new { Grade = "A", Id = 1 });
```

**What Dapper does NOT do:** generate SQL, track changes, create tables, manage migrations, or load relationships. You are responsible for all of that.

---

## 5. EF Core vs Dapper

| Feature | EF Core | Dapper |
|---|---|---|
| **Type** | Full ORM | Micro-ORM |
| **SQL required** | No (LINQ) | Yes â€” write all SQL |
| **Result mapping** | Automatic | Automatic |
| **Change tracking** | Yes | No |
| **Schema / Migrations** | Yes | No â€” schema must already exist |
| **Relationships** | `Include()` | Manual JOINs |
| **Performance** | Good | Excellent (near raw ADO.NET) |
| **Complex queries** | Can be awkward in LINQ | Full SQL control |
| **Boilerplate** | Very low | More code |
| **Best for** | CRUD APIs, rapid development | Reports, analytics, performance reads |

Many production apps use **both**: EF Core for CRUD, Dapper for complex queries.

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(conn)); // EF Core
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(conn));        // Dapper
```

---

## 6. Database Support

Both tools are database-agnostic â€” SQL Server is used in this project, but both support others:

| Database | EF Core Provider | Dapper Connection |
|---|---|---|
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` | `SqlConnection` |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` | `NpgsqlConnection` |
| MySQL | `Pomelo.EntityFrameworkCore.MySql` | `MySqlConnection` |
| SQLite | `Microsoft.EntityFrameworkCore.Sqlite` | `SqliteConnection` |

With EF Core, switching databases is one line in `Program.cs` â€” the C# code and LINQ queries stay the same.

---

## 7. Architecture Diagram

```
HTTP Request
     â”‚
     â–¼
StudentsController        â† Presentation Layer (HTTP, JSON)
     â”‚
     â–¼
StudentService            â† Application Layer (business logic, DTOs)
     â”‚
     â–¼
StudentRepository         â† Infrastructure Layer (data access)
     â”‚
     â–¼
ApplicationDbContext      â† EF Core (DB session, LINQ â†’ SQL, change tracking)
     â”‚
     â–¼
SQL Server Database       â† Students / Teachers tables
```

---

## 8. Summary

| Concept | Takeaway |
|---|---|
| **ORM** | Bridges C# objects â†” SQL tables; eliminates manual SQL and mapping |
| **EF Core** | Full ORM â€” LINQ queries, change tracking, migrations, native DI |
| **EF6** | Legacy EF for classic ASP.NET (Windows-only); maintenance mode |
| **Dapper** | Micro-ORM â€” fast, manual SQL, result mapping only |
| **This project** | EF Core 8.0 + SQL Server, Code-First, Repository Pattern |
| **DbContext** | One per HTTP request â€” manages the DB connection and change tracking |
| **DbSet\<T\>** | Represents one database table inside the DbContext |
| **Migrations** | `dotnet ef migrations add` â†’ `dotnet ef database update` |
| **AsNoTracking()** | Skip change tracking on read-only queries â€” used in `StudentRepository` |
| **SQL injection** | EF Core and Dapper both use parameterized queries â€” safe by default |

