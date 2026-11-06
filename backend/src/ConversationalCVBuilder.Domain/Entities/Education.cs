using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class Education : BaseEntity
{
    private Education()
    {
    }

    internal Education(
        CvProfile cvProfile,
        string qualification,
        string? institutionName,
        string? fieldOfStudy,
        DateOnly? startDate,
        DateOnly? endDate,
        bool isInProgress,
        string? description,
        decimal? grade,
        string? gradeScale,
        int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(cvProfile);
        ValidateDates(startDate, endDate, isInProgress);
        ValidateGrade(grade);
        CvProfile = cvProfile;
        CvProfileId = cvProfile.Id;
        Qualification = CvProfile.RequiredText(qualification, nameof(qualification), 160);
        InstitutionName = CvProfile.OptionalText(institutionName, 160);
        FieldOfStudy = CvProfile.OptionalText(fieldOfStudy, 160);
        StartDate = startDate;
        EndDate = endDate;
        IsInProgress = isInProgress;
        Description = CvProfile.OptionalText(description, 3000);
        Grade = grade;
        GradeScale = CvProfile.OptionalText(gradeScale, 40);
        DisplayOrder = displayOrder;
    }

    public Guid CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string Qualification { get; private set; } = string.Empty;
    public string? InstitutionName { get; private set; }
    public string? FieldOfStudy { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsInProgress { get; private set; }
    public string? Description { get; private set; }
    public decimal? Grade { get; private set; }
    public string? GradeScale { get; private set; }
    public int DisplayOrder { get; private set; }
    public string DisplayText => string.Join(
        Environment.NewLine,
        new[] { Qualification, InstitutionName, FieldOfStudy, Description }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal));

    public void Update(
        string qualification,
        string? institutionName,
        string? fieldOfStudy,
        DateOnly? startDate,
        DateOnly? endDate,
        bool isInProgress,
        string? description,
        decimal? grade,
        string? gradeScale,
        int displayOrder,
        Guid? actorId = null)
    {
        ValidateDates(startDate, endDate, isInProgress);
        ValidateGrade(grade);
        Qualification = CvProfile.RequiredText(qualification, nameof(qualification), 160);
        InstitutionName = CvProfile.OptionalText(institutionName, 160);
        FieldOfStudy = CvProfile.OptionalText(fieldOfStudy, 160);
        StartDate = startDate;
        EndDate = endDate;
        IsInProgress = isInProgress;
        Description = CvProfile.OptionalText(description, 3000);
        Grade = grade;
        GradeScale = CvProfile.OptionalText(gradeScale, 40);
        DisplayOrder = Math.Max(0, displayOrder);
        MarkUpdated(actorId);
    }

    private static void ValidateDates(DateOnly? startDate, DateOnly? endDate, bool isInProgress)
    {
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new ArgumentException("The end date cannot be earlier than the start date.", nameof(endDate));
        }

        if (isInProgress && endDate.HasValue)
        {
            throw new ArgumentException("Education in progress cannot have an end date.", nameof(endDate));
        }
    }

    private static void ValidateGrade(decimal? grade)
    {
        if (grade is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(grade), "A grade must be between 0 and 100.");
        }
    }
}
