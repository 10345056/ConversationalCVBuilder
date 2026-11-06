using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.Enums;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class Skill : BaseEntity
{
    private Skill()
    {
    }

    internal Skill(
        CvProfile cvProfile,
        string name,
        ProficiencyLevel? proficiency,
        string? category,
        int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(cvProfile);
        CvProfile = cvProfile;
        CvProfileId = cvProfile.Id;
        Name = CvProfile.RequiredText(name, nameof(name), 100);
        Proficiency = proficiency;
        Category = CvProfile.OptionalText(category, 100);
        DisplayOrder = displayOrder;
    }

    public Guid CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public ProficiencyLevel? Proficiency { get; private set; }
    public string? Category { get; private set; }
    public int DisplayOrder { get; private set; }

    internal void Update(ProficiencyLevel? proficiency, string? category)
    {
        Proficiency = proficiency;
        Category = CvProfile.OptionalText(category, 100);
        MarkUpdated(null);
    }

    public void UpdateDetails(
        string name,
        ProficiencyLevel? proficiency,
        string? category,
        int displayOrder,
        Guid? actorId = null)
    {
        Name = CvProfile.RequiredText(name, nameof(name), 100);
        Proficiency = proficiency;
        Category = CvProfile.OptionalText(category, 100);
        DisplayOrder = Math.Max(0, displayOrder);
        MarkUpdated(actorId);
    }
}
