using BeanShare.Application.Common;
using FluentValidation;
using MediatR;

namespace BeanShare.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
    where TRequest : IRequest<Result<T>>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<Result<T>> Handle(TRequest request, RequestHandlerDelegate<Result<T>> next, CancellationToken ct)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors).Where(e => e is not null);
        var errors = failures.Select(f => Error.ValidationFailure(f.PropertyName, f.ErrorMessage)).ToList();
        return errors.Count == 0 ? await next() : Result<T>.Failure(errors);
    }
}