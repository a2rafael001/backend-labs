using FluentValidation;
using Models.Dto.V1.Requests;

namespace WebApi.Validators;

public sealed class V1CreateOrderRequestValidator : AbstractValidator<V1CreateOrderRequest>
{
    public V1CreateOrderRequestValidator()
    {
        RuleFor(x => x.Orders)
            .NotNull().WithMessage("Orders must be provided")
            .NotEmpty().WithMessage("Orders cannot be empty");

        RuleForEach(x => x.Orders).SetValidator(new OrderUnitValidator());
    }
}

public sealed class OrderUnitValidator : AbstractValidator<OrderUnit>
{
    public OrderUnitValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.TotalPriceCents).GreaterThan(0);
        RuleFor(x => x.TotalPriceCurrency).NotEmpty();

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items must be provided")
            .NotEmpty().WithMessage("Items cannot be empty");

        RuleForEach(x => x.Items).SetValidator(new OrderItemUnitValidator());

        // Бизнес-правило: сумма позиций == TotalPriceCents
        RuleFor(x => x).Must(o =>
        {
            long sum = o.Items?.Sum(i => (long)i.PriceCents * i.Quantity) ?? 0;
            return sum == o.TotalPriceCents;
        }).WithMessage("TotalPriceCents must equal the sum of item prices × quantity");

        // Бизнес-правило: все валюты позиций совпадают с валютой заказа
        RuleFor(x => x).Must(o =>
        {
            if (o.Items == null || o.Items.Count == 0) return true;
            return o.Items.All(i => string.Equals(i.PriceCurrency, o.TotalPriceCurrency, StringComparison.OrdinalIgnoreCase));
        }).WithMessage("All item PriceCurrency must match TotalPriceCurrency");
    }
}

public sealed class OrderItemUnitValidator : AbstractValidator<OrderItemUnit>
{
    public OrderItemUnitValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.PriceCents).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceCurrency).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
