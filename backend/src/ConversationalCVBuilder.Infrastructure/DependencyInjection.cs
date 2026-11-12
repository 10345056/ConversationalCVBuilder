using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Infrastructure.Configuration;
using ConversationalCVBuilder.Infrastructure.Persistence;
using ConversationalCVBuilder.Infrastructure.Repositories;
using ConversationalCVBuilder.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversationalCVBuilder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CvBuilder")
            ?? "Data Source=conversational-cv-builder.db";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ICvRepository, CvRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        services.AddSingleton<IWorkspaceRepository, InMemoryWorkspaceRepository>();
        services.AddScoped<ConversationalCVBuilder.Application.Services.WorkspaceService>();
        services.AddScoped<ConversationalCVBuilder.Application.Services.ConversationService>();

        services.AddSingleton<DocumentParserService>();
        services.AddSingleton<IDocumentParserService>(provider =>
            provider.GetRequiredService<DocumentParserService>());
        services.AddSingleton<IDocumentTextExtractor>(provider =>
            provider.GetRequiredService<DocumentParserService>());
        services.AddSingleton<IDocxExporter, DocxExportService>();
        services.AddSingleton<IPdfExporter, PdfExportService>();
        services.AddSingleton<ICvDesignPresetStore, CvDesignPresetStore>();

        var copilot = configuration.GetSection(CopilotOptions.SectionName).Get<CopilotOptions>()
            ?? new CopilotOptions();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(copilot));
        services.AddSingleton<IAiService, CopilotAIService>();

        var ollama = configuration.GetSection(OllamaOptions.SectionName).Get<OllamaOptions>()
            ?? new OllamaOptions();
        ValidateOllamaOptions(ollama);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(ollama));
        services.AddHttpClient<IOllamaService, OllamaService>(client =>
        {
            client.BaseAddress = new Uri(ollama.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(ollama.TimeoutSeconds);
        });

        var whisper = configuration.GetSection(WhisperOptions.SectionName).Get<WhisperOptions>()
            ?? new WhisperOptions();
        ValidateWhisperOptions(whisper);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(whisper));
        services.AddHttpClient<IWhisperService, WhisperService>(client =>
        {
            client.BaseAddress = new Uri(whisper.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(whisper.TimeoutSeconds);
        });

        return services;
    }

    private static void ValidateOllamaOptions(OllamaOptions options)
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme is not ("http" or "https")
            || string.IsNullOrWhiteSpace(options.Model)
            || options.Temperature is < 0 or > 2
            || options.TimeoutSeconds is < 1 or > 600)
        {
            throw new InvalidOperationException("The Ollama configuration is invalid.");
        }
    }

    private static void ValidateWhisperOptions(WhisperOptions options)
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme is not ("http" or "https")
            || string.IsNullOrWhiteSpace(options.TranscriptionsPath)
            || Uri.TryCreate(options.TranscriptionsPath, UriKind.Absolute, out _)
            || string.IsNullOrWhiteSpace(options.Model)
            || options.TimeoutSeconds is < 1 or > 600
            || options.MaximumAudioBytes is < 1 or > 100 * 1024 * 1024)
        {
            throw new InvalidOperationException("The Whisper configuration is invalid.");
        }
    }
}
