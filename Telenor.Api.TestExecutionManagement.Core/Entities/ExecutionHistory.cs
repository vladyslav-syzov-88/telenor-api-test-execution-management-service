using System;
using Telenor.Api.TestExecutionManagement.Core.Enums;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record ExecutionHistory
{
	public long Id { get; init; }
	public string ExecutionId { get; init; } = string.Empty;
	public ExecutionStatus StatusId { get; init; }
	public string? Comment { get; init; }
	public string ChangedBy { get; init; } = string.Empty;
	public DateTime ChangedAt { get; init; } = DateTime.UtcNow;

	public TestExecution Execution { get; init; } = null!;
}
