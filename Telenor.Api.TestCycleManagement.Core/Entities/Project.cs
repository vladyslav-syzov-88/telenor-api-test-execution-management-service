namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class Project
{
	public int Id { get; set; }
	public int JiraProjectId { get; set; }
	public string Name { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public ICollection<Version> Versions { get; set; } = [];
	public ICollection<TestCycle> TestCycles { get; set; } = [];
	public ICollection<TestCase> TestCases { get; set; } = [];
}