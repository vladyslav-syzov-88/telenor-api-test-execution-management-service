using System;
using System.Collections.Generic;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record CycleFolder
{
	public string Id { get; init; } = Guid.NewGuid().ToString();
	public string CycleId { get; init; } = string.Empty;
	public string Name { get; init; } = string.Empty;
	public int SortOrder { get; init; }

	public TestCycle Cycle { get; init; } = null!;
	public ICollection<TestExecution> Executions { get; init; } = [];
}
