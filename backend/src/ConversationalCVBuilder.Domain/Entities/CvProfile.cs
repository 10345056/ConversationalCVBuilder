using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.Enums;
using ConversationalCVBuilder.Domain.Events;
using ConversationalCVBuilder.Domain.ValueObjects;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class CvProfile : AggregateRoot, IDomainEventSource
{
    private readonly List<Certification> _certifications = [];
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<Education> _education = [];
    private readonly List<string> _achievements = [];
    private readonly List<Language> _languages = [];
    private readonly List<Project> _projects = [];
    private readonly List<Skill> _skills = [];
    private readonly List<WorkExperience> _workExperiences = [];
    private string _summary = string.Empty;
    private string _targetRole = string.Empty;
    private string _title = "CV";

    private CvProfile()
    {
        PersonalInformation = new PersonalInformation(Id);
    }

    public CvProfile(Guid ownerId, string title = "CV")
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("A profile must have an owner.", nameof(ownerId));
        }

        OwnerId = ownerId;
        _title = RequiredText(title, nameof(title), 120);
        PersonalInformation = new PersonalInformation(Id);
        _domainEvents.Add(new CvProfileCreatedEvent(Id, OwnerId));
    }

    public Guid OwnerId { get; private set; }

    [JsonIgnore]
    public User? Owner { get; private set; }

    public string Title => _title;
    public string TargetRole => _targetRole;
    public string Summary => _summary;
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? LastUsedAtUtc { get; private set; }
    public bool IsDefault { get; private set; }
    public string SelectedTemplateKey { get; private set; } = "classic";
    public Guid? SelectedTemplateId { get; private set; }

    [JsonIgnore]
    public Template? SelectedTemplate { get; private set; }

    public PersonalInformation PersonalInformation { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<WorkExperience> WorkExperiences => _workExperiences.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<Education> EducationEntries => _education.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<Certification> Certifications => _certifications.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<Skill> SkillItems => _skills.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<Language> Languages => _languages.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // These projections preserve the original MVP API response shape.
    public string FullName => PersonalInformation.FullName;
    public string Email => PersonalInformation.Email?.Value ?? string.Empty;
    public string Phone => PersonalInformation.Phone?.Value ?? string.Empty;
    public string Location => PersonalInformation.Address?.Formatted ?? string.Empty;
    public string LinkedIn => PersonalInformation.LinkedInUrl?.ToString() ?? string.Empty;
    public IReadOnlyList<string> Skills => _skills.Select(skill => skill.Name).ToArray();
    public IReadOnlyList<string> Experience => _workExperiences.Select(item => item.DisplayText).ToArray();
    public IReadOnlyList<string> Education => _education.Select(item => item.DisplayText).ToArray();
    public IReadOnlyList<string> CertificationLabels => _certifications
        .OrderBy(item => item.DisplayOrder)
        .Select(item => string.Join(" | ", new[] { item.Name, item.IssuingOrganization }
            .Where(value => !string.IsNullOrWhiteSpace(value))))
        .ToArray();
    public IReadOnlyList<string> ProjectSummaries => _projects
        .OrderBy(item => item.DisplayOrder)
        .Select(item => string.Join(" | ", new[] { item.Name, item.Description }
            .Where(value => !string.IsNullOrWhiteSpace(value))))
        .ToArray();
    public IReadOnlyList<string> LanguageLabels => _languages
        .OrderBy(item => item.DisplayOrder)
        .Select(item => $"{item.Name} — {item.Proficiency}")
        .ToArray();
    public IReadOnlyList<string> Achievements => _achievements.AsReadOnly();

    public void ReplaceAchievements(IEnumerable<string> achievements, Guid? actorId = null)
    {
        ArgumentNullException.ThrowIfNull(achievements);
        _achievements.Clear();
        _achievements.AddRange(achievements
            .Select(achievement => RequiredText(achievement, nameof(achievements), 500))
            .Distinct(StringComparer.OrdinalIgnoreCase));
        Touch("achievements", actorId);
    }

    public void UpdateTitle(string title, Guid? actorId = null)
    {
        var normalized = RequiredText(title, nameof(title), 120);
        if (_title == normalized)
        {
            return;
        }

        _title = normalized;
        Touch("title", actorId);
    }

    public void UpdateTargetRole(string targetRole, Guid? actorId = null)
    {
        var normalized = OptionalText(targetRole, 160);
        if (_targetRole == normalized)
        {
            return;
        }

        _targetRole = normalized ?? string.Empty;
        Touch("targetRole", actorId);
    }

    public void UpdateSummary(string summary, Guid? actorId = null)
    {
        var normalized = OptionalText(summary, 5000);
        if (_summary == normalized)
        {
            return;
        }

        _summary = normalized ?? string.Empty;
        Touch("summary", actorId);
    }

    public void UpdatePersonalInformation(
        string givenName,
        string? familyName,
        EmailAddress? email = null,
        PhoneNumber? phone = null,
        PostalAddress? address = null,
        string? professionalHeadline = null,
        Uri? website = null,
        Uri? linkedInUrl = null,
        Guid? actorId = null)
    {
        PersonalInformation.Update(
            givenName, familyName, email, phone, address,
            professionalHeadline, website, linkedInUrl);
        Touch("personalInformation", actorId);
    }

    public WorkExperience AddWorkExperience(
        string positionTitle,
        string? employerName = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isCurrent = false,
        string? location = null,
        string? description = null,
        EmploymentType? employmentType = null,
        Guid? actorId = null)
    {
        var experience = new WorkExperience(
            this, positionTitle, employerName, startDate, endDate, isCurrent,
            location, description, employmentType, _workExperiences.Count);
        _workExperiences.Add(experience);
        Touch("workExperience", actorId);
        return experience;
    }

    public WorkExperience AddWorkExperienceNarrative(string narrative, Guid? actorId = null)
    {
        var normalized = RequiredText(narrative, nameof(narrative), 5000);
        return AddWorkExperience(
            normalized,
            description: normalized,
            actorId: actorId);
    }

    public Education AddEducation(
        string qualification,
        string? institutionName = null,
        string? fieldOfStudy = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isInProgress = false,
        string? description = null,
        decimal? grade = null,
        string? gradeScale = null,
        Guid? actorId = null)
    {
        var education = new Education(
            this, qualification, institutionName, fieldOfStudy, startDate,
            endDate, isInProgress, description, grade, gradeScale, _education.Count);
        _education.Add(education);
        Touch("education", actorId);
        return education;
    }

    public Education AddEducationNarrative(string narrative, Guid? actorId = null)
    {
        var normalized = RequiredText(narrative, nameof(narrative), 3000);
        return AddEducation(normalized, description: normalized, actorId: actorId);
    }

    public Certification AddCertification(
        string name,
        string? issuingOrganization = null,
        DateOnly? issueDate = null,
        DateOnly? expirationDate = null,
        string? credentialId = null,
        Uri? credentialUrl = null,
        Guid? actorId = null)
    {
        var certification = new Certification(
            this, name, issuingOrganization, issueDate, expirationDate,
            credentialId, credentialUrl, _certifications.Count);
        _certifications.Add(certification);
        Touch("certification", actorId);
        return certification;
    }

    public Skill AddSkill(
        string name,
        ProficiencyLevel? proficiency = null,
        string? category = null,
        Guid? actorId = null)
    {
        var normalized = RequiredText(name, nameof(name), 100);
        var existing = _skills.FirstOrDefault(skill =>
            string.Equals(skill.Name, normalized, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.Update(proficiency, category);
            Touch("skill", actorId);
            return existing;
        }

        var skill = new Skill(this, normalized, proficiency, category, _skills.Count);
        _skills.Add(skill);
        Touch("skill", actorId);
        return skill;
    }

    public void ReplaceSkills(IEnumerable<string> names, Guid? actorId = null)
    {
        ArgumentNullException.ThrowIfNull(names);
        var normalized = names
            .Select(name => RequiredText(name, nameof(names), 100))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _skills.Clear();
        for (var index = 0; index < normalized.Length; index++)
        {
            _skills.Add(new Skill(this, normalized[index], null, null, index));
        }

        Touch("skills", actorId);
    }

    public Language AddLanguage(string name, ProficiencyLevel proficiency, Guid? actorId = null)
    {
        var normalized = RequiredText(name, nameof(name), 100);
        var existing = _languages.FirstOrDefault(language =>
            string.Equals(language.Name, normalized, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.UpdateProficiency(proficiency);
            Touch("language", actorId);
            return existing;
        }

        var language = new Language(this, normalized, proficiency, _languages.Count);
        _languages.Add(language);
        Touch("language", actorId);
        return language;
    }

    public Project AddProject(
        string name,
        string? description = null,
        Uri? projectUrl = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        IEnumerable<string>? technologies = null,
        Guid? actorId = null)
    {
        var project = new Project(
            this, name, description, projectUrl, startDate, endDate,
            technologies, _projects.Count);
        _projects.Add(project);
        Touch("project", actorId);
        return project;
    }

    public void RemoveWorkExperience(Guid experienceId, Guid? actorId = null) =>
        RemoveChild(_workExperiences, experienceId, "workExperience", actorId);

    public void RemoveEducation(Guid educationId, Guid? actorId = null) =>
        RemoveChild(_education, educationId, "education", actorId);

    public void RemoveCertification(Guid certificationId, Guid? actorId = null) =>
        RemoveChild(_certifications, certificationId, "certification", actorId);

    public void RemoveSkill(Guid skillId, Guid? actorId = null) =>
        RemoveChild(_skills, skillId, "skill", actorId);

    public void RemoveLanguage(Guid languageId, Guid? actorId = null) =>
        RemoveChild(_languages, languageId, "language", actorId);

    public void RemoveProject(Guid projectId, Guid? actorId = null) =>
        RemoveChild(_projects, projectId, "project", actorId);

    public void SelectTemplate(Template template, Guid? actorId = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        if (!template.IsActive)
        {
            throw new DomainException("An inactive template cannot be selected.");
        }

        SelectedTemplateId = template.Id;
        SelectedTemplate = template;
        SelectedTemplateKey = template.Key;
        Touch("template", actorId);
    }

    public void SelectTemplate(string templateKey, Guid? actorId = null)
    {
        var normalized = RequiredText(templateKey, nameof(templateKey), 80).ToLowerInvariant();
        if (SelectedTemplateKey == normalized && SelectedTemplateId is null)
        {
            return;
        }

        SelectedTemplateKey = normalized;
        SelectedTemplateId = null;
        SelectedTemplate = null;
        Touch("template", actorId);
    }

    public void SetAsDefault(Guid? actorId = null)
    {
        if (IsDefault)
        {
            return;
        }

        IsDefault = true;
        Touch("default", actorId);
    }

    public void MarkCompleted(Guid? actorId = null)
    {
        if (CompletedAtUtc.HasValue)
        {
            return;
        }

        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch("completion", actorId);
    }

    public void RecordUse(Guid? actorId = null)
    {
        LastUsedAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(actorId);
    }

    public void Delete(Guid? actorId = null) => MarkDeleted(actorId);

    public void ClearDomainEvents() => _domainEvents.Clear();

    internal static string RequiredText(string value, string parameterName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Text cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? OptionalText(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Text cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private void RemoveChild<TChild>(
        List<TChild> collection,
        Guid childId,
        string section,
        Guid? actorId) where TChild : BaseEntity
    {
        var child = collection.FirstOrDefault(item => item.Id == childId);
        if (child is null)
        {
            throw new DomainException($"The {section} entry does not belong to this CV profile.");
        }

        collection.Remove(child);
        Touch(section, actorId);
    }

    private void Touch(string section, Guid? actorId)
    {
        MarkUpdated(actorId);
        _domainEvents.Add(new CvProfileUpdatedEvent(Id, section));
    }
}
