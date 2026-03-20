using System;
using System.Collections.Generic;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record TestCase
{
	public string Id { get; init; } = Guid.NewGuid().ToString();
	public int ProjectId { get; init; }
	public string JiraIssueKey { get; init; } = string.Empty;
	public int? JiraIssueId { get; init; }
	public string Summary { get; init; } = string.Empty;
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

	public Project Project { get; init; } = null!;
	public ICollection<TestExecution> Executions { get; init; } = [];
}
