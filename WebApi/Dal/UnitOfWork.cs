using Microsoft.Extensions.Options;
using Models;
using Npgsql;
using System.Data;

namespace WebApi.Dal;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly NpgsqlDataSource _ds;
    private NpgsqlConnection? _conn;
    private NpgsqlTransaction? _tx;

    public UnitOfWork(IOptions<DbSettings> opt)
    {
        var cs = opt.Value.ConnectionString ?? throw new InvalidOperationException("DbSettings.ConnectionString not set");
        _ds = NpgsqlDataSource.Create(cs);
    }

    public async Task<IDbConnection> GetOpenConnectionAsync(CancellationToken ct = default)
    {
        if (_conn is { State: ConnectionState.Open }) return _conn;
        _conn = await _ds.OpenConnectionAsync(ct);
        return _conn;
    }

    public async Task BeginAsync(CancellationToken ct = default)
    {
        await GetOpenConnectionAsync(ct);
        _tx = await _conn!.BeginTransactionAsync(ct); // ← тут была ошибка со скобками
    }

    public IDbTransaction RequireTx() =>
        _tx ?? throw new InvalidOperationException("Transaction not started");

    public async Task CommitAsync()
    {
        if (_tx != null) await _tx.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_tx != null) await _tx.RollbackAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_tx != null) await _tx.DisposeAsync();
        if (_conn != null) await _conn.DisposeAsync();
        await _ds.DisposeAsync();
    }
}
