namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Immutable audit record written whenever a Create, Update, or Delete operation
    /// is performed on any primary entity (Student, Teacher, Assessment, Submission).
    /// Rows are never updated or deleted — the audit trail must remain permanent.
    /// </summary>
    public class AuditLog
    {
        /// <summary>Auto-incremented primary key.</summary>
        public int Id { get; set; }

        /// <summary>Name of the affected entity type, e.g. "Student", "Teacher", "StudentAssessment".</summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>Primary key of the affected entity row.</summary>
        public int EntityId { get; set; }

        /// <summary>Operation performed: "Create", "Update", or "Delete".</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>JSON snapshot of the entity state before the change (null for Create).</summary>
        public string? OldValues { get; set; }

        /// <summary>JSON snapshot of the entity state after the change (null for Delete).</summary>
        public string? NewValues { get; set; }

        /// <summary>Identifier of the user who triggered the change (TeacherId, StudentId, or AdminId as a string).</summary>
        public string? ChangedBy { get; set; }

        /// <summary>Role of the user who triggered the change: "Teacher", "Student", or "Admin".</summary>
        public string? ChangedByRole { get; set; }

        /// <summary>UTC timestamp when the change was recorded.</summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
