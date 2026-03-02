using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext
    /// Manages the in-memory database and entity mappings
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext
        /// </summary>
        /// <param name="options">DbContext options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// DbSet for Student entities
        /// </summary>
        public DbSet<Student> Students { get; set; }

        /// <summary>
        /// DbSet for Teacher entities
        /// </summary>
        public DbSet<Teacher> Teachers { get; set; }

        /// <summary>
        /// Configures model builder with schema and constraints
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired();

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(8);

                entity.Property(e => e.Grade)
                    .IsRequired();

                entity.Property(e => e.Assessment1)
                    .HasPrecision(5, 2);

                entity.Property(e => e.Assessment2)
                    .HasPrecision(5, 2);

                entity.Property(e => e.Assessment3)
                    .HasPrecision(5, 2);

                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired();

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(8);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password)
                    .IsRequired();

                entity.Property(e => e.EnrollmentDate);
                entity.Property(e => e.CreatedDate);
            });
        }
    }
}
