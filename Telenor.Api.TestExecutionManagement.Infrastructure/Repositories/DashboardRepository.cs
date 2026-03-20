using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Enums;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

public class DashboardRepository(AppDbContext db) : IDashboardRepository
{
	public async Task<CycleSummaryResponse?> GetCycleSummaryAsync(string cycleId, CancellationToken ct = default)
	{
		var cycle = await db.TestCycles.FindAsync([cycleId], ct);
		if (cycle is null) return null;

		var executions = await db.TestExecutions
			.Where(x => x.CycleId == cycleId)
			.GroupBy(x => x.StatusId)
			.Select(g => new { Status = g.Key, Count = g.Count() })
			.ToListAsync(ct);

		var folders = await db.TestExecutions
			.Where(x => x.CycleId == cycleId)
			.GroupBy(x => new { x.FolderId, x.Folder.Name })
			.Select(g => new FolderSummary(
				g.Key.FolderId,
				g.Key.Name,
				g.Count(),
				g.Count(x => x.StatusId == ExecutionStatus.Pass),
				g.Count(x => x.StatusId == ExecutionStatus.Fail),
				g.Count(x => x.StatusId == ExecutionStatus.Blocked),
				g.Count(x => x.StatusId == ExecutionStatus.WIP),
				g.Count(x => x.StatusId == ExecutionStatus.UnExecuted)))
			.ToListAsync(ct);

		var statusCounts = executions.ToDictionary(e => e.Status, e => e.Count);
		int Count(ExecutionStatus s) => statusCounts.GetValueOrDefault(s, 0);

		return new CycleSummaryResponse(
			cycleId,
			cycle.Name,
			cycle.Status.ToString(),
			executions.Sum(e => e.Count),
			Count(ExecutionStatus.Pass),
			Count(ExecutionStatus.Fail),
			Count(ExecutionStatus.Blocked),
			Count(ExecutionStatus.WIP),
			Count(ExecutionStatus.UnExecuted),
			Count(ExecutionStatus.PartiallyPassed),
			Count(ExecutionStatus.FailedWithIssue),
			folders);
	}

	public async Task<VersionSummaryResponse?> GetVersionSummaryAsync(int versionId, CancellationToken ct = default)
	{
		var version = await db.Versions.FindAsync([versionId], ct);
		if (version is null) return null;

		var cycleIds = await db.TestCycles
			.Where(c => c.VersionId == versionId)
			.Select(c => c.Id)
			.ToListAsync(ct);

		var cycles = new List<CycleSummaryResponse>();
		foreach (var cycleId in cycleIds)
		{
			var summary = await GetCycleSummaryAsync(cycleId, ct);
			if (summary is not null) cycles.Add(summary);
		}

		return new VersionSummaryResponse(version.Id, version.Name, version.IsReleased, version.ReleaseDate, cycles);
	}

	public async Task<List<VersionSummaryResponse>> GetVersionsOverviewAsync(int projectId, CancellationToken ct = default)
	{
		var versions = await db.Versions
			.Where(v => v.ProjectId == projectId)
			.OrderByDescending(v => v.CreatedAt)
			.ToListAsync(ct);

		var result = new List<VersionSummaryResponse>();
		foreach (var version in versions)
		{
			var summary = await GetVersionSummaryAsync(version.Id, ct);
			if (summary is not null) result.Add(summary);
		}

		return result;
	}
}