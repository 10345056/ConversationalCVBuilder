namespace ConversationalCVBuilder.Infrastructure.Services;

internal static class CvExportMissingContent
{
    public static string For(string section) => section switch
    {
        "Professional Summary" => "Add a verified professional summary before sharing.",
        "Technical Skills" => "Add relevant skills before sharing.",
        "Professional Experience" => "Add verified employment details before sharing.",
        "Key Achievements" => "Add verified achievements before sharing.",
        "Projects" => "Suggested project — please review: Enterprise Digital Transformation Platform. Replace with a project that reflects your experience.",
        "Certifications" => "Add verified certifications before sharing.",
        "Education" => "Add verified education details before sharing.",
        "Additional Information" => "Add languages or other relevant information before sharing.",
        _ => "Add verified information before sharing.",
    };
}
