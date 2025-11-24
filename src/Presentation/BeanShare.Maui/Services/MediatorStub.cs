using MediatR;
using System.Diagnostics;

namespace BeanShare.Maui.Services;

/// <summary>
/// Stub implementation of IMediator for MAUI client.
/// Real MediatR operations happen on the server side.
/// This stub prevents DI exceptions in components that expect IMediator.
/// </summary>
public class MediatorStub : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[MediatorStub] Send called with request type: {request.GetType().Name}");
        return default(TResponse)!;
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        Debug.WriteLine($"[MediatorStub] Send called with request type: {request.GetType().Name}");
        await Task.CompletedTask;
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[MediatorStub] Send called with request type: {request.GetType().Name}");
        return await Task.FromResult<object?>(null);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[MediatorStub] CreateStream called with request type: {request.GetType().Name}");
        return AsyncEnumerable.Empty<TResponse>();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[MediatorStub] CreateStream called with request type: {request.GetType().Name}");
        return AsyncEnumerable.Empty<object?>();
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[MediatorStub] Publish called with notification type: {notification.GetType().Name}");
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        Debug.WriteLine($"[MediatorStub] Publish called with notification type: {notification.GetType().Name}");
        return Task.CompletedTask;
    }
}

internal static class AsyncEnumerable
{
    public static async IAsyncEnumerable<T> Empty<T>()
    {
        await Task.CompletedTask;
        yield break;
    }
}