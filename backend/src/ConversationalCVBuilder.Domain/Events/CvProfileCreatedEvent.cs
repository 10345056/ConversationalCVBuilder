using ConversationalCVBuilder.Domain.Common;

namespace ConversationalCVBuilder.Domain.Events;

public sealed record CvProfileCreatedEvent(Guid ProfileId, Guid OwnerId) : DomainEvent;
