using Dapper;
using WebApi.Dal.Models;

namespace WebApi.Dal;

public sealed class OrderItemRepository : IOrderItemRepository
{
    private readonly IUnitOfWork _uow;
    public OrderItemRepository(IUnitOfWork uow) => _uow = uow;

    public async Task BulkInsertAsync(IEnumerable<V1OrderItemDal> items, CancellationToken ct)
    {
        // простой вариант: обычный INSERT (для лабы ок)
        var sql = @"
insert into public.order_items (order_id, product_id, product_name, price_cents, price_currency, quantity)
values (@OrderId, @ProductId, @ProductName, @PriceCents, @PriceCurrency, @Quantity);";

        var conn = await _uow.GetOpenConnectionAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(sql, items, transaction: _uow.RequireTx(), cancellationToken: ct));
    }

    public async Task<IReadOnlyList<V1OrderItemDal>> QueryByOrderIdsAsync(IEnumerable<long> orderIds, CancellationToken ct)
    {
        var sql = @"
select id, order_id, product_id, product_name, price_cents, price_currency, quantity
from public.order_items
where order_id = any(@ids)
order by id;";

        var conn = await _uow.GetOpenConnectionAsync(ct);
        var list = await conn.QueryAsync<V1OrderItemDal>(new CommandDefinition(sql, new { ids = orderIds.ToArray() }, cancellationToken: ct));
        return list.AsList();
    }
}
