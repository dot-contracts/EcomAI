namespace EcomVideoAI.Application.Interfaces
{
    public interface IAsyncUseCase<in TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
    }

    public interface IAsyncUseCase<in TRequest>
    {
        Task ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
    }
} 