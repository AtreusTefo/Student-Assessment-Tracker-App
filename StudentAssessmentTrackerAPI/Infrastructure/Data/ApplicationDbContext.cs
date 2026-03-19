using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext — manages all tables and relationships.
    /// Improvements applied:
    ///   - Grades lookup table (seeded Grade 7–12) replaces free-text Grade column
    ///   - StudentAssessments table replaces hardcoded Assessment1/2/3 columns
    ///   - TeacherId FK on Students for referential integrity
    ///   - IdPassportNo unique index prevents duplicate student registrations
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Grade> Grades { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentAssessment> StudentAssessments { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Grades lookup table (read-only seed data) ─────────────────────
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Level).IsUnique();
            });

            // Seed fixed grade levels — teachers cannot create new ones
            modelBuilder.Entity<Grade>().HasData(
                new Grade { Id = 1, Name = "Grade 7", Level = 7 },
                new Grade { Id = 2, Name = "Grade 8", Level = 8 },
                new Grade { Id = 3, Name = "Grade 9", Level = 9 },
                new Grade { Id = 4, Name = "Grade 10", Level = 10 },
                new Grade { Id = 5, Name = "Grade 11", Level = 11 },
                new Grade { Id = 6, Name = "Grade 12", Level = 12 }
            );

            // ── Students ──────────────────────────────────────────────────────
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.StudentUniqueId).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.StudentUniqueId).IsUnique();

                entity.Property(e => e.IdPassportNo).IsRequired().HasMaxLength(20);
                // Unique constraint — prevents the same person being registered twice
                entity.HasIndex(e => e.IdPassportNo).IsUnique();

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);

                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Phone).IsRequired().HasMaxLength(8);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // FK → Grades (RESTRICT: cannot delete a grade that has students)
                entity.HasOne(e => e.GradeNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.GradeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // FK → Teachers (RESTRICT: cannot delete a teacher who still has students)
                entity.HasOne(e => e.Teacher)
                    .WithMany(t => t.Students)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── StudentAssessments ────────────────────────────────────────────
            modelBuilder.Entity<StudentAssessment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MaxScore).HasColumnType("decimal(8,2)");
                entity.Property(e => e.Score).HasColumnType("decimal(8,2)");
                entity.Property(e => e.DueDate).IsRequired(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // FK → Students (CASCADE: deleting a student removes all their assessments)
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Assessments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Teachers ──────────────────────────────────────────────────────
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(8);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
