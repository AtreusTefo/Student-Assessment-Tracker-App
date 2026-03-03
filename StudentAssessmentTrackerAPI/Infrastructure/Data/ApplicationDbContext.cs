using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext
    /// Manages the SQL Server database and entity mappings
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
        /// Configures model builder with SQL Server schema and constraints
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
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(8);

                entity.Property(e => e.Grade)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Assessment1)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.Assessment2)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.Assessment3)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
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
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(8);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.EnrollmentDate)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
