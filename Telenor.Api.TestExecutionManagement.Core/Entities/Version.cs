using System;
using System.Collections.Generic;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record Version
{
	public int Id { get; init; }
	public int ProjectId { get; init; }
	public int? JiraVersionId { get; init; }
	public string Name { get; init; } = string.Empty;
	public bool IsReleased { get; init; }
	public DateTime? ReleaseDate { get; init; }
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

	public Project Project { get; init; } = null!;
	public ICollection<TestCycle> TestCycles { get; init; } = [];
}
