using System.Data;

namespace WebApi.Dal;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<IDbConnection> GetOpenConnectionAsync(CancellationToken ct = default);
    Task BeginAsync(CancellationToken ct = default);
    IDbTransaction RequireTx();
    Task CommitAsync();
    Task RollbackAsync();
}
