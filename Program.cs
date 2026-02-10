using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using StudentAssessmentTracker.Validators;
using StudentAssessmentTracker.Mappings;
using Serilog;

var contentRoot = Directory.GetCurrentDirectory();
var logsDirectory = Path.Combine(contentRoot, "Logs");
Directory.CreateDirectory(logsDirectory);

// Configure console output for immediate display
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Out.Flush();

var angularDistPath = Path.Combine(contentRoot, "StudentApp", "dist", "StudentApp", "browser");
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = Directory.Exists(angularDistPath) ? angularDistPath : "wwwroot"
});

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("StudentDb"));

// Register FluentValidation
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<StudentValidator>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngular");

app.MapControllers();
app.MapFallbackToFile("index.html");

// Log startup information
Log.Information("╔════════════════════════════════════════════════════════════╗");
Log.Information("║   Student Assessment Tracker - Application Started        ║");
Log.Information("║   🚀 Running on: http://localhost:5000                    ║");
Log.Information("║   📊 API Base: http://localhost:5000/api                  ║");
Log.Information("║   ✨ Autocomplete enabled on all forms                    ║");
Log.Information("╚════════════════════════════════════════════════════════════╝");

app.Run();
