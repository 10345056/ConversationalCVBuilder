namespace ConversationalCVBuilder.Domain.ValueObjects;

public sealed record EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static EmailAddress Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        var separator = normalized.LastIndexOf('@');
        if (separator <= 0
            || separator == normalized.Length - 1
            || normalized.IndexOf('@') != separator
            || normalized.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("A valid email address is required.", nameof(value));
        }

        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;
}
