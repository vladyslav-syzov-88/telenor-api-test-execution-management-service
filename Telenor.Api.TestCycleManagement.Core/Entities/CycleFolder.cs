namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class CycleFolder
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string CycleId { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public int SortOrder { get; set; }

	public TestCycle Cycle { get; set; } = null!;
	public ICollection<TestExecution> Executions { get; set; } = [];
}