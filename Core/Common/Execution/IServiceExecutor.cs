namespace Core.Common.Execution;

public interface IServiceExecutor
{
    Task<ServiceResult<TResponse>> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<ServiceResult<TResponse>>> serviceMethod,
        CancellationToken cancellationToken);

    Task<ServiceResult<TResponse>> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        Func<
            TRequest,
            CancellationToken,
            Task<ServiceResult<TResponse>>> serviceMethod,
        CancellationToken cancellationToken);

    Task<ServiceResult> ExecuteAsync(
        Func<CancellationToken, Task<ServiceResult>> serviceMethod,
        CancellationToken cancellationToken);

    Task<ServiceResult> ExecuteAsync<TRequest>(
        TRequest request,
        Func<
            TRequest,
            CancellationToken,
            Task<ServiceResult>> serviceMethod,
        CancellationToken cancellationToken);
}