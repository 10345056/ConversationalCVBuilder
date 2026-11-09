namespace ConversationalCVBuilder.Application.Contracts;

public sealed record CvAiProfile
{
    public string FullName { get; init; } = string.Empty;
    public string TargetRole { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string LinkedIn { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<string> Certifications { get; init; } = [];
    public IReadOnlyList<string> Experience { get; init; } = [];
    public IReadOnlyList<string> Projects { get; init; } = [];
    public IReadOnlyList<string> Education { get; init; } = [];
    public IReadOnlyList<string> Languages { get; init; } = [];
    public IReadOnlyList<string> Achievements { get; init; } = [];
    public IReadOnlyList<string> AdditionalInformation { get; init; } = [];

    public CvAiProfile WithFallback(CvAiProfile fallback) => this with
    {
        FullName = Use(FullName, fallback.FullName),
        TargetRole = Use(TargetRole, fallback.TargetRole),
        Email = Use(Email, fallback.Email),
        Phone = Use(Phone, fallback.Phone),
        Location = Use(Location, fallback.Location),
        LinkedIn = Use(LinkedIn, fallback.LinkedIn),
        Summary = Use(Summary, fallback.Summary),
        Skills = Merge(Skills, fallback.Skills),
        Certifications = Merge(Certifications, fallback.Certifications),
        Experience = Merge(Experience, fallback.Experience),
        Projects = Merge(Projects, fallback.Projects),
        Education = Merge(Education, fallback.Education),
        Languages = Merge(Languages, fallback.Languages),
        Achievements = Merge(Achievements, fallback.Achievements),
        AdditionalInformation = Merge(AdditionalInformation, fallback.AdditionalInformation),
    };

    private static string Use(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static IReadOnlyList<string> Merge(
        IReadOnlyList<string> value,
        IReadOnlyList<string> fallback) =>
        (value ?? [])
            .Concat(fallback ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}

public record CvAiAnalysis
{
    public CvAiProfile Profile { get; init; } = new();
    public IReadOnlyList<string> MissingFields { get; init; } = [];
    public string NextQuestion { get; init; } = string.Empty;
    public IReadOnlyList<string> SuggestedImprovements { get; init; } = [];
    public string TemplateRecommendation { get; init; } = "modern";
}

public sealed record CvAiConversationResponse : CvAiAnalysis
{
    public string Reply { get; init; } = string.Empty;
}

public sealed record AiConversationMessage(string Role, string Content);

public sealed record AiResult<T>(T? Value, string? Error)
{
    public bool Succeeded => Value is not null;

    public static AiResult<T> Success(T value) => new(value, null);
    public static AiResult<T> Failure(string error) => new(default, error);
}
