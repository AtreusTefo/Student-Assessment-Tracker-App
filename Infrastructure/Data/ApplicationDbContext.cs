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
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }

        /// <summary>
        /// Configures model builder with schema and constraints
        /// </summary>
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
        }
    }
}
