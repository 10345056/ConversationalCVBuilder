namespace ConversationalCVBuilder.Domain.Common;

public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
