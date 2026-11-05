using Services.Models;

namespace Services.Interfaces;

public interface ILapCatalogService
{
    Task<LapCatalogResponse> Get(int sessionKey, int[] drivers, int? lapNumber, string? compound, bool? hasAlerts, bool import, CancellationToken cancellationToken);
    Task<LapSelectionResponse> Select(int sessionKey, LapSelectionRequest request, bool confirm, CancellationToken cancellationToken);
    Task<LapSelectionResponse?> GetConfirmed(int sessionKey, CancellationToken cancellationToken);
}
