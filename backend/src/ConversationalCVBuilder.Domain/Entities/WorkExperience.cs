using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.Enums;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class WorkExperience : BaseEntity
{
    private WorkExperience()
    {
    }

    internal WorkExperience(
        CvProfile cvProfile,
        string positionTitle,
        string? employerName,
        DateOnly? startDate,
        DateOnly? endDate,
        bool isCurrent,
        string? location,
        string? description,
        EmploymentType? employmentType,
        int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(cvProfile);
        ValidateDates(startDate, endDate, isCurrent);
        CvProfile = cvProfile;
        CvProfileId = cvProfile.Id;
        PositionTitle = CvProfile.RequiredText(positionTitle, nameof(positionTitle), 160);
        EmployerName = CvProfile.OptionalText(employerName, 160);
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
        Location = CvProfile.OptionalText(location, 160);
        Description = CvProfile.OptionalText(description, 5000);
        EmploymentType = employmentType;
        DisplayOrder = displayOrder;
    }

    public Guid CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string PositionTitle { get; private set; } = string.Empty;
    public string? EmployerName { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsCurrent { get; private set; }
    public string? Location { get; private set; }
    public string? Description { get; private set; }
    public EmploymentType? EmploymentType { get; private set; }
    public int DisplayOrder { get; private set; }
    public string DisplayText => string.Join(
        Environment.NewLine,
        new[] { PositionTitle, EmployerName, Description }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal));

    public void Update(
        string positionTitle,
        string? employerName,
        DateOnly? startDate,
        DateOnly? endDate,
        bool isCurrent,
        string? location,
        string? description,
        EmploymentType? employmentType,
        int displayOrder,
        Guid? actorId = null)
    {
        ValidateDates(startDate, endDate, isCurrent);
        PositionTitle = CommonText(positionTitle, nameof(positionTitle), 160);
        EmployerName = CommonOptionalText(employerName, 160);
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
        Location = CommonOptionalText(location, 160);
        Description = CommonOptionalText(description, 5000);
        EmploymentType = employmentType;
        DisplayOrder = Math.Max(0, displayOrder);
        MarkUpdated(actorId);
    }

    private static void ValidateDates(DateOnly? startDate, DateOnly? endDate, bool isCurrent)
    {
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new ArgumentException("The end date cannot be earlier than the start date.", nameof(endDate));
        }

        if (isCurrent && endDate.HasValue)
        {
            throw new ArgumentException("A current position cannot have an end date.", nameof(endDate));
        }
    }

    private static string CommonText(string value, string parameterName, int maxLength) =>
        global::ConversationalCVBuilder.Domain.Entities.CvProfile.RequiredText(value, parameterName, maxLength);

    private static string? CommonOptionalText(string? value, int maxLength) =>
        global::ConversationalCVBuilder.Domain.Entities.CvProfile.OptionalText(value, maxLength);
}
