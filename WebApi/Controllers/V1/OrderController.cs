using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Models.Dto.V1.Requests;
using Models.Dto.V1.Responses;
using WebApi.Services;
using WebApi.Validators;

namespace WebApi.Controllers.V1;

[ApiController]
[Route("api/v1/order")]
public class OrderController : ControllerBase
{
    private readonly ValidatorFactory _vf;
    private readonly IOrderService _svc;

    public OrderController(ValidatorFactory vf, IOrderService svc)
    {
        _vf = vf;
        _svc = svc;
    }

    // POST /api/v1/order/batch-create
    [HttpPost("batch-create")]
    public async Task<ActionResult<V1CreateOrderResponse>> BatchCreate(
        [FromBody] V1CreateOrderRequest request,
        CancellationToken ct)
    {
        var validator = _vf.GetValidator<V1CreateOrderRequest>();
        if (validator != null)
        {
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid) return BadRequest(result.ToDictionary());
        }

        var resp = await _svc.BatchCreateAsync(request, ct);
        return Ok(resp);
    }

    // POST /api/v1/order/query
    [HttpPost("query")]
    public async Task<ActionResult<object>> Query(
        [FromBody] V1QueryOrdersRequest request,
        CancellationToken ct)
    {
        var validator = _vf.GetValidator<V1QueryOrdersRequest>();
        if (validator != null)
        {
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid) return BadRequest(result.ToDictionary());
        }

        var (orders, page, pageSize, total) = await _svc.QueryAsync(request, ct);
return Ok(new { page, pageSize, total, orders });

    }

    [HttpGet("{id:long}")]
public async Task<IActionResult> GetById(long id, CancellationToken ct)
{
    var order = await _svc.GetByIdAsync(id, ct);
    return order is null ? NotFound() : Ok(order);
}

[HttpPut("{id:long}")]
public async Task<IActionResult> Update(long id, [FromBody] V1UpdateOrderRequest req, CancellationToken ct)
{
    var updated = await _svc.UpdateAsync(id, req, ct);
    return updated ? NoContent() : NotFound();
}

[HttpDelete("{id:long}")]
public async Task<IActionResult> Delete(long id, CancellationToken ct)
{
    var deleted = await _svc.DeleteAsync(id, ct);
    return deleted ? NoContent() : NotFound();
}

}
