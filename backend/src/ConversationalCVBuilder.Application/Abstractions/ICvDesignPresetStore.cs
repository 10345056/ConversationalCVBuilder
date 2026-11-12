using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface ICvDesignPresetStore
{
    Task<IReadOnlyList<CvDesignConfiguration>> GetAllAsync(CancellationToken cancellationToken);
    Task<CvDesignConfiguration> SaveAsync(CvDesignConfiguration configuration, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
