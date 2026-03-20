using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Telenor.Api.TestExecutionManagement.Import.Models;

public record ZephyrCycle
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;
}

public record ZephyrFolder
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName("cycleId")]
	public string CycleId { get; init; } = string.Empty;

	[JsonPropertyName("versionId")]
	public int VersionId { get; init; }

	[JsonPropertyName("projectId")]
	public int ProjectId { get; init; }
}

public record ZephyrExecutionStatus
{
	[JsonPropertyName("id")]
	public int Id { get; init; }
}

public record ZephyrExecutionDetails
{
	[JsonPropertyName("status")]
	public ZephyrExecutionStatus Status { get; init; } = new();

	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	[JsonPropertyName("projectId")]
	public int ProjectId { get; init; }

	[JsonPropertyName("issueId")]
	public int IssueId { get; init; }

	[JsonPropertyName("cycleId")]
	public string CycleId { get; init; } = string.Empty;

	[JsonPropertyName("versionId")]
	public int VersionId { get; init; }

	[JsonPropertyName("comment")]
	public string? Comment { get; init; }
}

public record ZephyrTestExecution
{
	[JsonPropertyName("issueSummary")]
	public string IssueSummary { get; init; } = string.Empty;

	[JsonPropertyName("issueKey")]
	public string IssueKey { get; init; } = string.Empty;

	[JsonPropertyName("execution")]
	public ZephyrExecutionDetails Execution { get; init; } = new();

	[JsonPropertyName("assigneeType")]
	public string? AssigneeType { get; init; }
}

public record ZephyrSearchResult
{
	[JsonPropertyName("searchObjectList")]
	public List<ZephyrTestExecution> SearchObjectList { get; init; } = [];

	[JsonPropertyName("totalCount")]
	public int TotalCount { get; init; }
}

public record ZephyrExecutionsSearchResult
{
	[JsonPropertyName("searchResult")]
	public ZephyrSearchResult SearchResult { get; init; } = new();
}

public record JiraVersion
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName("released")]
	public bool Released { get; init; }

	[JsonPropertyName("releaseDate")]
	public string? ReleaseDate { get; init; }

	[JsonPropertyName("projectId")]
	public long ProjectId { get; init; }
}
