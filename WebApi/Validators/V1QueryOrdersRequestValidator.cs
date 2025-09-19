using FluentValidation;
using Models.Dto.V1.Requests;

namespace WebApi.Validators;

public sealed class V1QueryOrdersRequestValidator : AbstractValidator<V1QueryOrdersRequest>
{
    public V1QueryOrdersRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 1000);

        RuleForEach(x => x.Ids!).GreaterThan(0)
            .When(x => x.Ids != null && x.Ids.Count > 0);

        RuleForEach(x => x.CustomerIds!).GreaterThan(0)
            .When(x => x.CustomerIds != null && x.CustomerIds.Count > 0);
    }
}
