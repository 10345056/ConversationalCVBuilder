namespace ConversationalCVBuilder.Infrastructure.Services;

internal static class CvExportMissingContent
{
    public static string For(string section) => section switch
    {
        "Professional Summary" => "Add a concise summary of your professional experience and strengths.",
        "Skills" => "Add relevant skills to highlight your strengths.",
        "Certifications" => "Add certification details to complete your CV.",
        "Experience" => "Add professional experience details to complete your CV.",
        "Projects" => "Add relevant projects to complete your CV.",
        "Education" => "Add your education details to complete your CV.",
        "Languages" => "Add language details to complete your CV.",
        "Achievements" => "Add your achievements to complete your CV.",
        _ => "Add details to complete your CV.",
    };
}
