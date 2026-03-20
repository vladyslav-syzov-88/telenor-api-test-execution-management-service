using System;
using System.Collections.Generic;
using Telenor.Api.TestExecutionManagement.Core.Enums;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record TestCycle
{
	public string Id { get; init; } = Guid.NewGuid().ToString();
	public int ProjectId { get; init; }
	public int VersionId { get; init; }
	public string Name { get; init; } = string.Empty;
	public CycleStatus Status { get; init; } = CycleStatus.Draft;
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
	public DateTime? StartDate { get; init; }
	public DateTime? EndDate { get; init; }

	public Project Project { get; init; } = null!;
	public Version Version { get; init; } = null!;
	public ICollection<CycleFolder> Folders { get; init; } = [];
	public ICollection<TestExecution> Executions { get; init; } = [];
}
