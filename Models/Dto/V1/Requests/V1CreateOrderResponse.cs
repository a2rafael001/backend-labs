namespace Models.Dto.V1.Responses;

public sealed class V1CreateOrderResponse
{
    public List<OrderView> Orders { get; set; } = new();
}

public sealed class OrderView
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long TotalPriceCents { get; set; }
    public string TotalPriceCurrency { get; set; } = "RUB";
    public DateTime CreatedAt { get; set; }
    public List<OrderItemView>? Items { get; set; }
}

public sealed class OrderItemView
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public long PriceCents { get; set; }
    public string PriceCurrency { get; set; } = "RUB";
    public int Quantity { get; set; }
}
