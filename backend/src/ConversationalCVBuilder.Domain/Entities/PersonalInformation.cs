using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.ValueObjects;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class PersonalInformation : BaseEntity
{
    private PersonalInformation()
    {
    }

    internal PersonalInformation(Guid cvProfileId)
    {
        if (cvProfileId == Guid.Empty)
        {
            throw new ArgumentException("Personal information must belong to a CV profile.", nameof(cvProfileId));
        }

        CvProfileId = cvProfileId;
    }

    public Guid CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string GivenName { get; private set; } = string.Empty;
    public string FamilyName { get; private set; } = string.Empty;
    public string FullName => string.Join(' ', new[] { GivenName, FamilyName }.Where(name => !string.IsNullOrWhiteSpace(name)));
    public EmailAddress? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public PostalAddress? Address { get; private set; }
    public string? ProfessionalHeadline { get; private set; }
    public Uri? Website { get; private set; }
    public Uri? LinkedInUrl { get; private set; }

    internal void Update(
        string givenName,
        string? familyName,
        EmailAddress? email,
        PhoneNumber? phone,
        PostalAddress? address,
        string? professionalHeadline,
        Uri? website,
        Uri? linkedInUrl)
    {
        GivenName = CvProfile.RequiredText(givenName, nameof(givenName), 100);
        FamilyName = CvProfile.OptionalText(familyName, 100) ?? string.Empty;
        Email = email;
        Phone = phone;
        Address = address;
        ProfessionalHeadline = CvProfile.OptionalText(professionalHeadline, 200);
        Website = ValidateWebUri(website, nameof(website));
        LinkedInUrl = ValidateWebUri(linkedInUrl, nameof(linkedInUrl));
        MarkUpdated(null);
    }

    private static Uri? ValidateWebUri(Uri? value, string parameterName)
    {
        if (value is null)
        {
            return null;
        }

        if (!value.IsAbsoluteUri || value.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException("A website URL must be an absolute HTTP or HTTPS URL.", parameterName);
        }

        return value;
    }
}
