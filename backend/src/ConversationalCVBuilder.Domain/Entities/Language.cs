using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.Enums;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class Language : BaseEntity
{
    private Language()
    {
    }

    internal Language(CvProfile cvProfile, string name, ProficiencyLevel proficiency, int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(cvProfile);
        ValidateProficiency(proficiency);
        CvProfile = cvProfile;
        CvProfileId = cvProfile.Id;
        Name = CvProfile.RequiredText(name, nameof(name), 100);
        Proficiency = proficiency;
        DisplayOrder = displayOrder;
    }

    public Guid CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public ProficiencyLevel Proficiency { get; private set; }
    public int DisplayOrder { get; private set; }

    internal void UpdateProficiency(ProficiencyLevel proficiency)
    {
        ValidateProficiency(proficiency);
        Proficiency = proficiency;
        MarkUpdated(null);
    }

    public void Update(string name, ProficiencyLevel proficiency, int displayOrder, Guid? actorId = null)
    {
        ValidateProficiency(proficiency);
        Name = CvProfile.RequiredText(name, nameof(name), 100);
        Proficiency = proficiency;
        DisplayOrder = Math.Max(0, displayOrder);
        MarkUpdated(actorId);
    }

    private static void ValidateProficiency(ProficiencyLevel proficiency)
    {
        if (!Enum.IsDefined(proficiency))
        {
            throw new ArgumentOutOfRangeException(nameof(proficiency));
        }
    }
}
