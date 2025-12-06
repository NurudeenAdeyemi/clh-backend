namespace CLHCRM.Domain.Common;

/// <summary>
/// Base class for all entities
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; }
}
