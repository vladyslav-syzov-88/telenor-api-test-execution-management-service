namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class TestCase
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public int ProjectId { get; set; }
	public string JiraIssueKey { get; set; } = string.Empty;
	public int? JiraIssueId { get; set; }
	public string Summary { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public Project Project { get; set; } = null!;
	public ICollection<TestExecution> Executions { get; set; } = [];
}