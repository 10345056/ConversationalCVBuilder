using System.ComponentModel.DataAnnotations;

namespace ConversationalCVBuilder.Api.Contracts.Cv;

/// <summary>Profile fields generated from an uploaded resume.</summary>
public sealed record UpdateWorkspaceProfileRequest
{
    [Required, MaxLength(200)]
    public required string FullName { get; init; }

    [MaxLength(160)]
    public string TargetRole { get; init; } = string.Empty;

    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(40)]
    public string Phone { get; init; } = string.Empty;

    [MaxLength(300)]
    public string Location { get; init; } = string.Empty;

    [MaxLength(500)]
    public string LinkedIn { get; init; } = string.Empty;

    [MaxLength(5000)]
    public string Summary { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public IReadOnlyList<string> Skills { get; init; } = [];

    [Required, MaxLength(20)]
    public IReadOnlyList<string> Experience { get; init; } = [];

    [Required, MaxLength(20)]
    public IReadOnlyList<string> Education { get; init; } = [];

    [Required, MaxLength(20)]
    public IReadOnlyList<string> Certifications { get; init; } = [];

    [Required, MaxLength(20)]
    public IReadOnlyList<string> Projects { get; init; } = [];

    [Required, MaxLength(20)]
    public IReadOnlyList<string> Languages { get; init; } = [];

    [Required, MaxLength(30)]
    public IReadOnlyList<string> Achievements { get; init; } = [];
}
