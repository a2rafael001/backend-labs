namespace Models.Dto.V1.Requests;

public sealed class V1UpdateOrderRequest
{
    public long   TotalPriceCents    { get; set; }
    public string TotalPriceCurrency { get; set; } = default!;
}
