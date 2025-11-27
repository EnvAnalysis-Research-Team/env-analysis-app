using System.Collections.Generic;
using System.Linq;

namespace env_analysis_project.Models
{
    /// <summary>
    /// Standard envelope used by JSON endpoints so the frontend can handle responses uniformly.
    /// </summary>
    /// <typeparam name="T">Type of the payload.</typeparam>
    public sealed record ApiResponse<T>
    {
        public bool Success { get; init; }

        public string? Message { get; init; }

        public T? Data { get; init; }

        public IReadOnlyCollection<string>? Errors { get; init; }

        public static ApiResponse<T> SuccessResponse(T? data, string? message = null) =>
            new()
            {
                Success = true,
                Message = message,
                Data = data
            };

        public static ApiResponse<T> FailureResponse(string message, IEnumerable<string>? errors = null) =>
            new()
            {
                Success = false,
                Message = message,
                Errors = errors?.ToArray()
            };
    }

    public static class ApiResponse
    {
        public static ApiResponse<T> Success<T>(T? data, string? message = null) =>
            ApiResponse<T>.SuccessResponse(data, message);

        public static ApiResponse<T> Fail<T>(string message, IEnumerable<string>? errors = null) =>
            ApiResponse<T>.FailureResponse(message, errors);

        public static ApiResponse<object?> Fail(string message, IEnumerable<string>? errors = null) =>
            ApiResponse<object?>.FailureResponse(message, errors);
    }
}
