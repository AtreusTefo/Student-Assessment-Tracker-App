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
    /// Uses <see cref="IDbContextFactory{TContext}"/> so each write creates an independent
    /// DbContext that commits outside any ambient transaction on the caller's context.
    /// This ensures audit entries survive even when the caller's transaction is rolled back.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuditLogService> _logger;

        /// <summary>Initialises the service.</summary>
        public AuditLogService(
            IDbContextFactory<ApplicationDbContext> factory,
            ApplicationDbContext db,
            ILogger<AuditLogService> logger)
        {
            _factory = factory;
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
                // Use a fresh, independent DbContext so the audit write is committed
                // outside any ambient transaction the caller may have open.  This guarantees
                // audit records are never silently lost when a caller transaction rolls back.
                await using var auditDb = await _factory.CreateDbContextAsync();
                auditDb.AuditLogs.Add(new AuditLog
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
                await auditDb.SaveChangesAsync();
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
