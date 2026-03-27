namespace FinShark.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; init; }
    public DateTime Created { get; private set; } = DateTime.UtcNow;
    public DateTime? Modified { get; private set; }

    public bool IsDeleted { get; private set; }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }
}

