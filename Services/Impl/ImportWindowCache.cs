using System.Text.Json;
using Dao.Cache;
using Dao.Interface;

namespace Services.Impl;

internal static class ImportWindowCache
{
    // A short window also covers sessions whose upstream data is still changing.
    internal static readonly TimeSpan Validity = TimeSpan.FromMinutes(5);

    internal static async Task<List<T>?> Read<T>(IUnityOfWork unit, string dataset, string key, CancellationToken cancellationToken)
    {
        var window = await unit.GetImportWindow(dataset, key, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (window == null || !window.IsComplete || window.CompletedAtUtc > now ||
            window.ExpiresAtUtc <= now || window.ExpiresAtUtc <= window.CompletedAtUtc)
            return null;
        try { return JsonSerializer.Deserialize<List<T>>(window.Payload); }
        catch (JsonException) { return null; }
    }

    internal static Task Stage<T>(IUnityOfWork unit, string dataset, string key, List<T> items, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return unit.StageImportWindow(new ImportWindow
        {
            Dataset = dataset, WindowKey = key, CompletedAtUtc = now,
            ExpiresAtUtc = now.Add(Validity), IsComplete = true,
            Payload = JsonSerializer.Serialize(items)
        }, cancellationToken);
    }
}
