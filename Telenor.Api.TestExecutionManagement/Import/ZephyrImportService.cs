using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Enums;
using Telenor.Api.TestExecutionManagement.Import.Models;
using Telenor.Api.TestExecutionManagement.Import.ZephyrClient;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;
using Version = Telenor.Api.TestExecutionManagement.Core.Entities.Version;

namespace Telenor.Api.TestExecutionManagement.Import;

public class ZephyrImportService(ZephyrApiClient zephyrClient, AppDbContext db, ILogger<ZephyrImportService> logger)
{
	public async Task<ImportResult> ImportAsync(ImportRequest request, CancellationToken ct)
	{
		// Fail fast if credentials are not configured
		zephyrClient.ValidateConfiguration();

		var warnings = new List<string>();
		var counters = new ImportCounters();

		var jiraProjectId = int.Parse(request.ProjectId);

		// 1. Ensure project exists
		var project = await db.Projects.FirstOrDefaultAsync(p => p.JiraProjectId == jiraProjectId, ct);
		if (project is null)
		{
			project = new Project { JiraProjectId = jiraProjectId, Name = $"Project {jiraProjectId}" };
			db.Projects.Add(project);
			await db.SaveChangesAsync(ct);
			counters.Projects++;
			logger.LogInformation("Created project {ProjectId} (Jira: {JiraProjectId})", project.Id, jiraProjectId);
		}

		// 2. Import Jira versions or use specified versionId
		var versionIds = new List<string>();
		if (!string.IsNullOrEmpty(request.VersionId))
		{
			versionIds.Add(request.VersionId);
			await EnsureVersionAsync(project, request.VersionId, null, counters, ct);
		}
		else
		{
			var jiraVersions = await TryGetJiraVersionsAsync(request.ProjectId, warnings, ct);
			foreach (var jv in jiraVersions)
			{
				versionIds.Add(jv.Id);
				DateTime? releaseDate = null;
				if (DateTime.TryParse(jv.ReleaseDate, out var parsed))
					releaseDate = parsed;

				await EnsureVersionAsync(project, jv.Id, new VersionInfo(jv.Name, jv.Released, releaseDate), counters, ct);
			}
		}

		// 3. For each version, import cycles → folders → executions
		foreach (var versionId in versionIds)
		{
			var cycles = await TryGetCyclesAsync(request.ProjectId, versionId, warnings, ct);

			foreach (var zCycle in cycles)
			{
				if (!string.IsNullOrEmpty(request.CycleName) &&
					!zCycle.Name.Equals(request.CycleName, StringComparison.OrdinalIgnoreCase))
					continue;

				logger.LogInformation("Importing cycle '{CycleName}' (Zephyr ID: {CycleId})", zCycle.Name, zCycle.Id);

				var version = await db.Versions.FirstAsync(
					v => v.ProjectId == project.Id && v.JiraVersionId == int.Parse(versionId), ct);

				var cycle = new TestCycle
				{
					ProjectId = project.Id,
					VersionId = version.Id,
					Name = zCycle.Name,
					Status = CycleStatus.Active
				};
				db.TestCycles.Add(cycle);
				await db.SaveChangesAsync(ct);
				counters.Cycles++;

				// Get folders for this cycle
				var folders = await TryGetFoldersAsync(zCycle.Id, request.ProjectId, versionId, warnings, ct);

				foreach (var zFolder in folders)
				{
					var folder = new CycleFolder
					{
						CycleId = cycle.Id,
						Name = zFolder.Name,
						SortOrder = 0
					};
					db.CycleFolders.Add(folder);
					await db.SaveChangesAsync(ct);
					counters.Folders++;

					// Get executions for this folder
					var executions = await TryGetExecutionsAsync(
						zFolder.Id, zCycle.Id, request.ProjectId, versionId, warnings, ct);

					foreach (var zExec in executions)
					{
						var testCase = await EnsureTestCaseAsync(project, zExec, counters, ct);

						var execution = new TestExecution
						{
							CycleId = cycle.Id,
							FolderId = folder.Id,
							TestCaseId = testCase.Id,
							VersionId = version.Id,
							ProjectId = project.Id,
							StatusId = (ExecutionStatus)zExec.Execution.Status.Id,
							Comment = zExec.Execution.Comment,
							AssignedTo = zExec.AssigneeType
						};
						db.TestExecutions.Add(execution);
						counters.Executions++;
					}

					await db.SaveChangesAsync(ct);
					logger.LogInformation(
						"  Folder '{FolderName}': {Count} executions imported", zFolder.Name, executions.Count);
				}
			}
		}

		return new ImportResult(
			counters.Projects, counters.Versions, counters.Cycles,
			counters.Folders, counters.TestCases, counters.Executions, warnings);
	}

