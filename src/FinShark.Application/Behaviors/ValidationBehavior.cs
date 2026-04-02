using FluentValidation;
using MediatorFlow.Core.Contracts;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Behaviors;

/// <summary>
/// MediatorFlow pipeline behavior for request validation.
/// Ensures all validators run before handlers execute.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count > 0)
            {
                _logger.LogWarning("Validation failed for {RequestType} with {ErrorCount} errors: {Errors}",
                    typeof(TRequest).Name, failures.Count, string.Join("; ", failures.Select(f => f.ErrorMessage)));
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
