using Core.Common;
using Core.Common.Execution;
using Core.Common.Validation;

namespace Application.Common.Execution;

public sealed class ServiceExecutor : IServiceExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceExecutor(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<ServiceResult<TResponse>> ExecuteAsync<TResponse>(
        Func<
            CancellationToken,
            Task<ServiceResult<TResponse>>> serviceMethod,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceMethod);

        cancellationToken.ThrowIfCancellationRequested();

        return serviceMethod(cancellationToken);
    }

    public async Task<ServiceResult<TResponse>>
        ExecuteAsync<TRequest, TResponse>(
            TRequest request,
            Func<
                TRequest,
                CancellationToken,
                Task<ServiceResult<TResponse>>> serviceMethod,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(serviceMethod);

        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = await ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            var errorMessage =
                CreateValidationErrorMessage(
                    validationResult);

            return ServiceResult<TResponse>.BadRequest(
                errorMessage);
        }

        return await serviceMethod(
            request,
            cancellationToken);
    }

    public Task<ServiceResult> ExecuteAsync(
        Func<
            CancellationToken,
            Task<ServiceResult>> serviceMethod,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceMethod);

        cancellationToken.ThrowIfCancellationRequested();

        return serviceMethod(cancellationToken);
    }

    public async Task<ServiceResult> ExecuteAsync<TRequest>(
        TRequest request,
        Func<
            TRequest,
            CancellationToken,
            Task<ServiceResult>> serviceMethod,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(serviceMethod);

        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = await ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            var errorMessage =
                CreateValidationErrorMessage(
                    validationResult);

            return ServiceResult.BadRequest(
                errorMessage);
        }

        return await serviceMethod(
            request,
            cancellationToken);
    }

    private async ValueTask<ValidationResult>
        ValidateAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken)
    {
        var validator = _serviceProvider.GetService(
            typeof(IValidator<TRequest>))
            as IValidator<TRequest>;

        /*
         * Request için validator kayıtlı değilse
         * validasyon başarılı kabul edilir.
         */
        if (validator is null)
        {
            return ValidationResult.Success();
        }

        return await validator.ValidateAsync(
            request,
            cancellationToken);
    }

    private static string CreateValidationErrorMessage(
        ValidationResult validationResult)
    {
        if (validationResult.Errors.Count == 0)
        {
            return "İstek doğrulaması başarısız.";
        }

        return string.Join(
            " | ",
            validationResult.Errors.Select(error =>
                $"{error.PropertyName}: {error.ErrorMessage}"));
    }
}