	private async Task EnsureVersionAsync(
		Project project, string jiraVersionId, VersionInfo? info, ImportCounters counters, CancellationToken ct)
	{
		var jiraVId = int.Parse(jiraVersionId);
		var existing = await db.Versions.FirstOrDefaultAsync(
			v => v.ProjectId == project.Id && v.JiraVersionId == jiraVId, ct);

		if (existing is not null) return;

		var version = new Version
		{
			ProjectId = project.Id,
			JiraVersionId = jiraVId,
			Name = info?.Name ?? $"Version {jiraVersionId}",
			IsReleased = info?.IsReleased ?? false,
			ReleaseDate = info?.ReleaseDate
		};
		db.Versions.Add(version);
		await db.SaveChangesAsync(ct);
		counters.Versions++;
		logger.LogInformation("Created version '{Name}' (Jira: {JiraVersionId})", version.Name, jiraVersionId);
	}

	private async Task<TestCase> EnsureTestCaseAsync(
		Project project, ZephyrTestExecution zExec, ImportCounters counters, CancellationToken ct)
	{
		var existing = await db.TestCases.AsNoTracking().FirstOrDefaultAsync(
			t => t.ProjectId == project.Id && t.JiraIssueKey == zExec.IssueKey, ct);

		if (existing is not null)
		{
			if (existing.Summary != zExec.IssueSummary)
			{
				var updated = existing with { Summary = zExec.IssueSummary, UpdatedAt = DateTime.UtcNow };
				db.TestCases.Update(updated);
				return updated;
			}
			return existing;
		}

		var testCase = new TestCase
		{
			ProjectId = project.Id,
			JiraIssueKey = zExec.IssueKey,
			JiraIssueId = zExec.Execution.IssueId,
			Summary = zExec.IssueSummary
		};
		db.TestCases.Add(testCase);
		await db.SaveChangesAsync(ct);
		counters.TestCases++;
		return testCase;
	}

	private async Task<List<JiraVersion>> TryGetJiraVersionsAsync(
		string projectId, List<string> warnings, CancellationToken ct)
	{
		try
		{
			return await zephyrClient.GetJiraVersionsAsync(projectId, ct);
		}
		catch (ImportConfigurationException) { throw; }
		catch (Exception ex)
		{
			warnings.Add($"Failed to fetch Jira versions: {ex.Message}. Supply versionId explicitly.");
			return [];
		}
	}

	private async Task<List<ZephyrCycle>> TryGetCyclesAsync(
		string projectId, string versionId, List<string> warnings, CancellationToken ct)
	{
		try
		{
			return await zephyrClient.GetCyclesAsync(projectId, versionId, ct);
		}
		catch (ImportConfigurationException) { throw; }
		catch (Exception ex)
		{
			warnings.Add($"Failed to fetch cycles for version {versionId}: {ex.Message}");
			return [];
		}
	}

	private async Task<List<ZephyrFolder>> TryGetFoldersAsync(
		string cycleId, string projectId, string versionId, List<string> warnings, CancellationToken ct)
	{
		try
		{
			return await zephyrClient.GetFoldersAsync(cycleId, projectId, versionId, ct);
		}
		catch (ImportConfigurationException) { throw; }
		catch (Exception ex)
		{
			warnings.Add($"Failed to fetch folders for cycle {cycleId}: {ex.Message}");
			return [];
		}
	}

	private async Task<List<ZephyrTestExecution>> TryGetExecutionsAsync(
		string folderId, string cycleId, string projectId, string versionId,
		List<string> warnings, CancellationToken ct)
	{
		try
		{
			return await zephyrClient.GetExecutionsForFolderAsync(folderId, cycleId, projectId, versionId, ct);
		}
		catch (ImportConfigurationException) { throw; }
		catch (Exception ex)
		{
			warnings.Add($"Failed to fetch executions for folder {folderId}: {ex.Message}");
			return [];
		}
	}

	private record VersionInfo(string Name, bool IsReleased, DateTime? ReleaseDate);

	private class ImportCounters
	{
		public int Projects;
		public int Versions;
		public int Cycles;
		public int Folders;
		public int TestCases;
		public int Executions;
	}
}