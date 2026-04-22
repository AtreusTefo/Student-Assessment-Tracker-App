# ORM Developer Report

**Project:** Student Assessment Tracker
**Stack:** ASP.NET Core 8.0 / SQL Server
**Date:** March 6, 2026

---

## 1. What is an ORM?

An **ORM (Object-Relational Mapper)** is a library that acts as a bridge between C# classes and a relational database. Instead of writing raw SQL, you work with C# objects and the ORM translates your code into SQL automatically.

Without an ORM you have to write SQL strings manually and map every column by hand (raw ADO.NET). With an ORM, this is handled for you:

```csharp
// Without ORM  manual, error-prone
var cmd = new SqlCommand("SELECT * FROM Students WHERE Id = @id", conn);
var reader = cmd.ExecuteReader();
var student = new Student { Id = (int)reader["Id"], FirstName = (string)reader["FirstName"] };

// With ORM (EF Core)  one line
var student = await _context.Students.FindAsync(1);
```

---

## 2. Entity Framework Core (EF Core)

**EF Core** is Microsoft's official full ORM for ASP.NET Core. This project uses **EF Core 8.0** with SQL Server.

It handles:
- Translating C# LINQ queries into SQL
- Mapping SQL results back into C# objects
- Tracking changes and generating UPDATE statements automatically
- Creating and updating database tables via **Migrations**

### DbContext and DbSet

`DbContext` manages the database session. Each `DbSet<T>` property represents one table.

```csharp
// ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Student> Students { get; set; }  //  Students table
    public DbSet<Teacher> Teachers { get; set; }  //  Teachers table
}
```

### Fluent API  Schema Configuration

`OnModelCreating()` configures column constraints in C# instead of writing SQL DDL:

```csharp
modelBuilder.Entity<Student>(entity =>
{
    entity.HasKey(e => e.Id);                                        // PRIMARY KEY
    entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50); // NOT NULL, NVARCHAR(50)
    entity.HasIndex(e => e.Email).IsUnique();                        // UNIQUE INDEX
    entity.Property(e => e.Assessment1).HasColumnType("decimal(5,2)");
    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
});
```

### LINQ Queries

EF Core translates C# LINQ into SQL at runtime:

```csharp
// C# LINQ  SQL generated automatically
_context.Students
    .Where(s => s.Grade == "A")
    .OrderBy(s => s.LastName)
    .ToListAsync();
//  SELECT * FROM [Students] WHERE [Grade]='A' ORDER BY [LastName]
```

### Change Tracking

EF Core watches loaded objects. When a property changes and `SaveChangesAsync()` is called, it generates an UPDATE for only the changed columns.

`AsNoTracking()` disables this for read-only queries (faster), which is why it is used in `StudentRepository.GetAllAsync()`.

### Migrations

Migrations translate C# entity classes into actual database tables:

```bash
dotnet ef migrations add InitialCreate   # snapshot the current model
dotnet ef database update                # run CREATE TABLE against the database
```

### Registration in Program.cs

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

---

## 3. Entity Framework 6 (EF6)  Legacy

**EF6** is the older ORM used in classic **ASP.NET** (Windows-only, .NET Framework). It works the same way as EF Core in concept  `DbContext`, `DbSet<T>`, Fluent API  but has key differences:

| | EF6 (Legacy) | EF Core (This Project) |
|---|---|---|
| **Framework** | .NET Framework (Windows only) | .NET 5/6/7/8 (cross-platform) |
| **Namespace** | `System.Data.Entity` | `Microsoft.EntityFrameworkCore` |
| **Config file** | `Web.config` (XML) | `appsettings.json` (JSON) |
| **DI support** | Manual | Native ASP.NET Core DI |
| **Migration CLI** | Package Manager Console (Visual Studio) | `dotnet ef` (any terminal) |
| **Status** | Maintenance only  no new features | Actively developed |

EF6 could not be updated to support cross-platform .NET Core because it was too tightly coupled to the Windows-only `System.Data` infrastructure. Microsoft rewrote it from scratch as EF Core.

---

## 4. Dapper

**Dapper** is a **micro-ORM** created by the Stack Overflow team. It does one thing only: maps SQL query results to C# objects. You write all the SQL yourself.

```csharp
using Dapper;

var conn = new SqlConnection(connectionString);

// You write the SQL  Dapper maps the result to Student objects
var students = await conn.QueryAsync<Student>(
    "SELECT * FROM Students WHERE Grade = @Grade",
    new { Grade = "A" }
);
```

**What Dapper does NOT do:**
- No migrations / table creation
- No change tracking  you write UPDATE SQL manually
- No LINQ  raw SQL strings only
- No relationship loading  you write JOINs yourself

---

## 5. EF Core vs Dapper

| Feature | EF Core | Dapper |
|---|---|---|
| SQL knowledge required | Minimal | Essential |
| Query language | C# LINQ | Raw SQL |
| Change tracking | Yes (automatic) | No |
| Schema / Migrations | Yes (built-in) | No |
| Performance | Good | Excellent (near raw ADO.NET) |
| Best for | CRUD-heavy APIs, rapid development | Complex queries, performance-critical reads |

Many projects use **both**: EF Core for standard CRUD, Dapper for complex reports or analytics queries.

---

## 6. How This Project Uses EF Core

```
HTTP Request
     StudentsController
     StudentService          (business logic)
     StudentRepository       (data access)
     ApplicationDbContext    (EF Core  manages DB session)
     SQL Server              (Students / Teachers tables)
```

The `Repository<T>` class wraps all EF Core calls (GetById, GetAll, Add, Update, Delete) and `StudentRepository` extends it with a student-specific `GetAllAsync()` that uses `AsNoTracking()` and sorts by last name.
