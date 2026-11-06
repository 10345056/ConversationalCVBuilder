namespace ConversationalCVBuilder.Application.Contracts;

public sealed record CvProfileDto(
    Guid Id,
    string Title,
    string FullName,
    string TargetRole,
    string Email,
    string Phone,
    string Location,
    string Summary,
    string SelectedTemplateKey,
    IReadOnlyList<string> Skills,
    IReadOnlyList<WorkExperienceDto> Experience,
    IReadOnlyList<EducationDto> Education,
    IReadOnlyList<CertificationDto> Certifications,
    IReadOnlyList<LanguageDto> Languages,
    IReadOnlyList<ProjectDto> Projects,
    IReadOnlyList<string> Achievements);

public sealed record WorkExperienceDto(
    Guid Id,
    string PositionTitle,
    string? EmployerName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string? Location,
    string? Description);

public sealed record EducationDto(
    Guid Id,
    string Qualification,
    string? InstitutionName,
    string? FieldOfStudy,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsInProgress,
    string? Description,
    decimal? Grade,
    string? GradeScale);

public sealed record CertificationDto(
    Guid Id,
    string Name,
    string? IssuingOrganization,
    DateOnly? IssueDate,
    DateOnly? ExpirationDate,
    string? CredentialId,
    string? CredentialUrl);

public sealed record LanguageDto(Guid Id, string Name, string Proficiency);

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string? ProjectUrl,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<string> Technologies);
