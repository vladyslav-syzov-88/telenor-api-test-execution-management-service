using System;
using System.Collections.Generic;

namespace Telenor.Api.TestExecutionManagement.Core.DTOs;

public record ExecutionResponse(
	string Id,
	string IssueKey,
	string IssueSummary,
	ExecutionDetailsResponse Execution);

public record ExecutionDetailsResponse(
	int StatusId,
	string Id,
	int ProjectId,
	string IssueId,
	string CycleId,
	int VersionId,
	string? Comment);

public record UpdateExecutionRequest(int StatusId, string? Comment = null);

public record BulkUpdateExecutionsRequest(string[] ExecutionIds, int StatusId);

public record ExecutionSearchRequest(
	string? CycleName = null,
	string? VersionName = null,
	string[]? FolderNames = null,
	int? ProjectId = null,
	int MaxRecords = 100,
	int Offset = 0);

public record ExecutionSearchResponse(List<ExecutionResponse> Executions, int TotalCount);

public record PaginatedExecutionResponse(List<ExecutionResponse> Executions, int TotalCount, int Offset, int Size);

public record ExecutionHistoryResponse(long Id, string ExecutionId, int StatusId, string? Comment, string ChangedBy, DateTime ChangedAt);