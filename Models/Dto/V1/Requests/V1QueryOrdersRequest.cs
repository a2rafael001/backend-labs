namespace Models.Dto.V1.Requests;

public sealed class V1QueryOrdersRequest
{
    public List<long>? Ids { get; set; }
    public List<long>? CustomerIds { get; set; }
    public bool IncludeOrderItems { get; set; } = false;
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 50;
}
