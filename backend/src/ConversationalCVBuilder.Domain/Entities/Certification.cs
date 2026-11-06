using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class Certification : BaseEntity
{
    private Certification()
    {
    }

    internal Certification(
        CvProfile cvProfile,
        string name,
        string? issuingOrganization,
        DateOnly? issueDate,
        DateOnly? expirationDate,
        string? credentialId,
        Uri? credentialUrl,
        int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(cvProfile);
        ValidateDates(issueDate, expirationDate);
        CvProfile = cvProfile;
        CvProfileId = cvProfile.Id;
        Name = CvProfile.RequiredText(name, nameof(name), 160);
        IssuingOrganization = CvProfile.OptionalText(issuingOrganization, 160);
        IssueDate = issueDate;
        ExpirationDate = expirationDate;
        CredentialId = CvProfile.OptionalText(credentialId, 120);
        CredentialUrl = ValidateUrl(credentialUrl);
        DisplayOrder = displayOrder;
    }

    public Guid CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? IssuingOrganization { get; private set; }
    public DateOnly? IssueDate { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public string? CredentialId { get; private set; }
    public Uri? CredentialUrl { get; private set; }
    public int DisplayOrder { get; private set; }

    public void Update(
        string name,
        string? issuingOrganization,
        DateOnly? issueDate,
        DateOnly? expirationDate,
        string? credentialId,
        Uri? credentialUrl,
        int displayOrder,
        Guid? actorId = null)
    {
        ValidateDates(issueDate, expirationDate);
        Name = CvProfile.RequiredText(name, nameof(name), 160);
        IssuingOrganization = CvProfile.OptionalText(issuingOrganization, 160);
        IssueDate = issueDate;
        ExpirationDate = expirationDate;
        CredentialId = CvProfile.OptionalText(credentialId, 120);
        CredentialUrl = ValidateUrl(credentialUrl);
        DisplayOrder = Math.Max(0, displayOrder);
        MarkUpdated(actorId);
    }

    private static void ValidateDates(DateOnly? issueDate, DateOnly? expirationDate)
    {
        if (issueDate.HasValue && expirationDate.HasValue && expirationDate < issueDate)
        {
            throw new ArgumentException("The expiration date cannot be earlier than the issue date.", nameof(expirationDate));
        }
    }

    private static Uri? ValidateUrl(Uri? url)
    {
        if (url is not null && (!url.IsAbsoluteUri || url.Scheme is not ("http" or "https")))
        {
            throw new ArgumentException("A credential URL must be an absolute HTTP or HTTPS URL.", nameof(url));
        }

        return url;
    }
}
