using Telenor.Api.TestExecutionManagement.Core.Enums;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class ExecutionHistory
{
	public long Id { get; set; }
	public string ExecutionId { get; set; } = string.Empty;
	public ExecutionStatus StatusId { get; set; }
	public string? Comment { get; set; }
	public string ChangedBy { get; set; } = string.Empty;
	public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

	public TestExecution Execution { get; set; } = null!;
}