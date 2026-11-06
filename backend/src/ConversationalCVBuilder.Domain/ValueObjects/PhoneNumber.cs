namespace ConversationalCVBuilder.Domain.ValueObjects;

public sealed record PhoneNumber
{
    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static PhoneNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        var digitCount = normalized.Count(char.IsDigit);
        if (digitCount is < 7 or > 20
            || normalized.Any(character =>
                !char.IsDigit(character)
                && character is not '+' and not ' ' and not '-' and not '(' and not ')' and not '.'))
        {
            throw new ArgumentException("A valid phone number is required.", nameof(value));
        }

        return new PhoneNumber(normalized);
    }

    public override string ToString() => Value;
}
