using Telenor.Api.TestExecutionManagement.Core.Enums;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class TestCycle
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public int ProjectId { get; set; }
	public int VersionId { get; set; }
	public string Name { get; set; } = string.Empty;
	public CycleStatus Status { get; set; } = CycleStatus.Draft;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public Project Project { get; set; } = null!;
	public Version Version { get; set; } = null!;
	public ICollection<CycleFolder> Folders { get; set; } = [];
	public ICollection<TestExecution> Executions { get; set; } = [];
}