using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext — manages all tables and relationships.
    /// Improvements applied:
    ///   - Grades lookup table (seeded Grade 7–12) replaces free-text Grade column
    ///   - StudentAssessments table replaces hardcoded Assessment1/2/3 columns
    ///   - TeacherStudents join table replaces the old single TeacherId FK, enabling
    ///     many-to-many Teacher↔Student assignments (one student, multiple subject teachers)
    ///   - IdPassportNo unique index prevents duplicate student/teacher registrations
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>Initialises the DbContext with the provided EF Core options.</summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>DbSet for the Grades lookup table.</summary>
        public DbSet<Grade> Grades { get; set; }
        /// <summary>DbSet for the Subjects lookup table.</summary>
        public DbSet<Subject> Subjects { get; set; }
        /// <summary>DbSet for student records.</summary>
        public DbSet<Student> Students { get; set; }
        /// <summary>DbSet for student assessment records.</summary>
        public DbSet<StudentAssessment> StudentAssessments { get; set; }
        /// <summary>DbSet for student-uploaded submission files.</summary>
        public DbSet<AssessmentSubmission> AssessmentSubmissions { get; set; }
        /// <summary>DbSet for teacher records.</summary>
        public DbSet<Teacher> Teachers { get; set; }
        /// <summary>DbSet for the Teacher↔Student many-to-many join table.</summary>
        public DbSet<TeacherStudent> TeacherStudents { get; set; }

        /// <summary>Configures entity relationships, indexes, constraints, and seed data.</summary>
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

            // ── Subjects lookup table (read-only seed data) ─────────────────
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Seed fixed subjects — teachers select from this list, cannot create new ones
            modelBuilder.Entity<Subject>().HasData(
                new Subject { Id = 1, Name = "Accounting" },
                new Subject { Id = 2, Name = "Art" },
                new Subject { Id = 3, Name = "Business Studies" },
                new Subject { Id = 4, Name = "English" },
                new Subject { Id = 5, Name = "Geography" },
                new Subject { Id = 6, Name = "History" },
                new Subject { Id = 7, Name = "ICT" },
                new Subject { Id = 8, Name = "Mathematics" },
                new Subject { Id = 9, Name = "Multimedia" },
                new Subject { Id = 10, Name = "Music" },
                new Subject { Id = 11, Name = "Physical Education" },
                new Subject { Id = 12, Name = "Science" }
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

                entity.Property(e => e.Password).IsRequired(false).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // FK → Grades (RESTRICT: cannot delete a grade that has students)
                entity.HasOne(e => e.GradeNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.GradeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── TeacherStudents (many-to-many join table) ─────────────────────────────────
            modelBuilder.Entity<TeacherStudent>(entity =>
            {
                // Composite PK ensures at most one row per (teacher, student) pair
                entity.HasKey(e => new { e.TeacherId, e.StudentId });

                entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETUTCDATE()");

                // Issue 1 fix: unique index on (StudentId, SubjectId) enforces at the DB level
                // that a student cannot have two teachers for the same subject simultaneously.
                entity.HasIndex(e => new { e.StudentId, e.SubjectId })
                    .IsUnique()
                    .HasDatabaseName("UX_TeacherStudents_StudentId_SubjectId");

                // FK → Teachers (RESTRICT: prevents deleting a teacher who still has student assignments)
                // The service-layer guard in DeleteTeacherAsync already blocks this via HasStudentsAsync,
                // but aligning the DB behavior closes the bypass path from raw SQL / EF direct removes.
                entity.HasOne(e => e.Teacher)
                    .WithMany(t => t.StudentAssignments)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                // FK → Students (CASCADE: deleting a student removes their teacher assignments)
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.TeacherAssignments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // FK → Subjects (RESTRICT: subject rows are lookup data — must not cascade)
                entity.HasOne(e => e.Subject)
                    .WithMany()
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── StudentAssessments ────────────────────────────────────────────
            modelBuilder.Entity<StudentAssessment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MaxScore).HasColumnType("decimal(8,2)");
                entity.Property(e => e.Score).HasColumnType("decimal(8,2)");

                // DB-level guards that enforce the same rules as FluentValidation —
                // protects against direct SQL writes, seeding scripts, and admin tools.
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_StudentAssessments_MaxScore_Positive",
                        "[MaxScore] > 0");
                    t.HasCheckConstraint("CK_StudentAssessments_Score_NonNegative",
                        "[Score] >= 0");
                    t.HasCheckConstraint("CK_StudentAssessments_Score_LteMaxScore",
                        "[Score] <= [MaxScore]");
                });

                entity.Property(e => e.DueDate).IsRequired(false);
                entity.Property(e => e.IsAssigned).HasDefaultValue(false);
                entity.Property(e => e.Instructions).IsRequired(false).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // FK → Students (CASCADE: deleting a student removes all their assessments)
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Assessments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── AssessmentSubmissions ─────────────────────────────────────────
            modelBuilder.Entity<AssessmentSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(260);
                entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(260);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");

                // FK → StudentAssessments (CASCADE: deleting an assessment removes all its submissions)
                // StudentId is intentionally NOT stored as a separate FK column — it is derived from
                // StudentAssessment.StudentId via the navigation property.  This eliminates the
                // silent inconsistency risk where AssessmentSubmission.StudentId could diverge from
                // StudentAssessment.StudentId (BUG #5 fix).
                entity.HasOne(e => e.StudentAssessment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(e => e.StudentAssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Teachers ──────────────────────────────────────────────────────
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IdPassportNo).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.IdPassportNo).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(8);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                // FK → Subjects (RESTRICT: cannot delete a subject that has teachers)
                entity.HasOne(e => e.SubjectNavigation)
                    .WithMany(s => s.Teachers)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
