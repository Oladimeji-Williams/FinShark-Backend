namespace FinShark.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; init; }

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

