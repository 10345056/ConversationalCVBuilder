using System.Text.RegularExpressions;
using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Domain.Enums;
using ConversationalCVBuilder.Domain.ValueObjects;

namespace ConversationalCVBuilder.Application.Services;

public sealed class ConversationService(IWorkspaceRepository repository)
{
    private static readonly Regex EmailPattern = new(
        @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new(
        @"\+?[\d][\d\s().-]{6,}\d",
        RegexOptions.Compiled);

    public Workspace Send(string message, string? aiFallbackNotice = null)
    {
        var workspace = repository.Get();
        var answer = message.Trim();
        workspace.AddMessage("user", answer);

        var reply = workspace.CurrentStep switch
        {
            "fullName" => SetName(workspace, answer),
            "targetRole" => SetTargetRole(workspace, answer),
            "contact" => SetContact(workspace, answer),
            "summary" => SetSummary(workspace, answer),
            "skills" => SetSkills(workspace, answer),
            "experience" => AddExperience(workspace, answer),
            "education" => AddEducation(workspace, answer),
            _ => UpdateDraft(workspace, answer)
        };

        if (!string.IsNullOrWhiteSpace(aiFallbackNotice))
        {
            reply = $"{aiFallbackNotice.Trim()} {reply}";
        }

        workspace.AddMessage("assistant", reply);
        return workspace;
    }

    private static string SetName(Workspace workspace, string answer)
    {
        var nameParts = answer.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        workspace.Profile.UpdatePersonalInformation(
            nameParts[0],
            nameParts.Length > 1 ? nameParts[1] : null);
        workspace.SetCurrentStep("targetRole");
        return "Nice to meet you! What role or job title are you targeting?";
    }

    private static string SetTargetRole(Workspace workspace, string answer)
    {
        workspace.Profile.UpdateTargetRole(answer);
        workspace.SetCurrentStep("contact");
        return "What contact details should appear on your CV? Share your email, phone number, and location.";
    }

    private static string SetContact(Workspace workspace, string answer)
    {
        var email = EmailPattern.Match(answer);
        var phone = PhonePattern.Match(answer);
        var parsedEmail = email.Success ? EmailAddress.Create(email.Value) : null;
        var parsedPhone = phone.Success ? PhoneNumber.Create(phone.Value.Trim()) : null;
        var remainder = EmailPattern.Replace(answer, string.Empty);
        remainder = PhonePattern.Replace(remainder, string.Empty);
        var location = remainder.Trim().Trim(',', '|', ';');
        var address = string.IsNullOrWhiteSpace(location) ? null : PostalAddress.Create(location);
        workspace.Profile.UpdatePersonalInformation(
            workspace.Profile.PersonalInformation.GivenName,
            workspace.Profile.PersonalInformation.FamilyName,
            parsedEmail,
            parsedPhone,
            address);
        workspace.SetCurrentStep("summary");
        return "In a few sentences, how would you describe your professional profile and strengths?";
    }

    private static string SetSummary(Workspace workspace, string answer)
    {
        workspace.Profile.UpdateSummary(answer);
        workspace.SetCurrentStep("skills");
        return "List your key skills, separated by commas. You can include tools, technologies, and strengths.";
    }

    private static string SetSkills(Workspace workspace, string answer)
    {
        workspace.Profile.ReplaceSkills(answer
            .Split([',', ';', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase));
        workspace.SetCurrentStep("experience");
        return "Tell me about one recent role: title, company, dates, and a few achievements. You can add more than one role in a single message.";
    }

    private static string AddExperience(Workspace workspace, string answer)
    {
        workspace.Profile.AddWorkExperienceNarrative(answer);
        workspace.SetCurrentStep("education");
        return "What education, certifications, or training would you like to include?";
    }

    private static string AddEducation(Workspace workspace, string answer)
    {
        workspace.Profile.AddEducationNarrative(answer);
        workspace.SetCurrentStep("complete");
        return "Your first draft is ready. Review it in the preview, choose a template, and export it. To keep editing, start a message with summary:, skills:, experience:, or education: followed by the updated details.";
    }

    private static string UpdateDraft(Workspace workspace, string answer)
    {
        var separator = answer.IndexOf(':');
        if (separator <= 0)
        {
            return "Your CV draft is ready. To edit it manually, start a message with a section name such as summary:, skills:, experience:, projects:, education:, certifications:, languages:, or achievements: followed by the details.";
        }

        var section = answer[..separator].Trim().ToLowerInvariant();
        var value = answer[(separator + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Add the details after the section name, for example: skills: research, writing, project management.";
        }

        workspace.SetCurrentStep("complete");
        switch (section)
        {
            case "name":
            case "full name":
                var nameParts = value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                workspace.Profile.UpdatePersonalInformation(
                    nameParts[0][..Math.Min(nameParts[0].Length, 100)],
                    nameParts.Length > 1 ? nameParts[1][..Math.Min(nameParts[1].Length, 100)] : null,
                    workspace.Profile.PersonalInformation.Email,
                    workspace.Profile.PersonalInformation.Phone,
                    workspace.Profile.PersonalInformation.Address,
                    workspace.Profile.TargetRole,
                    linkedInUrl: workspace.Profile.PersonalInformation.LinkedInUrl);
                return "I updated your name.";
            case "target role":
            case "role":
                workspace.Profile.UpdateTargetRole(value[..Math.Min(value.Length, 160)]);
                return "I updated your target role.";
            case "email":
                var email = EmailPattern.Match(value);
                if (!email.Success || email.Value.Length > 320)
                {
                    return "That email address does not look valid. Please use email: name@example.com.";
                }
                workspace.Profile.UpdatePersonalInformation(
                    workspace.Profile.PersonalInformation.GivenName,
                    workspace.Profile.PersonalInformation.FamilyName,
                    EmailAddress.Create(email.Value),
                    workspace.Profile.PersonalInformation.Phone,
                    workspace.Profile.PersonalInformation.Address,
                    workspace.Profile.TargetRole,
                    linkedInUrl: workspace.Profile.PersonalInformation.LinkedInUrl);
                return "I updated your email address.";
            case "phone":
                var phone = PhonePattern.Match(value);
                var phoneDigits = phone.Success ? phone.Value.Count(char.IsDigit) : 0;
                if (!phone.Success
                    || phoneDigits is < 7 or > 20
                    || phone.Value.Any(character =>
                        !char.IsDigit(character)
                        && character is not '+' and not ' ' and not '-' and not '(' and not ')' and not '.'))
                {
                    return "That phone number does not look valid. Please include at least seven digits.";
                }
                workspace.Profile.UpdatePersonalInformation(
                    workspace.Profile.PersonalInformation.GivenName,
                    workspace.Profile.PersonalInformation.FamilyName,
                    workspace.Profile.PersonalInformation.Email,
                    PhoneNumber.Create(phone.Value.Trim()),
                    workspace.Profile.PersonalInformation.Address,
                    workspace.Profile.TargetRole,
                    linkedInUrl: workspace.Profile.PersonalInformation.LinkedInUrl);
                return "I updated your phone number.";
            case "location":
                if (value.Length > 300)
                {
                    return "Please keep the location under 300 characters.";
                }
                workspace.Profile.UpdatePersonalInformation(
                    workspace.Profile.PersonalInformation.GivenName,
                    workspace.Profile.PersonalInformation.FamilyName,
                    workspace.Profile.PersonalInformation.Email,
                    workspace.Profile.PersonalInformation.Phone,
                    PostalAddress.Create(value),
                    workspace.Profile.TargetRole,
                    linkedInUrl: workspace.Profile.PersonalInformation.LinkedInUrl);
                return "I updated your location.";
            case "linkedin":
            case "linkedin url":
                if (value.Length > 500
                    || !Uri.TryCreate(value, UriKind.Absolute, out var linkedInUrl)
                    || linkedInUrl.Scheme is not ("http" or "https"))
                {
                    return "Please provide a complete LinkedIn URL, for example: https://www.linkedin.com/in/name.";
                }
                workspace.Profile.UpdatePersonalInformation(
                    workspace.Profile.PersonalInformation.GivenName,
                    workspace.Profile.PersonalInformation.FamilyName,
                    workspace.Profile.PersonalInformation.Email,
                    workspace.Profile.PersonalInformation.Phone,
                    workspace.Profile.PersonalInformation.Address,
                    workspace.Profile.TargetRole,
                    linkedInUrl: linkedInUrl);
                return "I updated your LinkedIn URL.";
            case "summary":
                workspace.Profile.UpdateSummary(value);
                return "I updated your professional summary.";
            case "skills":
                workspace.Profile.ReplaceSkills(value
                    .Split([',', ';', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase));
                return "I updated your skills.";
            case "experience":
                workspace.Profile.AddWorkExperienceNarrative(value);
                return "I added that experience entry.";
            case "education":
                workspace.Profile.AddEducationNarrative(value);
                return "I added that education entry.";
            case "certification":
            case "certifications":
                foreach (var item in SplitEntries(value))
                {
                    workspace.Profile.AddCertification(item);
                }
                return "I added your certification details.";
            case "projects":
            case "project":
                foreach (var item in SplitEntries(value))
                {
                    var projectSeparator = item.IndexOf(" | ", StringComparison.Ordinal);
                    workspace.Profile.AddProject(
                        projectSeparator > 0 ? item[..projectSeparator] : item,
                        projectSeparator > 0 ? item[(projectSeparator + 3)..] : null);
                }
                return "I added your project details.";
            case "languages":
            case "language":
                foreach (var item in SplitEntries(value))
                {
                    var languageSeparator = Math.Max(
                        item.LastIndexOf(" — ", StringComparison.Ordinal),
                        Math.Max(
                            item.LastIndexOf(" – ", StringComparison.Ordinal),
                            item.LastIndexOf(" - ", StringComparison.Ordinal)));
                    var name = languageSeparator > 0 ? item[..languageSeparator] : item;
                    var proficiencyText = languageSeparator > 0
                        ? item[(languageSeparator + 3)..].Replace(" ", string.Empty)
                        : string.Empty;
                    var proficiency = Enum.TryParse<ProficiencyLevel>(proficiencyText, true, out var parsed)
                        ? parsed
                        : ProficiencyLevel.Intermediate;
                    workspace.Profile.AddLanguage(name, proficiency);
                }
                return "I added your language details.";
            case "achievement":
            case "achievements":
                workspace.Profile.ReplaceAchievements(SplitEntries(value));
                return "I updated your achievements.";
            default:
                return "I couldn't match that section. Use name:, target role:, contact:, summary:, skills:, experience:, projects:, education:, certifications:, languages:, or achievements:.";
        }
    }

    private static IReadOnlyList<string> SplitEntries(string value) =>
        value.Split(['\n', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.TrimStart('•', '-', '–', ' '))
            .Where(item => item.Length > 1)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
