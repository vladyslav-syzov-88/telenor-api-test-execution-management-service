using System.Collections.Generic;

namespace Telenor.Api.TestExecutionManagement.Import.Models;

public record ImportRequest(
	string ProjectId,
	string? VersionId = null,
	string? CycleName = null);

public record ImportResult(
	int ProjectsImported,
	int VersionsImported,
	int CyclesImported,
	int FoldersImported,
	int TestCasesImported,
	int ExecutionsImported,
	List<string> Warnings);