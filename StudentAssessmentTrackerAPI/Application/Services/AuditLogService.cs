using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>Contract for writing and querying the immutable audit log.</summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Records a Create, Update, or Delete event.
        /// The call is fire-and-forget — failures are logged but never re-thrown.
        /// </summary>
        Task LogAsync(
            string entityName,
            int entityId,
            string action,
            string? oldValues,
            string? newValues,
            string? changedBy,
            string? changedByRole);

        /// <summary>Returns all audit log entries, newest first.</summary>
        Task<IEnumerable<AuditLogDto>> GetAllAsync(int page = 1, int pageSize = 50);

        /// <summary>Returns audit entries for a specific entity type and ID.</summary>
        Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId);
    }

    /// <summary>
    /// Writes and retrieves immutable audit records.
    /// Uses the DbContext directly (no generic repository) so audit writes bypass
    /// the normal repository layer and can never be accidentally suppressed.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuditLogService> _logger;

        /// <summary>Initialises the service.</summary>
        public AuditLogService(ApplicationDbContext db, ILogger<AuditLogService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task LogAsync(
            string entityName,
            int entityId,
            string action,
            string? oldValues,
            string? newValues,
            string? changedBy,
            string? changedByRole)
        {
            try
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    OldValues = oldValues,
                    NewValues = newValues,
                    ChangedBy = changedBy,
                    ChangedByRole = changedByRole,
                    ChangedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Audit failures must never block the primary operation
                _logger.LogError(ex, "Failed to write audit log for {Action} on {Entity}#{Id}",
                    action, entityName, entityId);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLogDto>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            return await _db.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.ChangedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId)
        {
            return await _db.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.ChangedAt)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        private static AuditLogDto MapToDto(AuditLog a) => new()
        {
            Id = a.Id,
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            Action = a.Action,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            ChangedBy = a.ChangedBy,
            ChangedByRole = a.ChangedByRole,
            ChangedAt = a.ChangedAt
        };
    }
}
