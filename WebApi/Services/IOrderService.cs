using Models.Dto.V1.Requests;
using Models.Dto.V1.Responses;

namespace WebApi.Services;

public interface IOrderService
{
    Task<V1CreateOrderResponse> BatchCreateAsync(V1CreateOrderRequest request, CancellationToken ct);

    // Вариант c total:
    Task<(IReadOnlyList<OrderView> Orders, int Page, int PageSize, int Total)>
        QueryAsync(V1QueryOrdersRequest request, CancellationToken ct);

        Task<OrderView?> GetByIdAsync(long id, CancellationToken ct);
Task<bool> UpdateAsync(long id, V1UpdateOrderRequest req, CancellationToken ct);
Task<bool> DeleteAsync(long id, CancellationToken ct);

}
