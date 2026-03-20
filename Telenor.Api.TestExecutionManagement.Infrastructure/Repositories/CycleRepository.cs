using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Enums;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

public class CycleRepository(AppDbContext db) : ICycleRepository
{
	public async Task<List<CycleResponse>> GetCyclesAsync(int projectId, int? versionId, CancellationToken ct = default)
	{
		var query = db.TestCycles.Where(c => c.ProjectId == projectId);
		if (versionId.HasValue)
			query = query.Where(c => c.VersionId == versionId.Value);

		return await query
			.OrderByDescending(c => c.CreatedAt)
			.Select(c => ToResponse(c))
			.ToListAsync(ct);
	}

	public async Task<CycleResponse?> GetCycleByIdAsync(string id, CancellationToken ct = default)
	{
		var cycle = await db.TestCycles.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
		return cycle is null ? null : ToResponse(cycle);
	}

	public async Task<CycleResponse> CreateCycleAsync(CreateCycleRequest request, CancellationToken ct = default)
	{
		var cycle = new TestCycle
		{
			Name = request.Name,
			ProjectId = request.ProjectId,
			VersionId = request.VersionId,
			Status = request.Status,
			StartDate = request.StartDate,
			EndDate = request.EndDate
		};
		db.TestCycles.Add(cycle);
		await db.SaveChangesAsync(ct);
		return ToResponse(cycle);
	}

	public async Task<CycleResponse?> UpdateCycleAsync(string id, UpdateCycleRequest request, CancellationToken ct = default)
	{
		var cycle = await db.TestCycles.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
		if (cycle is null) return null;

		var updated = cycle with
		{
			Name = request.Name ?? cycle.Name,
			Status = request.Status ?? cycle.Status,
			StartDate = request.StartDate ?? cycle.StartDate,
			EndDate = request.EndDate ?? cycle.EndDate
		};

		db.TestCycles.Update(updated);
		await db.SaveChangesAsync(ct);
		return ToResponse(updated);
	}

	public async Task<bool> DeleteCycleAsync(string id, CancellationToken ct = default)
	{
		return await db.TestCycles.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0;
	}

	public async Task<CycleResponse?> CloneCycleAsync(string cycleId, int targetVersionId, CancellationToken ct = default)
	{
		var source = await db.TestCycles
			.AsNoTracking()
			.Include(c => c.Folders)
			.ThenInclude(f => f.Executions)
			.FirstOrDefaultAsync(c => c.Id == cycleId, ct);

		if (source is null) return null;

		var clone = new TestCycle
		{
			Name = $"{source.Name} (Clone)",
			ProjectId = source.ProjectId,
			VersionId = targetVersionId,
			Status = CycleStatus.Draft
		};

		foreach (var folder in source.Folders.OrderBy(f => f.SortOrder))
		{
			var clonedFolder = new CycleFolder
			{
				Name = folder.Name,
				SortOrder = folder.SortOrder
			};

			foreach (var exec in folder.Executions)
			{
				clonedFolder.Executions.Add(new TestExecution
				{
					TestCaseId = exec.TestCaseId,
					VersionId = targetVersionId,
					ProjectId = exec.ProjectId,
					StatusId = ExecutionStatus.UnExecuted
				});
			}

			clone.Folders.Add(clonedFolder);
		}

		db.TestCycles.Add(clone);
		await db.SaveChangesAsync(ct);
		return ToResponse(clone);
	}

	private static CycleResponse ToResponse(TestCycle c) =>
		new(c.Id, c.Name, c.ProjectId, c.VersionId, c.Status.ToString(), c.CreatedAt, c.StartDate, c.EndDate);
}
