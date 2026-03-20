using System;

namespace Telenor.Api.TestExecutionManagement.Core.DTOs;

public record TestCaseResponse(string Id, int ProjectId, string JiraIssueKey, int? JiraIssueId, string Summary, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateTestCaseRequest(int ProjectId, string JiraIssueKey, string Summary, int? JiraIssueId = null);

public record UpdateTestCaseRequest(string? JiraIssueKey = null, string? Summary = null, int? JiraIssueId = null);