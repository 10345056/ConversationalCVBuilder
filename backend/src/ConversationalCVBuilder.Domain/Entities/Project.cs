using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class Project : BaseEntity
{
    private readonly List<string> _technologies = [];

    private Project()
    {
    }

    internal Project(
        CvProfile cvProfile,
        string name,
        string? description,
        Uri? projectUrl,
        DateOnly? startDate,
        DateOnly? endDate,
        IEnumerable<string>? technologies,
        int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(cvProfile);
        ValidateDates(startDate, endDate);
        CvProfile = cvProfile;
        CvProfileId = cvProfile.Id;
        Name = CvProfile.RequiredText(name, nameof(name), 160);
        Description = CvProfile.OptionalText(description, 5000);
        ProjectUrl = ValidateUrl(projectUrl);
        StartDate = startDate;
        EndDate = endDate;
        SetTechnologies(technologies);
        DisplayOrder = displayOrder;
    }

    public Guid CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Uri? ProjectUrl { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<string> Technologies => _technologies.AsReadOnly();

    public int DisplayOrder { get; private set; }

    public void Update(
        string name,
        string? description,
        Uri? projectUrl,
        DateOnly? startDate,
        DateOnly? endDate,
        IEnumerable<string>? technologies,
        int displayOrder,
        Guid? actorId = null)
    {
        ValidateDates(startDate, endDate);
        Name = CvProfile.RequiredText(name, nameof(name), 160);
        Description = CvProfile.OptionalText(description, 5000);
        ProjectUrl = ValidateUrl(projectUrl);
        StartDate = startDate;
        EndDate = endDate;
        SetTechnologies(technologies);
        DisplayOrder = Math.Max(0, displayOrder);
        MarkUpdated(actorId);
    }

    private void SetTechnologies(IEnumerable<string>? technologies)
    {
        _technologies.Clear();
        if (technologies is null)
        {
            return;
        }

        foreach (var technology in technologies)
        {
            var normalized = CvProfile.RequiredText(technology, nameof(technologies), 100);
            if (!_technologies.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                _technologies.Add(normalized);
            }
        }
    }

    private static void ValidateDates(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new ArgumentException("The end date cannot be earlier than the start date.", nameof(endDate));
        }
    }

    private static Uri? ValidateUrl(Uri? url)
    {
        if (url is not null && (!url.IsAbsoluteUri || url.Scheme is not ("http" or "https")))
        {
            throw new ArgumentException("A project URL must be an absolute HTTP or HTTPS URL.", nameof(url));
        }

        return url;
    }
}
