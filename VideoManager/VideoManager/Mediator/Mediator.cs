namespace VideoManager.Mediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        
        var handler = serviceProvider.GetService(handlerType) 
            ?? throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle)) 
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}");

        var result = handleMethod.Invoke(handler, [request, cancellationToken]);
        
        if (result is Task<TResponse> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for {requestType.Name} did not return a Task<{typeof(TResponse).Name}>");
    }
}
