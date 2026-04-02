using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FinShark.Application.Services;

/// <summary>
/// Simple dispatcher implementation for MediatorFlow.Core
/// </summary>
public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<object?> Dispatch(object request, IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = requestType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
            .GetGenericArguments()[0];

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        // Create a scope to resolve scoped services
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for request type {requestType}");
        }

        var method = handlerType.GetMethod("Handle");
        var task = (Task)method!.Invoke(handler, new object[] { request, cancellationToken })!;
        await task;

        // Get the result from the completed task
        var resultProperty = task.GetType().GetProperty("Result");
        var result = resultProperty!.GetValue(task);

        return result!;
    }
}