using Microsoft.EntityFrameworkCore;
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

// ============================================================================
// SWAGGER/OPENAPI DOCUMENTATION
// ============================================================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Student Assessment Tracker API",
        Version = "v1",
        Description = "REST API for Student Assessment Tracking System",
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
// INFRASTRUCTURE LAYER - Database
// ============================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("StudentDb"));

// ============================================================================
// INFRASTRUCTURE LAYER - Repositories (Data Access)
// ============================================================================
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRepository<Student>, StudentRepository>();
builder.Services.AddScoped<IRepository<Teacher>, Repository<Teacher>>();

// ============================================================================
// APPLICATION LAYER - Business Logic
// ============================================================================
// Register Service layer
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();

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
// MIDDLEWARE PIPELINE
// ============================================================================
app.UseSerilogRequestLogging();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

// ============================================================================
// SWAGGER AND SCALAR API DOCUMENTATION
// ============================================================================
app.UseSwagger(options =>
{
    options.RouteTemplate = "swagger/{documentName}/swagger.json";
});

// Map Swagger UI as main documentation interface
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Assessment Tracker API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("AllowAngular");
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

// ============================================================================
// STARTUP LOGGING
// ============================================================================
Log.Information("╔═══════════════════════════════════════════════════════════════════════════════╗");
Log.Information("║         Student Assessment Tracker - Multi-Layered Architecture              ║");
Log.Information("║                                                                               ║");
Log.Information("║   🚀 Running on: http://localhost:5000                                       ║");
Log.Information("║   � API Base: http://localhost:5000/api                                     ║");
Log.Information("║   📚 Swagger UI: http://localhost:5000/swagger                                ║");
Log.Information("║   🏗️  Architecture: Domain → Infrastructure → Application → Presentation   ║");
Log.Information("║                                                                               ║");
Log.Information("║   ✅ Dependency Injection: Configured                                        ║");
Log.Information("║   ✅ FluentValidation: Active                                                ║");
Log.Information("║   ✅ AutoMapper: Configured                                                  ║");
Log.Information("║   ✅ CORS: Enabled for Angular frontend                                      ║");
Log.Information("║   ✅ Serilog: Logging active                                                 ║");
Log.Information("║   ✅ Swagger UI: Configured at /swagger                                      ║");
Log.Information("║                                                                               ║");
Log.Information("╚═══════════════════════════════════════════════════════════════════════════════╝");

app.Run();
