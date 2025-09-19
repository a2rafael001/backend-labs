namespace Models.Dto.V1.Requests;

public sealed class V1CreateOrderRequest
{
    public List<OrderUnit> Orders { get; set; } = new();
}

public sealed class OrderUnit
{
    public long CustomerId { get; set; }
    public long TotalPriceCents { get; set; }
    public string TotalPriceCurrency { get; set; } = "RUB";
    public List<OrderItemUnit> Items { get; set; } = new();
}

public sealed class OrderItemUnit
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public long PriceCents { get; set; }
    public string PriceCurrency { get; set; } = "RUB";
    public int Quantity { get; set; }
}
