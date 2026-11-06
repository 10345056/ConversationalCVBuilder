using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Application.Mapping;

public sealed class CvProfileMapper : ICvProfileMapper
{
    public CvProfileDto Map(CvProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return new CvProfileDto(
            profile.Id,
            profile.Title,
            profile.FullName,
            profile.TargetRole,
            profile.Email,
            profile.Phone,
            profile.Location,
            profile.Summary,
            profile.SelectedTemplateKey,
            profile.SkillItems
                .OrderBy(item => item.DisplayOrder)
                .Select(item => item.Name)
                .ToArray(),
            profile.WorkExperiences
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new WorkExperienceDto(
                    item.Id, item.PositionTitle, item.EmployerName, item.StartDate,
                    item.EndDate, item.IsCurrent, item.Location, item.Description))
                .ToArray(),
            profile.EducationEntries
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new EducationDto(
                    item.Id, item.Qualification, item.InstitutionName, item.FieldOfStudy,
                    item.StartDate, item.EndDate, item.IsInProgress, item.Description,
                    item.Grade, item.GradeScale))
                .ToArray(),
            profile.Certifications
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new CertificationDto(
                    item.Id, item.Name, item.IssuingOrganization, item.IssueDate,
                    item.ExpirationDate, item.CredentialId, item.CredentialUrl?.ToString()))
                .ToArray(),
            profile.Languages
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new LanguageDto(item.Id, item.Name, item.Proficiency.ToString()))
                .ToArray(),
            profile.Projects
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new ProjectDto(
                    item.Id, item.Name, item.Description, item.ProjectUrl?.ToString(),
                    item.StartDate, item.EndDate, item.Technologies.ToArray()))
                .ToArray(),
            profile.Achievements);
    }
}
