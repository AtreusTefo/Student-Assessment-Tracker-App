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

    public StudentsController(ApplicationDbContext context, IValidator<Student> validator, IMapper mapper)
    {
        _context = context;
        _validator = validator;
        _mapper = mapper;
    }

    // GET: api/students?sortOrder=fname
    [HttpGet]
    public IActionResult GetAll(string sortOrder = "")
    {
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
        var studentDtos = _mapper.Map<List<StudentListDto>>(students.ToList());
        return Ok(studentDtos);
    }

    // GET: api/students/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var student = _context.Students.Find(id);
        if (student == null)
        {
            return NotFound();
        }

        // AutoMapper converts Student to StudentDetailDto (includes marks and calculations)
        var studentDetail = _mapper.Map<StudentDetailDto>(student);
        return Ok(studentDetail);
    }

    // POST: api/students
    [HttpPost]
    public IActionResult Create([FromBody] Student student)
    {
        // FluentValidation is registered with AddFluentValidationAutoValidation(),
        // so validators run automatically during model binding and populate ModelState.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Automatically prepend country code to phone number
        // User inputs: 72254856 → We store: +267 72254856
        student.Phone = $"+267 {student.Phone}";

        // AutoMapper usage (mapping Student to Student here as an example)
        var mappedStudent = _mapper.Map<Student>(student);

        _context.Students.Add(mappedStudent);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = mappedStudent.StudentId }, mappedStudent);
    }

    // PUT: api/students/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Student student)
    {
        if (student == null)
        {
            return BadRequest();
        }

        var existingStudent = _context.Students.Find(id);
        if (existingStudent == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
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
        return Ok(existingStudent);
    }

    // DELETE: api/students/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var student = _context.Students.Find(id);
        if (student == null)
        {
            return NotFound();
        }

        _context.Students.Remove(student);
        _context.SaveChanges();
        return Ok();
    }
}
