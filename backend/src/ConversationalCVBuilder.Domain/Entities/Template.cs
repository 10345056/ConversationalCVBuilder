using ConversationalCVBuilder.Domain.Common;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class Template : BaseEntity
{
    private Template()
    {
    }

    public Template(
        string key,
        string name,
        string description,
        string category,
        string accentColor,
        string configurationJson,
        bool isBuiltIn = false,
        int displayOrder = 0)
    {
        Key = NormalizeKey(key);
        Name = CvProfile.RequiredText(name, nameof(name), 100);
        Description = CvProfile.OptionalText(description, 1000) ?? string.Empty;
        Category = CvProfile.RequiredText(category, nameof(category), 80);
        AccentColor = NormalizeColor(accentColor);
        ConfigurationJson = ValidateConfiguration(configurationJson);
        IsBuiltIn = isBuiltIn;
        DisplayOrder = Math.Max(0, displayOrder);
        IsActive = true;
    }

    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string AccentColor { get; private set; } = "#1D4ED8";
    public string ConfigurationJson { get; private set; } = "{}";
    public bool IsBuiltIn { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    public void Update(
        string name,
        string description,
        string category,
        string accentColor,
        string configurationJson,
        int displayOrder,
        Guid? actorId = null)
    {
        if (IsBuiltIn)
        {
            throw new DomainException("Built-in template identity and design cannot be edited.");
        }

        Name = CvProfile.RequiredText(name, nameof(name), 100);
        Description = CvProfile.OptionalText(description, 1000) ?? string.Empty;
        Category = CvProfile.RequiredText(category, nameof(category), 80);
        AccentColor = NormalizeColor(accentColor);
        ConfigurationJson = ValidateConfiguration(configurationJson);
        DisplayOrder = Math.Max(0, displayOrder);
        MarkUpdated(actorId);
    }

    public void Deactivate(Guid? actorId = null)
    {
        if (IsBuiltIn)
        {
            throw new DomainException("Built-in templates cannot be deactivated.");
        }

        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkUpdated(actorId);
    }

    public void Activate(Guid? actorId = null)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkUpdated(actorId);
    }

    private static string NormalizeKey(string key)
    {
        var normalized = CvProfile.RequiredText(key, nameof(key), 80).ToLowerInvariant();
        if (normalized.Any(character =>
                !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
        {
            throw new ArgumentException("A template key may contain only letters, digits, hyphens, and underscores.", nameof(key));
        }

        return normalized;
    }

    private static string NormalizeColor(string color)
    {
        var normalized = CvProfile.RequiredText(color, nameof(color), 7).ToUpperInvariant();
        if (normalized.Length != 7
            || normalized[0] != '#'
            || normalized[1..].Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException("An accent color must use the #RRGGBB format.", nameof(color));
        }

        return normalized;
    }

    private static string ValidateConfiguration(string configurationJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationJson);
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(configurationJson);
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                throw new ArgumentException("Template configuration must be a JSON object.", nameof(configurationJson));
            }
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new ArgumentException("Template configuration must be valid JSON.", nameof(configurationJson), exception);
        }

        return configurationJson;
    }
}
