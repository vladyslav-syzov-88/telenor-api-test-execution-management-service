using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Telenor.Api.TestExecutionManagement.Import.Models;

public class ZephyrCycle
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;
}

public class ZephyrFolder
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("cycleId")]
	public string CycleId { get; set; } = string.Empty;

	[JsonPropertyName("versionId")]
	public int VersionId { get; set; }

	[JsonPropertyName("projectId")]
	public int ProjectId { get; set; }
}

public class ZephyrExecutionStatus
{
	[JsonPropertyName("id")]
	public int Id { get; set; }
}

public class ZephyrExecutionDetails
{
	[JsonPropertyName("status")]
	public ZephyrExecutionStatus Status { get; set; } = new();

	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("projectId")]
	public int ProjectId { get; set; }

	[JsonPropertyName("issueId")]
	public int IssueId { get; set; }

	[JsonPropertyName("cycleId")]
	public string CycleId { get; set; } = string.Empty;

	[JsonPropertyName("versionId")]
	public int VersionId { get; set; }

	[JsonPropertyName("comment")]
	public string? Comment { get; set; }
}

public class ZephyrTestExecution
{
	[JsonPropertyName("issueSummary")]
	public string IssueSummary { get; set; } = string.Empty;

	[JsonPropertyName("issueKey")]
	public string IssueKey { get; set; } = string.Empty;

	[JsonPropertyName("execution")]
	public ZephyrExecutionDetails Execution { get; set; } = new();

	[JsonPropertyName("assigneeType")]
	public string? AssigneeType { get; set; }
}

public class ZephyrSearchResult
{
	[JsonPropertyName("searchObjectList")]
	public List<ZephyrTestExecution> SearchObjectList { get; set; } = [];

	[JsonPropertyName("totalCount")]
	public int TotalCount { get; set; }
}

public class ZephyrExecutionsSearchResult
{
	[JsonPropertyName("searchResult")]
	public ZephyrSearchResult SearchResult { get; set; } = new();
}

public class JiraVersion
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("released")]
	public bool Released { get; set; }

	[JsonPropertyName("releaseDate")]
	public string? ReleaseDate { get; set; }

	[JsonPropertyName("projectId")]
	public long ProjectId { get; set; }
}