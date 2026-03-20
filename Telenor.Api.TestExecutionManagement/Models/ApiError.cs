namespace Telenor.Api.TestExecutionManagement.Models;

public record ApiError(string Code, string Reason, string? Message = null);