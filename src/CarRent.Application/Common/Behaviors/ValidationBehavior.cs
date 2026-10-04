using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CarRent.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly PropertyInfo? PayloadProperty = typeof(TRequest).GetProperty("Request");

    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        IServiceProvider serviceProvider)
    {
        _validators = validators;
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        List<ValidationFailure> failures = new();

        if (_validators.Any())
        {
            ValidationContext<TRequest> context = new(request);
            ValidationResult[] results = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            failures.AddRange(results.SelectMany(r => r.Errors).Where(f => f != null));
        }

        object? payload = PayloadProperty?.GetValue(request);
        if (payload is not null)
        {
            Type validatorType = typeof(IValidator<>).MakeGenericType(payload.GetType());
            IEnumerable<IValidator> payloadValidators =
                _serviceProvider.GetServices(validatorType).Cast<IValidator>();

            ValidationContext<object> payloadContext = new(payload);
            foreach (IValidator validator in payloadValidators)
            {
                ValidationResult result = await validator.ValidateAsync(payloadContext, cancellationToken);
                failures.AddRange(result.Errors);
            }
        }

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}