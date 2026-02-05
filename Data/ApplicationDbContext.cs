
using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Models;

namespace StudentAssessmentTracker.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Student> Students { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed initial student data for testing
        modelBuilder.Entity<Student>().HasData(
            new Student
            {
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "+267 72254856",
                Grade = "10A",
                EnrollmentDate = new DateTime(2023, 9, 1),
                Assessment1 = 18,
                Assessment2 = 19,
                Assessment3 = 17,
                CreatedDate = DateTime.Now
            },
            new Student
            {
                StudentId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Phone = "+267 71234567",
                Grade = "11B",
                EnrollmentDate = new DateTime(2023, 9, 1),
                Assessment1 = 15,
                Assessment2 = 16,
                Assessment3 = 14,
                CreatedDate = DateTime.Now
            },
            new Student
            {
                StudentId = 3,
                FirstName = "David",
                LastName = "Johnson",
                Email = "david.johnson@example.com",
                Phone = "+267 73456789",
                Grade = "12C",
                EnrollmentDate = new DateTime(2023, 9, 1),
                Assessment1 = 12,
                Assessment2 = 13,
                Assessment3 = 11,
                CreatedDate = DateTime.Now
            }
        );
    }
}
