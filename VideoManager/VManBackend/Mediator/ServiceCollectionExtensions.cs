namespace VManBackend.Mediator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();
        return services;
    }

    public static IServiceCollection AddRequestHandler<THandler, TRequest, TResponse>(this IServiceCollection services)
        where THandler : class, IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>();
        return services;
    }
}
