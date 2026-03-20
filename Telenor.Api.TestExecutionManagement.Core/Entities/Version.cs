using System;
using System.Collections.Generic;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class Version
{
	public int Id { get; set; }
	public int ProjectId { get; set; }
	public int? JiraVersionId { get; set; }
	public string Name { get; set; } = string.Empty;
	public bool IsReleased { get; set; }
	public DateTime? ReleaseDate { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public Project Project { get; set; } = null!;
	public ICollection<TestCycle> TestCycles { get; set; } = [];
}