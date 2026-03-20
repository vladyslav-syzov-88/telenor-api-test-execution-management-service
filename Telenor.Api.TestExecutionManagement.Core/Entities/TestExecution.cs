using System;
using System.Collections.Generic;
using Telenor.Api.TestExecutionManagement.Core.Enums;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record TestExecution
{
	public string Id { get; init; } = Guid.NewGuid().ToString();
	public string CycleId { get; init; } = string.Empty;
	public string FolderId { get; init; } = string.Empty;
	public string TestCaseId { get; init; } = string.Empty;
	public int VersionId { get; init; }
	public int ProjectId { get; init; }
	public ExecutionStatus StatusId { get; init; } = ExecutionStatus.UnExecuted;
	public string? AssignedTo { get; init; }
	public string? Comment { get; init; }
	public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

	public TestCycle Cycle { get; init; } = null!;
	public CycleFolder Folder { get; init; } = null!;
	public TestCase TestCase { get; init; } = null!;
	public Version Version { get; init; } = null!;
	public Project Project { get; init; } = null!;
	public ICollection<ExecutionHistory> History { get; init; } = [];
}
