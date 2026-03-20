using System;
using System.Collections.Generic;
using Telenor.Api.TestExecutionManagement.Core.Enums;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class TestExecution
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string CycleId { get; set; } = string.Empty;
	public string FolderId { get; set; } = string.Empty;
	public string TestCaseId { get; set; } = string.Empty;
	public int VersionId { get; set; }
	public int ProjectId { get; set; }
	public ExecutionStatus StatusId { get; set; } = ExecutionStatus.UnExecuted;
	public string? AssignedTo { get; set; }
	public string? Comment { get; set; }
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public TestCycle Cycle { get; set; } = null!;
	public CycleFolder Folder { get; set; } = null!;
	public TestCase TestCase { get; set; } = null!;
	public Version Version { get; set; } = null!;
	public Project Project { get; set; } = null!;
	public ICollection<ExecutionHistory> History { get; set; } = [];
}