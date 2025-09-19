namespace WebApi.Dal.Models;

public sealed class V1OrderDal
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long TotalPriceCents { get; set; }
    public string TotalPriceCurrency { get; set; } = "RUB";
    public DateTime CreatedAt { get; set; }
}
