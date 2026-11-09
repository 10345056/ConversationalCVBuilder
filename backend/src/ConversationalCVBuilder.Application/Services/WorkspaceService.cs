using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Domain.Enums;
using ConversationalCVBuilder.Domain.ValueObjects;

namespace ConversationalCVBuilder.Application.Services;

public sealed class WorkspaceService(IWorkspaceRepository repository)
{
    public Workspace Get() => repository.Get();

    public Workspace SelectTemplate(string templateId)
    {
        var workspace = repository.Get();
        workspace.SelectTemplate(templateId);
        return workspace;
    }

    public Workspace AddDocument(string fileName, string extractedText)
    {
        var workspace = repository.Get();
        workspace.SetDocumentReference(fileName, extractedText);
        return workspace;
    }

    public Workspace UpdateProfile(
        string fullName,
        string targetRole,
        string email,
        string phone,
        string location,
        string linkedIn,
        string summary,
        IReadOnlyList<string> skills,
        IReadOnlyList<string> experience,
        IReadOnlyList<string> education,
        IReadOnlyList<string> certifications,
        IReadOnlyList<string> projects,
        IReadOnlyList<string> languages,
        IReadOnlyList<string> achievements,
        IReadOnlyList<string>? additionalInformation = null)
    {
        var profile = repository.Get().Profile;
        var nameParts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (nameParts.Length == 0)
        {
            throw new ArgumentException("A candidate name is required.", nameof(fullName));
        }

        profile.UpdatePersonalInformation(
            nameParts[0][..Math.Min(nameParts[0].Length, 100)],
            nameParts.Length > 1 ? nameParts[1][..Math.Min(nameParts[1].Length, 100)] : null,
            string.IsNullOrWhiteSpace(email) ? null : EmailAddress.Create(email),
            string.IsNullOrWhiteSpace(phone) ? null : PhoneNumber.Create(phone),
            string.IsNullOrWhiteSpace(location) ? null : PostalAddress.Create(location),
            targetRole,
            linkedInUrl: Uri.TryCreate(linkedIn, UriKind.Absolute, out var linkedInUri)
                && linkedInUri.Scheme is "http" or "https"
                    ? linkedInUri
                    : null);
        profile.UpdateTargetRole(targetRole);
        profile.UpdateSummary(summary);
        profile.ReplaceSkills((skills ?? []).Where(item => !string.IsNullOrWhiteSpace(item)));
        ClearEntries(profile);

        foreach (var item in (experience ?? []).Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            profile.AddWorkExperienceNarrative(item);
        }

        foreach (var item in (education ?? []).Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            profile.AddEducationNarrative(item);
        }

        foreach (var item in (certifications ?? []).Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            profile.AddCertification(item);
        }

        foreach (var item in (projects ?? []).Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            var separator = item.IndexOf(" | ", StringComparison.Ordinal);
            var name = separator > 0 ? item[..separator] : item;
            var description = separator > 0 ? item[(separator + 3)..] : null;
            if (name.Length > 160)
            {
                description = string.Join(Environment.NewLine, new[] { name[160..], description }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
                name = name[..160];
            }

            profile.AddProject(name, description);
        }

        foreach (var item in (languages ?? []).Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            var separator = Math.Max(
                item.LastIndexOf(" — ", StringComparison.Ordinal),
                Math.Max(
                    item.LastIndexOf(" – ", StringComparison.Ordinal),
                    item.LastIndexOf(" - ", StringComparison.Ordinal)));
            var name = separator > 0 ? item[..separator] : item;
            var proficiencyText = separator > 0 ? item[(separator + 3)..].Replace(" ", string.Empty) : string.Empty;
            var proficiency = Enum.TryParse<ProficiencyLevel>(proficiencyText, true, out var parsed)
                ? parsed
                : proficiencyText.Equals("Fluent", StringComparison.OrdinalIgnoreCase)
                    ? ProficiencyLevel.UpperIntermediate
                    : proficiencyText.Equals("Professional", StringComparison.OrdinalIgnoreCase)
                        ? ProficiencyLevel.Advanced
                        : ProficiencyLevel.Intermediate;
            profile.AddLanguage(name, proficiency);
        }

        profile.ReplaceAchievements((achievements ?? []).Where(item => !string.IsNullOrWhiteSpace(item)));
        profile.ReplaceAdditionalInformation(additionalInformation ?? []);
        var workspace = repository.Get();
        workspace.SetCurrentStep("complete");
        return workspace;
    }

    public Workspace UpdateProfile(CvAiProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return UpdateProfile(
            profile.FullName,
            profile.TargetRole,
            profile.Email,
            profile.Phone,
            profile.Location,
            profile.LinkedIn,
            profile.Summary,
            profile.Skills,
            profile.Experience,
            profile.Education,
            profile.Certifications,
            profile.Projects,
            profile.Languages,
            profile.Achievements,
            profile.AdditionalInformation);
    }

    public CvAiProfile GetAiProfile()
    {
        var profile = repository.Get().Profile;
        return new CvAiProfile
        {
            FullName = profile.FullName,
            TargetRole = profile.TargetRole,
            Email = profile.Email,
            Phone = profile.Phone,
            Location = profile.Location,
            LinkedIn = profile.LinkedIn,
            Summary = profile.Summary,
            Skills = profile.Skills,
            Experience = profile.Experience,
            Education = profile.Education,
            Certifications = profile.CertificationLabels,
            Projects = profile.ProjectSummaries,
            Languages = profile.LanguageLabels,
            Achievements = profile.Achievements,
            AdditionalInformation = profile.AdditionalInformation,
        };
    }

    public void SetAiInsights(
        string status,
        string? error,
        IEnumerable<string>? missingFields = null,
        string? nextQuestion = null,
        IEnumerable<string>? suggestedImprovements = null,
        string? templateRecommendation = null) =>
        repository.Get().SetAiInsights(
            status,
            error,
            missingFields,
            nextQuestion,
            suggestedImprovements,
            templateRecommendation);

    private static void ClearEntries(CvProfile profile)
    {
        foreach (var item in profile.WorkExperiences.ToArray())
        {
            profile.RemoveWorkExperience(item.Id);
        }

        foreach (var item in profile.EducationEntries.ToArray())
        {
            profile.RemoveEducation(item.Id);
        }

        foreach (var item in profile.Certifications.ToArray())
        {
            profile.RemoveCertification(item.Id);
        }

        foreach (var item in profile.Projects.ToArray())
        {
            profile.RemoveProject(item.Id);
        }

        foreach (var item in profile.Languages.ToArray())
        {
            profile.RemoveLanguage(item.Id);
        }
    }
}
