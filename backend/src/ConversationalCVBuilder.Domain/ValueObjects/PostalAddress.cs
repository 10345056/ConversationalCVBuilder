namespace ConversationalCVBuilder.Domain.ValueObjects;

public sealed record PostalAddress
{
    private PostalAddress(string formatted) => Formatted = formatted;

    public string Formatted { get; }

    public static PostalAddress Create(string formatted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatted);
        var normalized = formatted.Trim();
        if (normalized.Length > 300)
        {
            throw new ArgumentOutOfRangeException(nameof(formatted), "An address cannot exceed 300 characters.");
        }

        return new PostalAddress(normalized);
    }

    public override string ToString() => Formatted;
}
