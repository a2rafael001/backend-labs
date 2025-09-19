using Dapper;
using System.Text;
using WebApi.Dal.Models;

namespace WebApi.Dal;

public sealed class OrderRepository : IOrderRepository
{
    private readonly IUnitOfWork _uow;
    public OrderRepository(IUnitOfWork uow) => _uow = uow;

    public async Task<long> InsertAsync(V1OrderDal o, CancellationToken ct)
    {
        const string sql = @"
insert into public.orders (customer_id, total_price_cents, total_price_currency, created_at)
values (@CustomerId, @TotalPriceCents, @TotalPriceCurrency, now())
returning id;";

        var conn = await _uow.GetOpenConnectionAsync(ct);
        var id = await conn.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, o, transaction: _uow.RequireTx(), cancellationToken: ct));
        return id;
    }

    public async Task<IReadOnlyList<V1OrderDal>> QueryAsync(
        IReadOnlyCollection<long>? ids,
        IReadOnlyCollection<long>? customerIds,
        int page, int pageSize,
        CancellationToken ct)
    {
        var sb = new StringBuilder(@"
select id, customer_id, total_price_cents, total_price_currency, created_at
from public.orders
");
        var where = new List<string>();
        var p = new DynamicParameters();

        if (ids is { Count: > 0 })
        {
            where.Add("id = any(@ids)");
            p.Add("ids", ids.ToArray());
        }
        if (customerIds is { Count: > 0 })
        {
            where.Add("customer_id = any(@cids)");
            p.Add("cids", customerIds.ToArray());
        }
        if (where.Count > 0) sb.Append(" where ").Append(string.Join(" and ", where));

        sb.Append(" order by id desc limit @limit offset @offset;");
        p.Add("limit", pageSize);
        p.Add("offset", Math.Max(0, (page - 1) * pageSize)); // FIX: корректный offset

        var conn = await _uow.GetOpenConnectionAsync(ct);
        var list = await conn.QueryAsync<V1OrderDal>(new CommandDefinition(
            sb.ToString(), p, transaction: _uow.RequireTx(), cancellationToken: ct));
        return list.AsList();
    }

    // ← ДОБАВЛЕНО
    public async Task<V1OrderDal?> GetByIdAsync(long id, CancellationToken ct)
{
    const string sql = @"
select id, customer_id, total_price_cents, total_price_currency, created_at
from public.orders
where id = @id;";

    var conn = await _uow.GetOpenConnectionAsync(ct);
    return await conn.QuerySingleOrDefaultAsync<V1OrderDal>(new CommandDefinition(
        sql, new { id }, transaction: _uow.RequireTx(), cancellationToken: ct));
}


public async Task<int> UpdateAsync(long id, long totalPriceCents, string totalPriceCurrency, CancellationToken ct)
{
    const string sql = @"
update public.orders
set total_price_cents = @totalPriceCents,
    total_price_currency = @totalPriceCurrency
where id = @id;";

    var conn = await _uow.GetOpenConnectionAsync(ct);
    return await conn.ExecuteAsync(new CommandDefinition(
        sql, new { id, totalPriceCents, totalPriceCurrency },
        transaction: _uow.RequireTx(), cancellationToken: ct));
}


    public async Task<int> DeleteAsync(long id, CancellationToken ct)
    {
        // HARD delete:
        const string sql = @"delete from public.orders where id = @id;";

        var conn = await _uow.GetOpenConnectionAsync(ct);
        return await conn.ExecuteAsync(new CommandDefinition(
            sql, new { id }, transaction: _uow.RequireTx(), cancellationToken: ct));
    }

    public async Task<int> CountAsync(
        IReadOnlyCollection<long>? ids,
        IReadOnlyCollection<long>? customerIds,
        CancellationToken ct)
    {
        var sb = new StringBuilder("select count(*) from public.orders");
        var where = new List<string>();
        var p = new DynamicParameters();

        if (ids is { Count: > 0 })
        {
            where.Add("id = any(@ids)");
            p.Add("ids", ids.ToArray());
        }
        if (customerIds is { Count: > 0 })
        {
            where.Add("customer_id = any(@cids)");
            p.Add("cids", customerIds.ToArray());
        }
        if (where.Count > 0) sb.Append(" where ").Append(string.Join(" and ", where));
        sb.Append(';');

        var conn = await _uow.GetOpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            sb.ToString(), p, transaction: _uow.RequireTx(), cancellationToken: ct));
    }
}
