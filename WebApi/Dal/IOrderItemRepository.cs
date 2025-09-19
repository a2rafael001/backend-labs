using WebApi.Dal.Models;

namespace WebApi.Dal;

public interface IOrderItemRepository
{
    Task BulkInsertAsync(IEnumerable<V1OrderItemDal> items, CancellationToken ct);
    Task<IReadOnlyList<V1OrderItemDal>> QueryByOrderIdsAsync(IEnumerable<long> orderIds, CancellationToken ct);
}
