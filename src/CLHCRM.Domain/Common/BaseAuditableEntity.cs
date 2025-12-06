namespace CLHCRM.Domain.Common;

/// <summary>
/// Base class for auditable entities with soft delete support
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    protected BaseAuditableEntity() : base()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseAuditableEntity(Guid id) : base(id)
    {
        CreatedAt = DateTime.UtcNow;
    }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
