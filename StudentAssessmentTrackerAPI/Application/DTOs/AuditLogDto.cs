namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>Audit log record returned to admin users.</summary>
    public class AuditLogDto
    {
        /// <summary>Primary key of the audit entry.</summary>
        public int Id { get; set; }
        /// <summary>Entity type that was changed, e.g. "Student".</summary>
        public string EntityName { get; set; } = string.Empty;
        /// <summary>PK of the changed entity row.</summary>
        public int EntityId { get; set; }
        /// <summary>"Create", "Update", or "Delete".</summary>
        public string Action { get; set; } = string.Empty;
        /// <summary>JSON of the old state before the change (null for Create).</summary>
        public string? OldValues { get; set; }
        /// <summary>JSON of the new state after the change (null for Delete).</summary>
        public string? NewValues { get; set; }
        /// <summary>Identity of the user who made the change.</summary>
        public string? ChangedBy { get; set; }
        /// <summary>Role of the user who made the change.</summary>
        public string? ChangedByRole { get; set; }
        /// <summary>UTC timestamp of the change.</summary>
        public DateTime ChangedAt { get; set; }
    }
}
