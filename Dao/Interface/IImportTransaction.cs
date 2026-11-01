namespace Dao.Interface;

public interface IImportTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
