using System.Text.Json;
using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using Microsoft.Extensions.Configuration;

namespace ConversationalCVBuilder.Infrastructure.Services;

public sealed class CvDesignPresetStore : ICvDesignPresetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public CvDesignPresetStore(IConfiguration configuration)
    {
        _filePath = configuration["DesignConfigurations:FilePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "cv-design-presets.json");
    }

    public async Task<IReadOnlyList<CvDesignConfiguration>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadAllAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<CvDesignConfiguration> SaveAsync(
        CvDesignConfiguration configuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var presets = (await ReadAllAsync(cancellationToken)).ToList();
            var saved = configuration with
            {
                Name = configuration.Name.Trim(),
                CustomSections = configuration.CustomSections
                    .Select(section => section with { Content = string.Empty })
                    .ToList()
            };
            var existingIndex = presets.FindIndex(preset =>
                string.Equals(preset.Id, saved.Id, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                presets[existingIndex] = saved;
            }
            else
            {
                presets.Add(saved);
            }

            await WriteAllAsync(presets, cancellationToken);
            return saved;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var presets = (await ReadAllAsync(cancellationToken)).ToList();
            var removed = presets.RemoveAll(preset =>
                string.Equals(preset.Id, id, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed)
            {
                await WriteAllAsync(presets, cancellationToken);
            }

            return removed;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<CvDesignConfiguration>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<CvDesignConfiguration>>(
            stream, JsonOptions, cancellationToken) ?? [];
    }

    private async Task WriteAllAsync(
        List<CvDesignConfiguration> presets,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("The design preset storage path is invalid.");
        Directory.CreateDirectory(directory);
        var temporaryPath = $"{_filePath}.tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, presets, JsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, _filePath, overwrite: true);
    }
}
