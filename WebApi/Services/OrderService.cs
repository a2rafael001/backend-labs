using Models.Dto.V1.Requests;
using Models.Dto.V1.Responses;
using WebApi.Dal;
using WebApi.Dal.Models;

namespace WebApi.Services;

public sealed class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderRepository _orders;
    private readonly IOrderItemRepository _items;

    public OrderService(IUnitOfWork uow, IOrderRepository orders, IOrderItemRepository items)
    {
        _uow = uow;
        _orders = orders;
        _items = items;
    }

    public async Task<V1CreateOrderResponse> BatchCreateAsync(V1CreateOrderRequest req, CancellationToken ct)
    {
        var resp = new V1CreateOrderResponse();

        try
        {
            await _uow.BeginAsync(ct);

            foreach (var src in req.Orders)
            {
                var orderId = await _orders.InsertAsync(new V1OrderDal
                {
                    CustomerId = src.CustomerId,
                    TotalPriceCents = src.TotalPriceCents,
                    TotalPriceCurrency = src.TotalPriceCurrency
                }, ct);

                var dalItems = src.Items.Select(i => new V1OrderItemDal
                {
                    OrderId = orderId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    PriceCents = i.PriceCents,
                    PriceCurrency = i.PriceCurrency,
                    Quantity = i.Quantity
                });

                await _items.BulkInsertAsync(dalItems, ct);

                resp.Orders.Add(new OrderView
                {
                    Id = orderId,
                    CustomerId = src.CustomerId,
                    TotalPriceCents = src.TotalPriceCents,
                    TotalPriceCurrency = src.TotalPriceCurrency,
                    CreatedAt = DateTime.UtcNow,
                    Items = src.Items.Select((it, idx) => new OrderItemView
                    {
                        Id = idx + 1,
                        OrderId = orderId,
                        ProductId = it.ProductId,
                        ProductName = it.ProductName,
                        PriceCents = it.PriceCents,
                        PriceCurrency = it.PriceCurrency,
                        Quantity = it.Quantity
                    }).ToList()
                });
            }

            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }

        return resp;
    }

    public async Task<(IReadOnlyList<OrderView> Orders, int Page, int PageSize, int Total)>
    QueryAsync(V1QueryOrdersRequest request, CancellationToken ct)
{
    await _uow.BeginAsync(ct);
    try
    {
        // 1) Читаем заказы и общий total
        var list = await _orders.QueryAsync(
            request.Ids, request.CustomerIds,
            request.Page, request.PageSize, ct);

        var total = await _orders.CountAsync(
            request.Ids, request.CustomerIds, ct);

        // 2) По требованию — подгружаем items
        Dictionary<long, List<OrderItemView>>? itemsByOrder = null;
        if (request.IncludeOrderItems == true && list.Count > 0)
        {
            var ids = list.Select(o => o.Id).ToArray();
            var items = await _items.QueryByOrderIdsAsync(ids, ct);

            itemsByOrder = items
                .GroupBy(i => i.OrderId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(i => new OrderItemView
                    {
                        Id = i.Id,
                        OrderId = i.OrderId,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        PriceCents = i.PriceCents,
                        PriceCurrency = i.PriceCurrency,
                        Quantity = i.Quantity
                    }).ToList()
                );
        }

        await _uow.CommitAsync();

        // 3) Маппинг
        var result = list.Select(o => new OrderView
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            TotalPriceCents = o.TotalPriceCents,
            TotalPriceCurrency = o.TotalPriceCurrency,
            CreatedAt = o.CreatedAt,
            Items = request.IncludeOrderItems
                ? (itemsByOrder != null && itemsByOrder.TryGetValue(o.Id, out var v) ? v : new List<OrderItemView>())
                : null
        }).ToList();

        return (result, request.Page, request.PageSize, total);
    }
    catch
    {
        await _uow.RollbackAsync();
        throw;
    }
}


    public async Task<OrderView?> GetByIdAsync(long id, CancellationToken ct)
{
    // транзакция не обязательно нужна для read-only, но можно оставить единообразно
    await _uow.BeginAsync(ct);
    try
    {
        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) { await _uow.CommitAsync(); return null; }

        var items = await _items.QueryByOrderIdsAsync(new[] { id }, ct);
        await _uow.CommitAsync();

        return new OrderView
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            TotalPriceCents = order.TotalPriceCents,
            TotalPriceCurrency = order.TotalPriceCurrency,
            CreatedAt = order.CreatedAt,
            Items = items.Where(i => i.OrderId == id).Select(i => new OrderItemView
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                PriceCents = i.PriceCents,
                PriceCurrency = i.PriceCurrency,
                Quantity = i.Quantity
            }).ToList()
        };
    }
    catch { await _uow.RollbackAsync(); throw; }
}

public async Task<bool> UpdateAsync(long id, V1UpdateOrderRequest req, CancellationToken ct)
{
    await _uow.BeginAsync(ct);
    try
    {
        var affected = await _orders.UpdateAsync(id, req.TotalPriceCents, req.TotalPriceCurrency, ct);
        await _uow.CommitAsync();
        return affected > 0;
    }
    catch { await _uow.RollbackAsync(); throw; }
}

public async Task<bool> DeleteAsync(long id, CancellationToken ct)
{
    await _uow.BeginAsync(ct);
    try
    {
        var affected = await _orders.DeleteAsync(id, ct);
        await _uow.CommitAsync();
        return affected > 0;
    }
    catch { await _uow.RollbackAsync(); throw; }
}


}
