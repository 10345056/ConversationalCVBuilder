namespace ConversationalCVBuilder.Domain.Common;

public interface IDomainEventSource
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
