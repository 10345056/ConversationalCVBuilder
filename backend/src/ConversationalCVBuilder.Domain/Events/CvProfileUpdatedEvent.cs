using ConversationalCVBuilder.Domain.Common;

namespace ConversationalCVBuilder.Domain.Events;

public sealed record CvProfileUpdatedEvent(Guid ProfileId, string Section) : DomainEvent;
