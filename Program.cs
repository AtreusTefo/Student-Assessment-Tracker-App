using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using StudentAssessmentTracker.Validators;
using StudentAssessmentTracker.Mappings;

var contentRoot = Directory.GetCurrentDirectory();
var angularDistPath = Path.Combine(contentRoot, "StudentApp", "dist", "StudentApp", "browser");
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = Directory.Exists(angularDistPath) ? angularDistPath : "wwwroot"
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

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngular");

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
