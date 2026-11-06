namespace ConversationalCVBuilder.Application.Contracts;

public sealed record GeneratedCvDto(
    CvProfileDto Profile,
    DateTimeOffset GeneratedAtUtc);
