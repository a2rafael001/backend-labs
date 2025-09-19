using WebApi.Dal.Models;

namespace WebApi.Dal;

public interface IOrderRepository
{
    Task<long> InsertAsync(V1OrderDal order, CancellationToken ct);

    Task<IReadOnlyList<V1OrderDal>> QueryAsync(
        IReadOnlyCollection<long>? ids,
        IReadOnlyCollection<long>? customerIds,
        int page, int pageSize,
        CancellationToken ct);

    Task<int> CountAsync(
        IReadOnlyCollection<long>? ids,
        IReadOnlyCollection<long>? customerIds,
        CancellationToken ct);

    Task<V1OrderDal?> GetByIdAsync(long id, CancellationToken ct);
Task<int> UpdateAsync(long id, long totalPriceCents, string totalPriceCurrency, CancellationToken ct);

Task<int> DeleteAsync(long id, CancellationToken ct);

}
