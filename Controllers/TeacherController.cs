using Microsoft.AspNetCore.Mvc;
using StudentAssessmentTracker.Data;
using StudentAssessmentTracker.Models;
using FluentValidation;
using AutoMapper;

namespace StudentAssessmentTracker.Controllers;

/// <summary>
/// DEPRECATED: This controller has been replaced by the new multi-layered architecture.
/// This file is kept for reference only and is not used by the application.
/// </summary>
[ApiController]
[Route("api/_legacy/teachers")]
public class TeacherControllerLegacy : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<Teacher> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherControllerLegacy> _logger;

    public TeacherControllerLegacy(ApplicationDbContext context, IValidator<Teacher> validator, IMapper mapper, ILogger<TeacherControllerLegacy> logger)
    {
        _context = context;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: api/teachers?sortOrder=fname
    [HttpGet]
    public IActionResult GetAll(string sortOrder = "")
    {
        _logger.LogInformation("Fetching all teachers with sort order: {SortOrder}", sortOrder);

        var teachers = _context.Teachers.AsQueryable();

        switch (sortOrder)
        {
            case "fname":
                teachers = teachers.OrderBy(t => t.FirstName);
                break;
            case "lname":
                teachers = teachers.OrderBy(t => t.LastName);
                break;
            case "email":
                teachers = teachers.OrderBy(t => t.Email);
                break;
            case "subject":
                teachers = teachers.OrderBy(t => t.Subject);
                break;
        }

        var teacherList = teachers.ToList();
        _logger.LogInformation("Successfully retrieved {TeacherCount} teachers", teacherList.Count);
        return Ok(teacherList);
    }

    // GET: api/teachers/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        _logger.LogInformation("Fetching teacher with ID: {TeacherId}", id);

        var teacher = _context.Teachers.Find(id);
        if (teacher == null)
        {
            _logger.LogWarning("Teacher not found with ID: {TeacherId}", id);
            return NotFound();
        }

        // AutoMapper converts Teacher to TeacherDetailDto (includes marks and calculations)
        var teacherDetail = _mapper.Map<TeacherDetailDto>(teacher);
        _logger.LogInformation("Successfully retrieved teacher: {TeacherName}", $"{teacher.FirstName} {teacher.LastName}");
        return Ok(teacherDetail);
    }

    // POST: api/teachers
    [HttpPost]
    public IActionResult Create([FromBody] Teacher teacher)
    {
        var teacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "unknown";
        _logger.LogInformation("Creating new teacher: {TeacherName}", teacherName);

        // FluentValidation is registered with AddFluentValidationAutoValidation(),
        // so validators run automatically during model binding and populate ModelState.
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Teacher creation failed - validation errors: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        // Automatically prepend country code to phone number
        // User inputs: 72254856 → We store: +267 72254856
        if (teacher != null)
        {
            teacher.Phone = $"+267 {teacher.Phone}";
        }

        // AutoMapper usage (mapping Teacher to Teacher here as an example)
        var mappedTeacher = _mapper.Map<Teacher>(teacher);

        _context.Teachers.Add(mappedTeacher);
        _context.SaveChanges();

        _logger.LogInformation("Teacher created successfully with ID: {TeacherId}", mappedTeacher.TeacherId);
        return CreatedAtAction(nameof(GetById), new { id = mappedTeacher.TeacherId }, mappedTeacher);
    }

    // PUT: api/teachers/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Teacher teacher)
    {
        _logger.LogInformation("Updating teacher with ID: {TeacherId}", id);

        if (teacher == null)
        {
            _logger.LogWarning("Update failed - teacher object is null for ID: {TeacherId}", id);
            return BadRequest();
        }

        var existingTeacher = _context.Teachers.Find(id);
        if (existingTeacher == null)
        {
            _logger.LogWarning("Update failed - teacher not found with ID: {TeacherId}", id);
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Update validation failed for teacher ID: {TeacherId}", id);
            return BadRequest(ModelState);
        }

        // Automatically prepend country code to phone number if not already there
        if (!teacher.Phone.StartsWith("+267"))
        {
            teacher.Phone = $"+267 {teacher.Phone}";
        }

        existingTeacher.FirstName = teacher.FirstName;
        existingTeacher.LastName = teacher.LastName;
        existingTeacher.Email = teacher.Email;
        existingTeacher.Phone = teacher.Phone;
        existingTeacher.Subject = teacher.Subject;

        _context.Teachers.Update(existingTeacher);
        _context.SaveChanges();

        _logger.LogInformation("Teacher updated successfully: ID={TeacherId}, Name={TeacherName}", id, $"{existingTeacher.FirstName} {existingTeacher.LastName}");
        return Ok(existingTeacher);
    }

    // DELETE: api/teachers/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("Deleting teacher with ID: {TeacherId}", id);

        var teacher = _context.Teachers.Find(id);
        if (teacher == null)
        {
            _logger.LogWarning("Delete failed - teacher not found with ID: {TeacherId}", id);
            return NotFound();
        }

        _context.Teachers.Remove(teacher);
        _context.SaveChanges();

        _logger.LogInformation("Teacher deleted successfully: ID={TeacherId}, Name={TeacherName}", id, $"{teacher.FirstName} {teacher.LastName}");
        return Ok();
    }

    // POST: api/teachers/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto credentials)
    {
        _logger.LogInformation("Login attempt for email: {Email}", credentials?.Email ?? "unknown");

        if (credentials == null || string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
        {
            _logger.LogWarning("Login failed - missing email or password");
            return BadRequest(new { message = "Email and password are required" });
        }

        // Find teacher by email
        var teacher = _context.Teachers.FirstOrDefault(t => t.Email == credentials.Email);
        if (teacher == null)
        {
            _logger.LogWarning("Login failed - teacher not found with email: {Email}", credentials.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Simple password check (in production, use proper password hashing)
        if (teacher.Password != credentials.Password)
        {
            _logger.LogWarning("Login failed - invalid password for email: {Email}", credentials.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // For now, return a simple token (in production, use JWT)
        var token = $"Bearer_{teacher.TeacherId}_{DateTime.UtcNow.Ticks}";
        _logger.LogInformation("Login successful for teacher: {TeacherId}", teacher.TeacherId);

        return Ok(new
        {
            token = token,
            teacher = new
            {
                teacher.TeacherId,
                teacher.FirstName,
                teacher.LastName,
                teacher.Email,
                teacher.Phone,
                teacher.Subject,
                teacher.EnrollmentDate,
                teacher.CreatedDate
            }
        });
    }
}
