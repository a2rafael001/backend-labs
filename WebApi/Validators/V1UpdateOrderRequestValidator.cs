using FluentValidation;
using Models.Dto.V1.Requests;

namespace WebApi.Validators;

public sealed class V1UpdateOrderRequestValidator : AbstractValidator<V1UpdateOrderRequest>
{
    public V1UpdateOrderRequestValidator()
    {
        RuleFor(x => x.TotalPriceCents).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalPriceCurrency).NotEmpty().MaximumLength(10);
    }
}
