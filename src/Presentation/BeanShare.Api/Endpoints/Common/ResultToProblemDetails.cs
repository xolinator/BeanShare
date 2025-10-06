using System.Net;
using BeanShare.Application.Common;
using FastEndpoints;

namespace BeanShare.Api.Endpoints.Common;

/// <summary>
/// Helper for translating Result/Result&lt;T&gt; to HTTP responses with correct status codes.
/// Maps domain error codes to appropriate HTTP status codes following REST conventions.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Adds errors from a Result to FastEndpoints error collection and sends error response.
    /// Automatically maps error codes to appropriate HTTP status codes.
    /// </summary>
    public static async Task SendResultErrorsAsync<TRequest, TResponse>(
        this Endpoint<TRequest, TResponse> endpoint,
        Result result,
        CancellationToken ct = default)
        where TRequest : notnull
    {
        if (!result.IsFailure)
            return;

        foreach (var error in result.Errors)
        {
            endpoint.AddError(error.Code, error.Message);
        }

        var statusCode = GetStatusCode(result.Errors);
        await endpoint.HttpContext.Response.SendErrorsAsync(endpoint.ValidationFailures, statusCode, cancellation: ct);
    }

    /// <summary>
    /// Adds errors from a Result to FastEndpoints error collection and sends error response.
    /// Automatically maps error codes to appropriate HTTP status codes.
    /// </summary>
    public static async Task SendResultErrorsAsync<TRequest, TResponse, TValue>(
        this Endpoint<TRequest, TResponse> endpoint,
        Result<TValue> result,
        CancellationToken ct = default)
        where TRequest : notnull
    {
        if (!result.IsFailure)
            return;

        foreach (var error in result.Errors)
        {
            endpoint.AddError(error.Code, error.Message);
        }

        var statusCode = GetStatusCode(result.Errors);
        await endpoint.HttpContext.Response.SendErrorsAsync(endpoint.ValidationFailures, statusCode, cancellation: ct);
    }

    /// <summary>
    /// Gets the appropriate HTTP status code for a collection of errors.
    /// Uses the first error to determine status code.
    /// </summary>
    public static int GetStatusCode(IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
            return (int)HttpStatusCode.BadRequest;

        return GetStatusCode(errors[0]);
    }

    /// <summary>
    /// Maps an error code to the appropriate HTTP status code.
    /// </summary>
    public static int GetStatusCode(Error error)
    {
        return error.Code switch
        {
            "auth.unauthenticated" => (int)HttpStatusCode.Unauthorized,
            "auth.not_member" => (int)HttpStatusCode.Forbidden,
            "auth.not_admin" => (int)HttpStatusCode.Forbidden,

            "auth.space_not_found" => (int)HttpStatusCode.NotFound,
            "stock.not_found" => (int)HttpStatusCode.NotFound,
            "stock.product_not_found" => (int)HttpStatusCode.NotFound,
            "space.not_found" => (int)HttpStatusCode.NotFound,
            "user.not_found" => (int)HttpStatusCode.NotFound,

            "stock.insufficient" => (int)HttpStatusCode.Conflict,
            "stock.currency_conflict" => (int)HttpStatusCode.Conflict,
            "space.duplicate_invite_code" => (int)HttpStatusCode.Conflict,
            "space.already_member" => (int)HttpStatusCode.Conflict,

            var code when code.StartsWith("validation.") => (int)HttpStatusCode.BadRequest,
            var code when code.StartsWith("invariant.") => (int)HttpStatusCode.BadRequest,

            _ => (int)HttpStatusCode.BadRequest
        };
    }
}