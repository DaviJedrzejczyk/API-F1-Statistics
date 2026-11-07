using Entities.Dtos;
using Shared.Responses;

namespace Services.Interfaces;

public interface ILapCatalogService
{
    Task<SingleResponse<LapCatalogDto>> GetCatalog(int sessionKey, int[] drivers, int? lapNumber,
        string? compound, bool? hasAlerts, bool import, CancellationToken cancellationToken);
    Task<SingleResponse<LapSelectionDto>> Preview(int sessionKey, LapSelectionRequest request, CancellationToken cancellationToken);
    Task<SingleResponse<LapSelectionDto>> Confirm(int sessionKey, Guid previewId, CancellationToken cancellationToken);
    Task<SingleResponse<LapSelectionDto>> GetSelection(int sessionKey, Guid? previewId, CancellationToken cancellationToken);
}
