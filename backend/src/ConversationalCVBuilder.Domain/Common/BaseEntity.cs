namespace ConversationalCVBuilder.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is BaseEntity other
            && GetType() == other.GetType()
            && Id != Guid.Empty
            && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    internal void SetCreatedBy(Guid? userId)
    {
        CreatedBy = userId;
    }

    internal void MarkUpdated(Guid? userId)
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedBy = userId;
    }

    internal void MarkDeleted(Guid? userId)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(userId);
    }
}
