# ORM Developer Report
## Object-Relational Mappers: What They Are, Entity Framework Core & Dapper

**Project:** Student Assessment Tracker  
**Stack:** ASP.NET Core 8.0 / SQL Server  
**Date:** March 6, 2026  

---

## Table of Contents

1. [What is an ORM?](#1-what-is-an-orm)
2. [The Problem ORMs Solve](#2-the-problem-orms-solve)
3. [Entity Framework Core](#3-entity-framework-core)
   - 3.1 [What is EF Core?](#31-what-is-ef-core)
   - 3.2 [How EF Core Works in This Project](#32-how-ef-core-works-in-this-project)
   - 3.3 [DbContext](#33-dbcontext)
   - 3.4 [Entities — C# Classes as Tables](#34-entities--c-classes-as-tables)
   - 3.5 [Fluent API Configuration](#35-fluent-api-configuration)
   - 3.6 [LINQ Queries — Write C#, Get SQL](#36-linq-queries--write-c-get-sql)
   - 3.7 [Change Tracking](#37-change-tracking)
   - 3.8 [Migrations — Version-Controlled Schema](#38-migrations--version-controlled-schema)
   - 3.9 [Repository Pattern with EF Core](#39-repository-pattern-with-ef-core)
   - 3.10 [Registering EF Core in Program.cs](#310-registering-ef-core-in-programcs)
4. [Dapper](#4-dapper)
   - 4.1 [What is Dapper?](#41-what-is-dapper)
   - 4.2 [How Dapper Works](#42-how-dapper-works)
   - 4.3 [What Dapper Does NOT Do](#43-what-dapper-does-not-do)
   - 4.4 [Equivalent Repository Methods in Dapper](#44-equivalent-repository-methods-in-dapper)
5. [EF Core vs Dapper — Full Comparison](#5-ef-core-vs-dapper--full-comparison)
6. [Database Support — Not Just SQL Server](#6-database-support--not-just-sql-server)
7. [ASP.NET vs ASP.NET Core](#7-aspnet-vs-aspnet-core)
8. [Architecture Diagram](#8-architecture-diagram)
9. [Summary](#9-summary)

---

## 1. What is an ORM?

**ORM** stands for **Object-Relational Mapper**.

It is a software library that acts as a **bridge between your object-oriented application code (C# classes) and a relational database (SQL tables)**. Instead of writing raw SQL queries inside your C# code, an ORM lets you work with regular C# objects and methods, translating those operations into SQL automatically behind the scenes.

The name breaks down as:
- **Object** — the C# classes and objects in your application (e.g., `Student`, `Teacher`)
- **Relational** — the relational database storing data in tables and rows (e.g., SQL Server)
- **Mapper** — the layer that maps (links) C# objects to database rows and vice versa

---

## 2. The Problem ORMs Solve

Relational databases store data in **tables with rows and columns**. Object-oriented languages represent data as **objects with properties and methods**. These are fundamentally different structures — this mismatch is known as the **"Object-Relational Impedance Mismatch"**.

### Without an ORM (Raw ADO.NET)

Without an ORM, every database interaction requires writing raw SQL strings and manually mapping every column to a property:

```csharp
// WITHOUT an ORM — manual ADO.NET
using var conn = new SqlConnection(connectionString);
conn.Open();

var cmd = new SqlCommand(
    "SELECT Id, FirstName, LastName, Email, Grade FROM Students WHERE Id = @id", conn);
cmd.Parameters.AddWithValue("@id", 1);

using var reader = cmd.ExecuteReader();
if (reader.Read())
{
    var student = new Student
    {
        Id         = (int)reader["Id"],
        FirstName  = (string)reader["FirstName"],
        LastName   = (string)reader["LastName"],
        Email      = (string)reader["Email"],
        Grade      = (string)reader["Grade"]
        // every single column must be manually mapped
    };
}
```

Problems with this approach:
- **Repetitive and error-prone** — every query requires the same boilerplate code
- **SQL injection risk** — if parameters are not carefully handled, the app is vulnerable
- **Hard to maintain** — renaming a column in the database means hunting through every SQL string in the code
- **No type safety** — column names are plain strings; typos cause runtime errors, not compile errors

### With an ORM

```csharp
// WITH an ORM — Entity Framework Core
var student = await _context.Students.FindAsync(1);
// student is already a fully-typed Student object — no manual mapping needed
```

The ORM handles the SQL generation, execution, and result mapping automatically.

### What an ORM Does for You

| Concern | Without ORM | With ORM |
|---|---|---|
| Writing SQL | Raw SQL strings in code | C# LINQ or method calls |
| Mapping results | Manual column-by-column | Automatic |
| Schema changes | Edit SQL scripts + update mappings | Edit the C# class only |
| SQL injection protection | Must be handled manually | Handled automatically |
| Database portability | Vendor-specific SQL | Swap the provider in one line |
| Change tracking | Not available | Automatic (in full ORMs) |

---

## 3. Entity Framework Core

### 3.1 What is EF Core?

**Entity Framework Core (EF Core)** is Microsoft's official, **full-featured ORM** for .NET. It is the primary data access technology for modern ASP.NET Core applications and is included in the official .NET ecosystem.

It is described as a **"full ORM"** because it handles the entire data access lifecycle:
- Translating C# LINQ queries into SQL
- Executing those queries against the database
- Mapping results back into C# objects
- Tracking changes to objects
- Generating database schema (tables, indexes, constraints) via Migrations

This project uses **EF Core 8.0**, referencing these NuGet packages defined in
`StudentAssessmentTrackerAPI/StudentAssessmentTracker.csproj`:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore"           Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design"    Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools"     Version="8.0.0" />
```

- **`Microsoft.EntityFrameworkCore`** — the core ORM engine
- **`Microsoft.EntityFrameworkCore.SqlServer`** — the SQL Server database provider
- **`Microsoft.EntityFrameworkCore.Design`** — used at design time to scaffold migrations
- **`Microsoft.EntityFrameworkCore.Tools`** — CLI tools for running `dotnet ef` commands

---

### 3.2 How EF Core Works in This Project

The EF Core pipeline in this project flows as follows:

```
HTTP Request (e.g., GET /api/students)
        │
        ▼
  StudentsController
        │
        ▼
  StudentService         ← Application Layer (business logic)
        │
        ▼
  StudentRepository      ← Infrastructure Layer (data access)
        │
        ▼
  ApplicationDbContext   ← EF Core DbContext (manages DB session)
        │  generates SQL
        ▼
  SQL Server Database    ← actual Students / Teachers tables
```

---

### 3.3 DbContext

`DbContext` is the central class in EF Core. It represents a **session with the database** and is responsible for:
- Holding the database connection
- Exposing `DbSet<T>` properties representing tables
- Tracking changes to loaded entities
- Executing queries and saving changes

This project's DbContext lives in
`StudentAssessmentTrackerAPI/Infrastructure/Data/ApplicationDbContext.cs`:

```csharp
public class ApplicationDbContext : DbContext
{
    // Constructor receives configuration options (connection string, etc.)
    // injected by the ASP.NET Core DI container
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Each DbSet<T> represents one database TABLE
    public DbSet<Student> Students { get; set; }   // → Students table
    public DbSet<Teacher> Teachers { get; set; }   // → Teachers table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fine-grained schema configuration (see Section 3.5)
    }
}
```

**Important rules about `DbContext`:**
- It is registered as **scoped** in ASP.NET Core — one instance per HTTP request
- It is **not thread-safe** — never share a context across threads
- Always dispose it when done (the DI container handles this automatically in web apps)

---

### 3.4 Entities — C# Classes as Tables

In EF Core's **Code-First** approach, your C# domain classes define the database schema. EF Core reads the classes and generates the corresponding `CREATE TABLE` statements.

This project's `Student` entity in
`StudentAssessmentTrackerAPI/Domain/Entities/Student.cs` maps directly to the `Students` table:

```csharp
public class Student
{
    public int Id { get; set; }              // → INT PRIMARY KEY (auto-increment)
    public string? StudentUniqueId { get; set; } // → NVARCHAR(20) UNIQUE
    public string? IdPassportNo { get; set; }    // → NVARCHAR(20)
    public string? FirstName { get; set; }       // → NVARCHAR(50) NOT NULL
    public string? LastName { get; set; }        // → NVARCHAR(50) NOT NULL
    public string? Email { get; set; }           // → NVARCHAR(255) UNIQUE
    public string? Phone { get; set; }           // → NVARCHAR(8)
    public string? Grade { get; set; }           // → NVARCHAR(10)
    public decimal Assessment1 { get; set; }     // → DECIMAL(5,2)
    public decimal Assessment2 { get; set; }     // → DECIMAL(5,2)
    public decimal Assessment3 { get; set; }     // → DECIMAL(5,2)
    public DateTime CreatedAt { get; set; }      // → DATETIME2 DEFAULT GETUTCDATE()
    public DateTime UpdatedAt { get; set; }      // → DATETIME2 DEFAULT GETUTCDATE()

    // Domain method — does NOT map to a column (no setter)
    public decimal GetTotalScore() => Assessment1 + Assessment2 + Assessment3;
}
```

**Key principle:** The C# class is the single source of truth. Change the class, run a migration, and the database reflects the update — no manual SQL schema editing required.

---

### 3.5 Fluent API Configuration

EF Core's **Fluent API**, configured inside `OnModelCreating()`, allows precise control over how C# properties map to SQL columns — without needing to use data annotation attributes on the entity class.

From `ApplicationDbContext.cs`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Student>(entity =>
    {
        // Define the PRIMARY KEY
        entity.HasKey(e => e.Id);

        // StudentUniqueId → NVARCHAR(20) NOT NULL with a UNIQUE INDEX
        entity.Property(e => e.StudentUniqueId)
            .IsRequired()
            .HasMaxLength(20);
        entity.HasIndex(e => e.StudentUniqueId).IsUnique();

        // FirstName → NVARCHAR(50) NOT NULL
        entity.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        // Email → NVARCHAR(255) NOT NULL with a UNIQUE INDEX
        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
        entity.HasIndex(e => e.Email).IsUnique();

        // Assessment scores → DECIMAL(5,2) columns
        entity.Property(e => e.Assessment1).HasColumnType("decimal(5,2)");
        entity.Property(e => e.Assessment2).HasColumnType("decimal(5,2)");
        entity.Property(e => e.Assessment3).HasColumnType("decimal(5,2)");

        // Timestamps → SQL Server default value of current UTC time
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
    });
}
```

Each Fluent API call maps directly to a SQL constraint or column option:

| Fluent API Call | Generated SQL Equivalent |
|---|---|
| `HasKey(e => e.Id)` | `PRIMARY KEY` |
| `IsRequired()` | `NOT NULL` |
| `HasMaxLength(50)` | `NVARCHAR(50)` |
| `HasIndex(...).IsUnique()` | `UNIQUE INDEX` |
| `HasColumnType("decimal(5,2)")` | `DECIMAL(5,2)` |
| `HasDefaultValueSql("GETUTCDATE()")` | `DEFAULT GETUTCDATE()` |

---

### 3.6 LINQ Queries — Write C#, Get SQL

EF Core translates **LINQ (Language Integrated Query)** — a C# querying syntax — into SQL at runtime. The developer never writes SQL strings; they write C# expressions.

```csharp
// C# LINQ                                   SQL EF Core generates
// ─────────────────────────────────────────────────────────────────

// Get all students
_context.Students.ToListAsync();
// → SELECT * FROM [Students]

// Filter by grade
_context.Students
    .Where(s => s.Grade == "A")
    .ToListAsync();
// → SELECT * FROM [Students] WHERE [Grade] = 'A'

// Sort results
_context.Students
    .OrderBy(s => s.LastName)
    .ThenBy(s => s.FirstName)
    .ToListAsync();
// → SELECT * FROM [Students] ORDER BY [LastName], [FirstName]

// Select specific columns (projection)
_context.Students
    .Select(s => new { s.FirstName, s.LastName, s.Email })
    .ToListAsync();
// → SELECT [FirstName], [LastName], [Email] FROM [Students]

// Find a single record by ID
_context.Students.FindAsync(1);
// → SELECT TOP(1) * FROM [Students] WHERE [Id] = 1
```

This means:
- **No SQL strings** scattered through the code
- **Compile-time safety** — a typo in a property name is caught by the compiler, not at runtime
- **Database portability** — the same LINQ query works with SQL Server, PostgreSQL, MySQL, etc.

---

### 3.7 Change Tracking

One of EF Core's most powerful features is **Change Tracking**. When EF Core loads an entity from the database, it keeps an internal snapshot of its original values. When `SaveChangesAsync()` is called, EF Core compares the current state to the snapshot and automatically generates `UPDATE` statements for only the changed columns.

```csharp
// 1. Load a student — EF Core snapshots the original values
var student = await _context.Students.FindAsync(5);
// student.Grade is currently "B"

// 2. Modify a property
student.Grade = "A";
student.UpdatedAt = DateTime.UtcNow;

// 3. Save — EF Core detects what changed and generates minimal SQL
await _context.SaveChangesAsync();
// → UPDATE [Students] SET [Grade]='A', [UpdatedAt]='2026-03-06'
//   WHERE [Id] = 5
// Only the changed columns are included — not the entire row
```

**`AsNoTracking()`** — When you only need to read data (no updates), you can disable change tracking for better performance:

```csharp
// Used in StudentRepository.GetAllAsync()
return await _context.Students
    .AsNoTracking()        // ← skip the snapshot overhead
    .OrderBy(s => s.LastName)
    .ThenBy(s => s.FirstName)
    .ToListAsync();
```

This is used in `StudentRepository.GetAllAsync()` (in `StudentRepository.cs`) because listing students is a read-only operation — there is no need to track changes, so `AsNoTracking()` gives a measurable performance improvement.

---

### 3.8 Migrations — Version-Controlled Schema

EF Core **Migrations** are the mechanism that translates your C# entity classes into actual database table definitions. They work similarly to version control (like Git) for your database schema — every change is tracked in a timestamped file.

**Migration Workflow:**

```bash
# Step 1 — Create a migration snapshot after changing an entity class
dotnet ef migrations add InitialCreate
# EF Core reads all DbSet<T> + OnModelCreating config and creates a .cs migration file

# Step 2 — Apply the migration to the actual database
dotnet ef database update
# EF Core runs the generated CREATE TABLE / ALTER TABLE SQL against the database
```

The generated migration file looks like:

```csharp
// Auto-generated by EF Core — do not edit manually
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id             = table.Column<int>(nullable: false)
                                      .Annotation("SqlServer:Identity", "1, 1"),
                StudentUniqueId = table.Column<string>(maxLength: 20, nullable: false),
                FirstName      = table.Column<string>(maxLength: 50, nullable: false),
                Email          = table.Column<string>(maxLength: 255, nullable: false),
                Assessment1    = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                CreatedAt      = table.Column<DateTime>(nullable: false,
                                      defaultValueSql: "GETUTCDATE()")
                // ... all other columns
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
            });

        migrationBuilder.CreateIndex("IX_Students_Email", "Students", "Email", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback: drop the table
        migrationBuilder.DropTable(name: "Students");
    }
}
```

**Why Migrations matter:**
- The schema is tracked in source control alongside the code
- Every developer on the team runs the same migrations to keep databases in sync
- Rollbacks are possible using the `Down()` method
- No manual `CREATE TABLE` SQL scripts to maintain separately

---

### 3.9 Repository Pattern with EF Core

This project wraps EF Core inside the **Repository Pattern** to keep data access logic separated from business logic. The repository is in
`StudentAssessmentTrackerAPI/Infrastructure/Repositories/StudentRepository.cs`.

**Generic Repository** — handles standard CRUD for any entity type:

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;

    public Repository(ApplicationDbContext context)
    {
        _context = context;   // Injected by ASP.NET Core's DI container
    }

    // READ — SELECT * WHERE Id = @id
    public virtual async Task<T?> GetByIdAsync(int id)
        => await _context.Set<T>().FindAsync(id);

    // READ ALL — SELECT * FROM table
    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _context.Set<T>().ToListAsync();

    // CREATE — INSERT INTO table VALUES (...)
    public virtual async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    // UPDATE — UPDATE table SET ... WHERE Id = @id
    public virtual async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    // DELETE — DELETE FROM table WHERE Id = @id
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
```

**Student-Specific Repository** — extends generic with a custom read-all query:

```csharp
public class StudentRepository : Repository<Student>
{
    public StudentRepository(ApplicationDbContext context) : base(context) { }

    // Overrides the generic GetAllAsync with a student-specific implementation:
    // - AsNoTracking() for read-only performance
    // - Sorted by LastName then FirstName
    public override async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
        // → SELECT * FROM [Students] ORDER BY [LastName], [FirstName]
    }
}
```

---

### 3.10 Registering EF Core in Program.cs

EF Core is registered with ASP.NET Core's **Dependency Injection container** in `Program.cs`:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});
```

- `AddDbContext<ApplicationDbContext>()` — registers the context as **Scoped** (one instance per HTTP request)
- `UseSqlServer(connectionString)` — selects SQL Server as the database provider
- `EnableRetryOnFailure()` — automatically retries failed queries up to 5 times (resilience for transient network errors)
- The connection string itself lives in `appsettings.json` (never hardcoded)

---

## 4. Dapper

### 4.1 What is Dapper?

**Dapper** is a **micro-ORM** for .NET, created by the Stack Overflow engineering team. It was born from the need to handle extreme performance demands on one of the world's most-visited websites.

Unlike EF Core (a full ORM), Dapper is deliberately minimal. It does exactly **one thing**: it takes the results of a SQL query you wrote and **maps them into C# objects**. Everything else — writing the SQL, managing the schema, tracking changes — is your responsibility.

Dapper is distributed as the **`Dapper`** NuGet package:

```xml
<PackageReference Include="Dapper" Version="2.1.35" />
```

> **This project does not use Dapper** — it uses EF Core exclusively. The Dapper examples below show what equivalent operations would look like if Dapper were used instead, for comparison purposes.

---

### 4.2 How Dapper Works

Dapper adds **extension methods** directly to `IDbConnection` — the standard .NET database connection interface. You open a connection, pass a SQL string plus optional parameters, and Dapper maps the results to your C# class by matching column names to property names.

```csharp
using Dapper;
using Microsoft.Data.SqlClient;

await using var conn = new SqlConnection(connectionString);

// QueryAsync<T> — maps each result row to a Student object
var students = await conn.QueryAsync<Student>(
    "SELECT * FROM Students ORDER BY LastName, FirstName"
);

// Parameterized query — @ parameters prevent SQL injection
var student = await conn.QueryFirstOrDefaultAsync<Student>(
    "SELECT * FROM Students WHERE Id = @Id",
    new { Id = 1 }
);

// Execute — for INSERT, UPDATE, DELETE (returns rows affected)
var rowsAffected = await conn.ExecuteAsync(
    "UPDATE Students SET Grade = @Grade WHERE Id = @Id",
    new { Grade = "A", Id = 1 }
);
```

Dapper matches SQL column names to C# property names **by name** (case-insensitive). If the column is `FirstName`, the C# property must also be `FirstName`.

---

### 4.3 What Dapper Does NOT Do

| Feature | EF Core | Dapper |
|---|---|---|
| Auto-generate SQL | Yes (from LINQ) | **No** — you write all SQL |
| Change tracking | Yes (automatic) | **No** — no tracking |
| Migrations / schema creation | Yes | **No** — schema must exist |
| Relationship loading (JOIN) | Yes (`Include()`) | **No** — write JOINs yourself |
| LINQ support | Full | **No** — SQL strings only |
| Query caching / optimization | Yes | Minimal |

Because Dapper has no change tracking, updates require explicit SQL:

```csharp
// With EF Core — just change the property, EF Core finds the diff
student.Grade = "A";
await _context.SaveChangesAsync();   // EF Core writes the UPDATE automatically

// With Dapper — you must write the entire UPDATE statement yourself
await conn.ExecuteAsync(
    "UPDATE Students SET Grade = @Grade, UpdatedAt = @UpdatedAt WHERE Id = @Id",
    new { Grade = "A", UpdatedAt = DateTime.UtcNow, Id = student.Id }
);
```

---

### 4.4 Equivalent Repository Methods in Dapper

For illustration, here is what the `StudentRepository` from this project would look like if rewritten using Dapper instead of EF Core:

```csharp
public class StudentRepository
{
    private readonly string _connectionString;

    public StudentRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    // READ — equivalent to _context.Students.FindAsync(id)
    public async Task<Student?> GetByIdAsync(int id)
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.QueryFirstOrDefaultAsync<Student>(
            "SELECT * FROM Students WHERE Id = @Id",
            new { Id = id }
        );
    }

    // READ ALL — equivalent to _context.Students.AsNoTracking().OrderBy(...).ToListAsync()
    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.QueryAsync<Student>(
            "SELECT * FROM Students ORDER BY LastName, FirstName"
        );
    }

    // CREATE — equivalent to _context.Students.AddAsync(student) + SaveChangesAsync()
    public async Task AddAsync(Student student)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(@"
            INSERT INTO Students
                (StudentUniqueId, IdPassportNo, FirstName, LastName,
                 Email, Phone, Grade, Assessment1, Assessment2, Assessment3)
            VALUES
                (@StudentUniqueId, @IdPassportNo, @FirstName, @LastName,
                 @Email, @Phone, @Grade, @Assessment1, @Assessment2, @Assessment3)",
            student   // Dapper reads matching properties from the object automatically
        );
    }

    // UPDATE — equivalent to _context.Students.Update(student) + SaveChangesAsync()
    public async Task UpdateAsync(Student student)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(@"
            UPDATE Students
            SET FirstName   = @FirstName,
                LastName    = @LastName,
                Email       = @Email,
                Grade       = @Grade,
                Assessment1 = @Assessment1,
                Assessment2 = @Assessment2,
                Assessment3 = @Assessment3,
                UpdatedAt   = @UpdatedAt
            WHERE Id = @Id",
            student
        );
    }

    // DELETE — equivalent to _context.Students.Remove(entity) + SaveChangesAsync()
    public async Task DeleteAsync(int id)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(
            "DELETE FROM Students WHERE Id = @Id",
            new { Id = id }
        );
    }
}
```

Notice how much more SQL you must write manually with Dapper compared to EF Core, but also how direct and transparent each operation is.

---

## 5. EF Core vs Dapper — Full Comparison

| Category | Entity Framework Core | Dapper |
|---|---|---|
| **Type** | Full ORM | Micro-ORM |
| **SQL knowledge required** | Minimal | Essential |
| **Query language** | C# LINQ (no SQL) | Raw SQL strings |
| **Result mapping** | Automatic | Automatic |
| **Change tracking** | Yes | No |
| **Schema creation (tables)** | Yes — via Migrations | No — schema must pre-exist |
| **Migrations** | Built-in CLI tool | Not available |
| **Relationships** | Automatic (`Include()`) | Manual SQL JOINs |
| **Performance** | Good (slight overhead for tracking) | Excellent (near raw ADO.NET) |
| **Complex queries** | Can be difficult to express in LINQ | Full SQL control |
| **Stored procedures** | Supported | Supported (simpler syntax) |
| **Bulk operations** | Slow without extensions | Fast with raw SQL |
| **Learning curve** | Higher | Low (if you know SQL) |
| **Boilerplate code** | Very low | More (write all SQL) |
| **Best for** | CRUD-heavy apps, rapid development | Complex queries, high-performance reads |
| **NuGet packages** | `Microsoft.EntityFrameworkCore.*` | `Dapper` |

### When to Use EF Core

- Building standard CRUD REST APIs (like this project)
- The team prefers C# over SQL
- Schema changes are frequent and migrations simplify deployment
- You want compile-time safety for queries
- Rapid development speed is a priority

### When to Use Dapper

- You have complex reporting queries with many JOINs, CTEs, or aggregations
- Performance is critical — e.g., bulk reading thousands of rows for analytics
- You are integrating with an existing database schema that cannot be changed
- You need fine-grained control over every SQL statement
- Calling stored procedures is a core part of the data layer

### Using Both Together

Many production projects use **EF Core for CRUD** and **Dapper for complex read queries** side-by-side. Both can be registered in the same DI container:

```csharp
// Program.cs — both registered together
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IDbConnection>(_ =>
    new SqlConnection(connectionString));  // for Dapper

// EF Core handles: Student CRUD, Teacher CRUD
// Dapper handles:  Complex reports, analytics queries with JOINs
```

---

## 6. Database Support — Not Just SQL Server

Both EF Core and Dapper are **database-agnostic**. This project uses SQL Server, but either tool can target multiple database engines by switching the provider or connection type:

| Database | EF Core Provider Package | Dapper Connection Type |
|---|---|---|
| **SQL Server** | `Microsoft.EntityFrameworkCore.SqlServer` | `SqlConnection` |
| **PostgreSQL** | `Npgsql.EntityFrameworkCore.PostgreSQL` | `NpgsqlConnection` |
| **MySQL** | `Pomelo.EntityFrameworkCore.MySql` | `MySqlConnection` |
| **SQLite** | `Microsoft.EntityFrameworkCore.Sqlite` | `SqliteConnection` |
| **Oracle** | `Oracle.EntityFrameworkCore` | `OracleConnection` |

With EF Core, switching databases requires only changing the provider registration in `Program.cs`:

```csharp
// SQL Server (current)
options.UseSqlServer(connectionString);

// Switch to PostgreSQL — same C# code, same LINQ queries, just change this line
options.UseNpgsql(connectionString);
```

---

## 7. ASP.NET vs ASP.NET Core

It is important to understand the distinction between the two frameworks and which ORM version belongs to each:

| | ASP.NET (Legacy) | ASP.NET Core (Modern) |
|---|---|---|
| **Release** | 2002 | 2016 |
| **Platform** | Windows only | Windows, Linux, macOS |
| **ORM used** | Entity Framework 6 (EF6) | Entity Framework Core (EF Core) |
| **Performance** | Good | High-performance (rebuilt from scratch) |
| **Status** | Maintenance mode | Actively developed |
| **This project** | No | **Yes — ASP.NET Core 8.0** |

---

### Entity Framework 6 (EF6) — The Legacy Version

**Entity Framework 6** is the older, mature ORM that was built specifically for the classic **ASP.NET** framework (Windows-only). It was the standard way to access databases in .NET applications from around 2008 until ASP.NET Core arrived in 2016.

#### Key Characteristics of EF6

- **Windows-only** — because classic ASP.NET itself only runs on Windows (via IIS), EF6 is also Windows-bound
- **Tied to .NET Framework** — EF6 targets `.NET Framework` (e.g., 4.5, 4.6, 4.8), not `.NET Core` or `.NET 5+`
- **Stable and mature** — EF6 has been in production for over a decade and is very well understood
- **Maintenance mode** — Microsoft still fixes critical bugs, but no new features are being added. All new development is happening in EF Core

#### Installing EF6 (Classic ASP.NET Project)

In a classic ASP.NET project, EF6 is added via NuGet:

```xml
<!-- packages.config (classic ASP.NET style) -->
<package id="EntityFramework" version="6.4.4" targetFramework="net48" />
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package EntityFramework
```

#### EF6 DbContext and DbSet

The concept of `DbContext` and `DbSet<T>` exists in both EF6 and EF Core and works almost identically:

```csharp
// EF6 — classic ASP.NET project
using System.Data.Entity;   // ← EF6 namespace (not Microsoft.EntityFrameworkCore)

public class SchoolDbContext : DbContext
{
    // Connection string name from Web.config
    public SchoolDbContext() : base("name=SchoolDb") { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
}
```

Compare this to EF Core in this project:

```csharp
// EF Core — ASP.NET Core project (this project)
using Microsoft.EntityFrameworkCore;   // ← EF Core namespace

public class ApplicationDbContext : DbContext
{
    // Options injected via Dependency Injection (no connection string in constructor)
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
}
```

The biggest structural difference is how the connection string is provided:
- **EF6** — reads from `Web.config` by name, passed directly in the constructor
- **EF Core** — receives `DbContextOptions` injected by the ASP.NET Core DI container

#### EF6 Configuration — Web.config vs appsettings.json

EF6 stores its connection string in `Web.config` (XML), the classic ASP.NET configuration file:

```xml
<!-- Web.config (EF6 / classic ASP.NET) -->
<connectionStrings>
  <add name="SchoolDb"
       connectionString="Server=.\SQLEXPRESS;Database=SchoolDB;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

EF Core (this project) stores it in `appsettings.json` (JSON):

```json
// appsettings.json (EF Core / ASP.NET Core)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentAssessmentDB;..."
  }
}
```

#### EF6 Fluent API — OnModelCreating

EF6 also supports Fluent API configuration via `OnModelCreating`, and the syntax is nearly identical to EF Core:

```csharp
// EF6 — OnModelCreating
protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
    modelBuilder.Entity<Student>()
        .HasKey(s => s.Id);

    modelBuilder.Entity<Student>()
        .Property(s => s.FirstName)
        .IsRequired()
        .HasMaxLength(50);

    modelBuilder.Entity<Student>()
        .HasIndex(s => s.Email)   // NOTE: HasIndex() was added late in EF6
        .IsUnique();
}
```

#### EF6 Migrations

EF6 has a migrations system too, but it is driven through the **Package Manager Console** inside Visual Studio, not the CLI:

```powershell
# EF6 — run inside Visual Studio Package Manager Console
Enable-Migrations                    # sets up the Migrations folder
Add-Migration InitialCreate          # creates a migration snapshot
Update-Database                      # applies the migration to the database
```

EF Core uses the cross-platform `dotnet ef` CLI instead:

```bash
# EF Core — run in any terminal (Windows, Linux, macOS)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### EF6 vs EF Core — Direct Comparison

| Feature | EF6 (Legacy) | EF Core (This Project) |
|---|---|---|
| **Namespace** | `System.Data.Entity` | `Microsoft.EntityFrameworkCore` |
| **Platform** | Windows only (.NET Framework) | Cross-platform (.NET 5 / 6 / 7 / 8) |
| **Configuration file** | `Web.config` (XML) | `appsettings.json` (JSON) |
| **DI integration** | Manual / not built-in | Native ASP.NET Core DI |
| **Migration CLI** | Package Manager Console (Visual Studio) | `dotnet ef` (any terminal) |
| **Performance** | Good | Better (optimized query pipeline) |
| **LINQ support** | Full | Full (improved translation) |
| **Bulk operations** | Slow (no built-in) | Better with EF Core extensions |
| **Raw SQL** | `Database.SqlQuery<T>()` | `FromSqlRaw()` / `ExecuteSqlRaw()` |
| **Shadow properties** | Not supported | Supported |
| **Owned entity types** | Not supported | Supported |
| **Status** | Maintenance only | Actively developed |

#### Why EF Core Was Created Instead of Updating EF6

EF6 was deeply tied to the `System.Data` infrastructure of the .NET Framework, making it impossible to port to the new cross-platform .NET Core runtime. Microsoft made the decision to **rewrite the ORM from scratch** as "Entity Framework Core" — lighter, faster, truly cross-platform, and built for modern dependency injection patterns. EF6 remains available for teams maintaining legacy Windows applications on .NET Framework, but all new projects should use EF Core.

---

**Dapper** is framework-independent — it works in both ASP.NET (with EF6) and ASP.NET Core (with EF Core), as well as console apps, desktop apps, or any .NET application, because it only requires a standard `IDbConnection`.

---

## 8. Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        HTTP Request                                  │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER — StudentsController / TeachersController        │
│  (Handles HTTP, validates input, returns JSON responses)             │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│  APPLICATION LAYER — StudentService                                  │
│  (Business logic, validation, DTO mapping)                           │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE LAYER — StudentRepository / Repository<T>            │
│                                                                      │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Entity Framework Core (EF Core 8.0)                          │  │
│  │                                                                │  │
│  │  ApplicationDbContext                                          │  │
│  │  ├─ DbSet<Student>   ─── maps to ──► Students table           │  │
│  │  └─ DbSet<Teacher>   ─── maps to ──► Teachers table           │  │
│  │                                                                │  │
│  │  Features used:                                                │  │
│  │  ✔ LINQ → SQL translation                                      │  │
│  │  ✔ Change Tracking                                             │  │
│  │  ✔ AsNoTracking() for read-only queries                        │  │
│  │  ✔ Fluent API schema configuration                             │  │
│  │  ✔ Migrations (CREATE TABLE, ALTER TABLE)                      │  │
│  │  ✔ Retry on failure (resilience)                               │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ─── Dapper could also be used here for complex queries ───          │
│                                                                      │
└──────────────────────────────┬──────────────────────────────────────┘
                               │  SQL (SELECT / INSERT / UPDATE / DELETE)
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│  SQL SERVER DATABASE                                                 │
│  ├─ Students table  (Id, StudentUniqueId, FirstName, ..., Grade)     │
│  └─ Teachers table  (Id, FirstName, Email, Subject, Password, ...)   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 9. Summary

| Concept | Key Takeaway |
|---|---|
| **ORM** | A library that bridges C# objects and SQL database tables, eliminating manual SQL writing and result mapping |
| **Entity Framework Core** | Microsoft's full ORM — handles SQL generation (LINQ), change tracking, schema creation via Migrations, and relationship management |
| **Dapper** | A micro-ORM from Stack Overflow — handles result mapping only; you write all SQL yourself; faster but requires more code |
| **This project** | Uses EF Core 8.0 with SQL Server; no Dapper |
| **Code-First** | The approach used here — C# classes define the schema; EF Core generates the database |
| **Migrations** | EF Core's version-control system for the database schema (`dotnet ef migrations add` / `dotnet ef database update`) |
| **DbContext** | The EF Core session object — one per HTTP request, manages connection + change tracking |
| **DbSet<T>** | Represents a single database table inside the DbContext |
| **Repository Pattern** | Wraps EF Core calls behind an interface, keeping data access logic out of the business logic layer |
| **LINQ** | The C# query language that EF Core translates to SQL automatically |
| **AsNoTracking()** | Disables change tracking for read-only queries — improves performance |
| **SQL injection** | Both EF Core and Dapper use parameterized queries automatically — safe from injection attacks |
