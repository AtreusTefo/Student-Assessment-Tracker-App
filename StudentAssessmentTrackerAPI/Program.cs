using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

// Import all architectural layers
using StudentAssessmentTracker.Infrastructure.Data;
using StudentAssessmentTracker.Infrastructure.Repositories;
using StudentAssessmentTracker.Domain.Interfaces;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Application.Validators;
using StudentAssessmentTracker.Application.Mappings;
using StudentAssessmentTracker.Application.Services;
using StudentAssessmentTracker.Application.DTOs;

var contentRoot = Directory.GetCurrentDirectory();
var logsDirectory = Path.Combine(contentRoot, "Logs");
Directory.CreateDirectory(logsDirectory);

// Configure console output for immediate display
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Out.Flush();

// Navigate to parent directory to find StudentApp (now at same level as API)
var parentDirectory = Directory.GetParent(contentRoot)?.FullName ?? contentRoot;
var angularDistPath = Path.Combine(parentDirectory, "StudentApp", "dist", "StudentApp", "browser");
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = Directory.Exists(angularDistPath) ? angularDistPath : "wwwroot"
});

// ============================================================================
// SERILOG CONFIGURATION
// ============================================================================
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// ============================================================================
// PRESENTATION LAYER - API Controllers
// ============================================================================
builder.Services.AddControllers();

// ── Multipart / file-upload request body limit (10 MB) ─────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

// ============================================================================
// SWAGGER/OPENAPI DOCUMENTATION
// ============================================================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Student Assessment Tracker API",
        Version = "v1",
        Description = """
            REST API for the Student Assessment Tracking System.

            ## Authentication
            Most endpoints require a JWT Bearer token. Obtain one from one of the login endpoints below,
            then click the **Authorize** button (🔒) at the top of this page and paste the token — no `Bearer` prefix needed.

            | Role | Login endpoint |
            |------|---------------|
            | **Teacher** | `POST /api/teachers/login` |
            | **Admin** | `POST /api/admins/login` |
            | **Student** | `POST /api/students/login` (activate first via `POST /api/students/activate`) |

            Public endpoints (no token required): `GET /api/grades`, `GET /api/subjects`,
            `POST /api/teachers` (register), `POST /api/teachers/login`, `POST /api/admins/login`,
            `POST /api/students/activate`, `POST /api/students/login`.
            """,
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Development Team"
        }
    });

    // Include XML documentation
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "StudentAssessmentTracker.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }

    // Bearer token support in Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = """
            Enter the JWT token obtained from one of the login endpoints.
            Do NOT include the 'Bearer' prefix — Swagger adds it automatically.

            • Teacher token  → POST /api/teachers/login
            • Admin token    → POST /api/admins/login
            • Student token  → POST /api/students/login
            """
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================================
// CORS CONFIGURATION
// ============================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================================
// INFRASTRUCTURE LAYER - Database (SQL Server LocalDB)
// ============================================================================

// ── JWT Authentication ──────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured. Add it to appsettings.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// ============================================================================
// INFRASTRUCTURE LAYER - Repositories (Data Access)
// ============================================================================
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IStudentAssessmentRepository, StudentAssessmentRepository>();
builder.Services.AddScoped<IAssessmentSubmissionRepository, AssessmentSubmissionRepository>();
// Resolve generic IRepository<T> requests via the specialized implementations
builder.Services.AddScoped<IRepository<Student>>(sp => sp.GetRequiredService<IStudentRepository>());
builder.Services.AddScoped<IRepository<Teacher>>(sp => sp.GetRequiredService<ITeacherRepository>());

// ============================================================================
// APPLICATION LAYER - Business Logic
// ============================================================================
// Register Service layer
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IStudentAssessmentService, StudentAssessmentService>();
builder.Services.AddScoped<IAssessmentSubmissionService, AssessmentSubmissionService>();

// ── New feature services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IClassGroupService, ClassGroupService>();

// Register Validation
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<CreateStudentValidator>();

// Register AutoMapper for DTO mapping
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ============================================================================
// BUILD APPLICATION
// ============================================================================
var app = builder.Build();

// ============================================================================
// AUTO-MIGRATE DATABASE ON STARTUP
// Creates the database if it doesn't exist, applies any pending migrations
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var dbLogger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
    try
    {
        dbLogger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        dbLogger.LogInformation("Database ready: {Database}", db.Database.GetConnectionString());
    }
    catch (Exception ex)
    {
        dbLogger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// ============================================================================
// ENSURE UPLOADS DIRECTORY EXISTS (DO NOT serve via static files)
// ============================================================================
var uploadsRoot = Path.Combine(app.Environment.WebRootPath, "uploads", "submissions");
Directory.CreateDirectory(uploadsRoot);

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================
app.UseSerilogRequestLogging();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

// ============================================================================
// SWAGGER UI - API DOCUMENTATION
// ============================================================================
app.UseSwagger(options =>
{
    options.RouteTemplate = "swagger/{documentName}/swagger.json";
});

// Configure Swagger UI at /swagger endpoint
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Assessment Tracker API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Student Assessment Tracker API";
    options.DisplayRequestDuration();
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DefaultModelsExpandDepth(1);
    options.EnableDeepLinking();
    options.ShowExtensions();
});

app.MapControllers();
app.MapFallbackToFile("index.html");

// ============================================================================
// STARTUP LOGGING
// ============================================================================
Log.Information("╔═══════════════════════════════════════════════════════════════════════════════╗");
Log.Information("║         Student Assessment Tracker - Multi-Layered Architecture              ║");
Log.Information("║                                                                               ║");
Log.Information("║   Running on: http://localhost:5000                                       ║");
Log.Information("║   API Base: http://localhost:5000/api                                     ║");
Log.Information("║   Swagger UI: http://localhost:5000/swagger                                ║");
Log.Information("║   Architecture: Domain → Infrastructure → Application → Presentation   ║");
Log.Information("║                                                                               ║");
Log.Information("║   Dependency Injection: Configured                                        ║");
Log.Information("║   FluentValidation: Active                                                ║");
Log.Information("║   AutoMapper: Configured                                                  ║");
Log.Information("║   CORS: Enabled for Angular frontend                                      ║");
Log.Information("║   Serilog: Logging active                                                 ║");
Log.Information("║   Swagger UI: http://localhost:5000/swagger                                ║");
Log.Information("║                                                                               ║");
Log.Information("╚═══════════════════════════════════════════════════════════════════════════════╝");

app.Run();
