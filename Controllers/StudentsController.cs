using Microsoft.AspNetCore.Mvc;
using StudentAssessmentTracker.Data;
using StudentAssessmentTracker.Models;
using FluentValidation;
using AutoMapper;

namespace StudentAssessmentTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<Student> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(ApplicationDbContext context, IValidator<Student> validator, IMapper mapper, ILogger<StudentsController> logger)
    {
        _context = context;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: api/students?sortOrder=fname
    [HttpGet]
    public IActionResult GetAll(string sortOrder = "")
    {
        _logger.LogInformation("Fetching all students with sort order: {SortOrder}", sortOrder);

        var students = _context.Students.AsQueryable();

        switch (sortOrder)
        {
            case "fname":
                students = students.OrderBy(s => s.FirstName);
                break;
            case "lname":
                students = students.OrderBy(s => s.LastName);
                break;
            case "total":
                students = students.OrderByDescending(s => s.Total);
                break;
            case "percent":
                students = students.OrderByDescending(s => s.Percentage);
                break;
        }

        // AutoMapper converts Student list to StudentListDto list
        // This filters and transforms the data to return only essential fields
        var studentList = students.ToList();
        var studentDtos = _mapper.Map<List<StudentListDto>>(studentList);

        _logger.LogInformation("Successfully retrieved {StudentCount} students", studentDtos.Count);
        return Ok(studentDtos);
    }

    // GET: api/students/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        _logger.LogInformation("Fetching student with ID: {StudentId}", id);

        var student = _context.Students.Find(id);
        if (student == null)
        {
            _logger.LogWarning("Student not found with ID: {StudentId}", id);
            return NotFound();
        }

        // AutoMapper converts Student to StudentDetailDto (includes marks and calculations)
        var studentDetail = _mapper.Map<StudentDetailDto>(student);
        _logger.LogInformation("Successfully retrieved student: {StudentName}", $"{student.FirstName} {student.LastName}");
        return Ok(studentDetail);
    }

    // POST: api/students
    [HttpPost]
    public IActionResult Create([FromBody] Student student)
    {
        var studentName = student != null ? $"{student.FirstName} {student.LastName}" : "unknown";
        _logger.LogInformation("Creating new student: {StudentName}", studentName);

        // FluentValidation is registered with AddFluentValidationAutoValidation(),
        // so validators run automatically during model binding and populate ModelState.
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Student creation failed - validation errors: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        // Automatically prepend country code to phone number
        // User inputs: 72254856 → We store: +267 72254856
        if (student != null)
        {
            student.Phone = $"+267 {student.Phone}";
        }

        // AutoMapper usage (mapping Student to Student here as an example)
        var mappedStudent = _mapper.Map<Student>(student);

        _context.Students.Add(mappedStudent);
        _context.SaveChanges();

        _logger.LogInformation("Student created successfully with ID: {StudentId}", mappedStudent.StudentId);
        return CreatedAtAction(nameof(GetById), new { id = mappedStudent.StudentId }, mappedStudent);
    }

    // PUT: api/students/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Student student)
    {
        _logger.LogInformation("Updating student with ID: {StudentId}", id);

        if (student == null)
        {
            _logger.LogWarning("Update failed - student object is null for ID: {StudentId}", id);
            return BadRequest();
        }

        var existingStudent = _context.Students.Find(id);
        if (existingStudent == null)
        {
            _logger.LogWarning("Update failed - student not found with ID: {StudentId}", id);
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Update validation failed for student ID: {StudentId}", id);
            return BadRequest(ModelState);
        }

        // Automatically prepend country code to phone number if not already there
        if (!student.Phone.StartsWith("+267"))
        {
            student.Phone = $"+267 {student.Phone}";
        }

        existingStudent.FirstName = student.FirstName;
        existingStudent.LastName = student.LastName;
        existingStudent.Email = student.Email;
        existingStudent.Phone = student.Phone;
        existingStudent.Grade = student.Grade;
        existingStudent.Assessment1 = student.Assessment1;
        existingStudent.Assessment2 = student.Assessment2;
        existingStudent.Assessment3 = student.Assessment3;

        _context.Students.Update(existingStudent);
        _context.SaveChanges();

        _logger.LogInformation("Student updated successfully: ID={StudentId}, Name={StudentName}", id, $"{existingStudent.FirstName} {existingStudent.LastName}");
        return Ok(existingStudent);
    }

    // DELETE: api/students/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("Deleting student with ID: {StudentId}", id);

        var student = _context.Students.Find(id);
        if (student == null)
        {
            _logger.LogWarning("Delete failed - student not found with ID: {StudentId}", id);
            return NotFound();
        }

        _context.Students.Remove(student);
        _context.SaveChanges();

        _logger.LogInformation("Student deleted successfully: ID={StudentId}, Name={StudentName}", id, $"{student.FirstName} {student.LastName}");
        return Ok();
    }
}
