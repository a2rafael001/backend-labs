namespace WebApi.Dal.Models;

public sealed class V1OrderItemDal
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public long PriceCents { get; set; }
    public string PriceCurrency { get; set; } = "RUB";
    public int Quantity { get; set; }
}
