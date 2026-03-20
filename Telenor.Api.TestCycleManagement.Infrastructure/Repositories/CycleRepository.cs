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
		var cycle = await db.TestCycles.FindAsync([id], ct);
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
		var cycle = await db.TestCycles.FindAsync([id], ct);
		if (cycle is null) return null;

		if (request.Name is not null) cycle.Name = request.Name;
		if (request.Status.HasValue) cycle.Status = request.Status.Value;
		if (request.StartDate.HasValue) cycle.StartDate = request.StartDate.Value;
		if (request.EndDate.HasValue) cycle.EndDate = request.EndDate.Value;

		await db.SaveChangesAsync(ct);
		return ToResponse(cycle);
	}

	public async Task<bool> DeleteCycleAsync(string id, CancellationToken ct = default)
	{
		var cycle = await db.TestCycles.FindAsync([id], ct);
		if (cycle is null) return false;

		db.TestCycles.Remove(cycle);
		await db.SaveChangesAsync(ct);
		return true;
	}

	public async Task<CycleResponse?> CloneCycleAsync(string cycleId, int targetVersionId, CancellationToken ct = default)
	{
		var source = await db.TestCycles
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