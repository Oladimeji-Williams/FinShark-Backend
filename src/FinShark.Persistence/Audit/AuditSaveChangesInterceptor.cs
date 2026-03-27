using FinShark.Application.Common;
using FinShark.Domain.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace FinShark.Persistence.Audit;

public sealed class AuditSaveChangesInterceptor(ICurrentUserService? currentUserService) : SaveChangesInterceptor
{
    private readonly ICurrentUserService? _currentUserService = currentUserService;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        var userId = _currentUserService?.UserId;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("Created").CurrentValue = now;
                entry.Property("CreatedBy").CurrentValue = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("Modified").CurrentValue = now;
                entry.Property("ModifiedBy").CurrentValue = userId;
            }
        }
    }
}
