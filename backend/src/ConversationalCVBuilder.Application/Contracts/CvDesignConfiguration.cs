namespace ConversationalCVBuilder.Application.Contracts;

public sealed record CvDesignConfiguration
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PrimaryColor { get; init; } = "#00A9E0";
    public string SecondaryColor { get; init; } = "#003B5C";
    public string LightColor { get; init; } = "#F5FBFF";
    public string FontFamily { get; init; } = "Arial";
    public int FontSize { get; init; } = 10;
    public int PageMargin { get; init; } = 42;
    public int SectionSpacing { get; init; } = 8;
    public string Layout { get; init; } = "standard";
    public bool ShowLogo { get; init; } = true;
    public bool ShowFooter { get; init; } = true;
    public List<CvDesignSection> Sections { get; init; } = [];
    public List<CvDesignCustomSection> CustomSections { get; init; } = [];
}

public sealed record CvDesignSection(string Id, string Title, bool Enabled, int Order);

public sealed record CvDesignCustomSection(string Id, string Title, string Content);
