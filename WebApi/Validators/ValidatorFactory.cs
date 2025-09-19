using FluentValidation;

namespace WebApi.Validators;

public sealed class ValidatorFactory
{
    private readonly IServiceProvider _sp;
    public ValidatorFactory(IServiceProvider sp) => _sp = sp;

    public IValidator<T>? GetValidator<T>() => _sp.GetService<IValidator<T>>();
}

