using Dao.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dao.Impl;

internal sealed class ImportTransaction(IDbContextTransaction transaction, bool ownsTransaction) : IImportTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken) => ownsTransaction ? transaction.CommitAsync(cancellationToken) : Task.CompletedTask;
    public ValueTask DisposeAsync() => ownsTransaction ? transaction.DisposeAsync() : ValueTask.CompletedTask;
}
