using System;
using System.Collections.Generic;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record Project
{
	public int Id { get; init; }
	public int JiraProjectId { get; init; }
	public string Name { get; init; } = string.Empty;
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

	public ICollection<Version> Versions { get; init; } = [];
	public ICollection<TestCycle> TestCycles { get; init; } = [];
	public ICollection<TestCase> TestCases { get; init; } = [];
}
