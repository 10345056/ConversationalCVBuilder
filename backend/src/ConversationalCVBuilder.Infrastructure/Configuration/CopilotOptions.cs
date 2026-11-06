namespace ConversationalCVBuilder.Infrastructure.Configuration;

public sealed class CopilotOptions
{
    public const string SectionName = "Copilot";
    public string Model { get; set; } = "auto";
    public int TimeoutSeconds { get; set; } = 90;
}
