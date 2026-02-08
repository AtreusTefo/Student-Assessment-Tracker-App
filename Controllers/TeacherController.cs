using Microsoft.AspNetCore.Mvc;
using StudentAssessmentTracker.Data;
using StudentAssessmentTracker.Models;
using FluentValidation;
using AutoMapper;

namespace StudentAssessmentTracker.Controllers;

[ApiController]
[Route("api/teachers")]
public class TeacherController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<Teacher> _validator;
    private readonly IMapper _mapper;

    public TeacherController(ApplicationDbContext context, IValidator<Teacher> validator, IMapper mapper)
    {
        _context = context;
        _validator = validator;
        _mapper = mapper;
    }

    // GET: api/teachers?sortOrder=fname
    [HttpGet]
    public IActionResult GetAll(string sortOrder = "")
    {
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

        return Ok(teachers.ToList());
    }

    // GET: api/teachers/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var teacher = _context.Teachers.Find(id);
        if (teacher == null)
        {
            return NotFound();
        }

        // AutoMapper converts Teacher to TeacherDetailDto (includes marks and calculations)
        var teacherDetail = _mapper.Map<TeacherDetailDto>(teacher);
        return Ok(teacherDetail);
    }

    // POST: api/teachers
    [HttpPost]
    public IActionResult Create([FromBody] Teacher teacher)
    {
        // FluentValidation is registered with AddFluentValidationAutoValidation(),
        // so validators run automatically during model binding and populate ModelState.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Automatically prepend country code to phone number
        // User inputs: 72254856 → We store: +267 72254856
        teacher.Phone = $"+267 {teacher.Phone}";

        // AutoMapper usage (mapping Teacher to Teacher here as an example)
        var mappedTeacher = _mapper.Map<Teacher>(teacher);

        _context.Teachers.Add(mappedTeacher);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = mappedTeacher.TeacherId }, mappedTeacher);
    }

    // PUT: api/teachers/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Teacher teacher)
    {
        if (teacher == null)
        {
            return BadRequest();
        }

        var existingTeacher = _context.Teachers.Find(id);
        if (existingTeacher == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
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
        return Ok(existingTeacher);
    }

    // DELETE: api/teachers/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var teacher = _context.Teachers.Find(id);
        if (teacher == null)
        {
            return NotFound();
        }

        _context.Teachers.Remove(teacher);
        _context.SaveChanges();
        return Ok();
    }

    // POST: api/teachers/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto credentials)
    {
        if (credentials == null || string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        // Find teacher by email
        var teacher = _context.Teachers.FirstOrDefault(t => t.Email == credentials.Email);
        if (teacher == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Simple password check (in production, use proper password hashing)
        if (teacher.Password != credentials.Password)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // For now, return a simple token (in production, use JWT)
        var token = $"Bearer_{teacher.TeacherId}_{DateTime.UtcNow.Ticks}";

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
