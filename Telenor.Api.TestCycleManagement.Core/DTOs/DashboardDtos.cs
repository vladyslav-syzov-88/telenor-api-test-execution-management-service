namespace Telenor.Api.TestExecutionManagement.Core.DTOs;

public record CycleSummaryResponse(
	string CycleId,
	string CycleName,
	string CycleStatus,
	int TotalExecutions,
	int Passed,
	int Failed,
	int Blocked,
	int WIP,
	int UnExecuted,
	int PartiallyPassed,
	int FailedWithIssue,
	List<FolderSummary>? Folders = null);

public record FolderSummary(
	string FolderId,
	string FolderName,
	int TotalExecutions,
	int Passed,
	int Failed,
	int Blocked,
	int WIP,
	int UnExecuted);

public record VersionSummaryResponse(
	int VersionId,
	string VersionName,
	bool IsReleased,
	DateTime? ReleaseDate,
	List<CycleSummaryResponse> Cycles);