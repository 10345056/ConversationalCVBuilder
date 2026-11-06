namespace ConversationalCVBuilder.Infrastructure.Services;

internal static class NttBrandAssets
{
    public static byte[] ReadLogo() =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "assets", "ntt-logo.png"));
}
