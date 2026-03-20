using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Enums;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

public class ExecutionRepository(AppDbContext db) : IExecutionRepository
{
	public async Task<PaginatedExecutionResponse> GetExecutionsByFolderAsync(string folderId, int offset, int size, CancellationToken ct = default)
	{
		var query = db.TestExecutions
			.Include(x => x.TestCase)
			.Include(x => x.Project)
			.Include(x => x.Version)
			.Where(x => x.FolderId == folderId);

		var total = await query.CountAsync(ct);

		var executions = await query
			.OrderBy(x => x.TestCase.JiraIssueKey)
			.Skip(offset)
			.Take(size)
			.Select(x => ToResponse(x))
			.ToListAsync(ct);

		return new PaginatedExecutionResponse(executions, total, offset, size);
	}

	public async Task<ExecutionSearchResponse> SearchExecutionsAsync(ExecutionSearchRequest request, CancellationToken ct = default)
	{
		var query = db.TestExecutions
			.Include(x => x.TestCase)
			.Include(x => x.Cycle)
			.Include(x => x.Folder)
			.Include(x => x.Project)
			.Include(x => x.Version)
			.AsQueryable();

		if (request.ProjectId.HasValue)
			query = query.Where(x => x.ProjectId == request.ProjectId.Value);

		if (!string.IsNullOrEmpty(request.CycleName))
			query = query.Where(x => x.Cycle.Name == request.CycleName);

		if (!string.IsNullOrEmpty(request.VersionName))
			query = query.Where(x => x.Version.Name == request.VersionName);

		if (request.FolderNames is { Length: > 0 })
			query = query.Where(x => request.FolderNames.Contains(x.Folder.Name));

		var total = await query.CountAsync(ct);

		var executions = await query
			.OrderBy(x => x.TestCase.JiraIssueKey)
			.Skip(request.Offset)
			.Take(request.MaxRecords)
			.Select(x => ToResponse(x))
			.ToListAsync(ct);

		return new ExecutionSearchResponse(executions, total);
	}

	public async Task<ExecutionResponse?> UpdateExecutionAsync(string id, UpdateExecutionRequest request, string changedBy, CancellationToken ct = default)
	{
		var execution = await db.TestExecutions
			.Include(x => x.TestCase)
			.Include(x => x.Project)
			.Include(x => x.Version)
			.FirstOrDefaultAsync(x => x.Id == id, ct);

		if (execution is null) return null;

		execution.StatusId = (ExecutionStatus)request.StatusId;
		execution.Comment = request.Comment ?? execution.Comment;
		execution.UpdatedAt = DateTime.UtcNow;

		db.ExecutionHistory.Add(new ExecutionHistory
		{
			ExecutionId = id,
			StatusId = (ExecutionStatus)request.StatusId,
			Comment = request.Comment,
			ChangedBy = changedBy,
			ChangedAt = DateTime.UtcNow
		});

		await db.SaveChangesAsync(ct);
		return ToResponse(execution);
	}

	public async Task<int> BulkUpdateExecutionsAsync(BulkUpdateExecutionsRequest request, string changedBy, CancellationToken ct = default)
	{
		var executions = await db.TestExecutions
			.Where(x => request.ExecutionIds.Contains(x.Id))
			.ToListAsync(ct);

		var now = DateTime.UtcNow;
		var status = (ExecutionStatus)request.StatusId;

		foreach (var execution in executions)
		{
			execution.StatusId = status;
			execution.UpdatedAt = now;

			db.ExecutionHistory.Add(new ExecutionHistory
			{
				ExecutionId = execution.Id,
				StatusId = status,
				ChangedBy = changedBy,
				ChangedAt = now
			});
		}

		await db.SaveChangesAsync(ct);
		return executions.Count;
	}

	public async Task<List<ExecutionHistoryResponse>> GetExecutionHistoryAsync(string executionId, CancellationToken ct = default)
	{
		return await db.ExecutionHistory
			.Where(h => h.ExecutionId == executionId)
			.OrderByDescending(h => h.ChangedAt)
			.Select(h => new ExecutionHistoryResponse(h.Id, h.ExecutionId, (int)h.StatusId, h.Comment, h.ChangedBy, h.ChangedAt))
			.ToListAsync(ct);
	}

	private static ExecutionResponse ToResponse(TestExecution x) =>
		new(x.Id, x.TestCase.JiraIssueKey, x.TestCase.Summary,
			new ExecutionDetailsResponse((int)x.StatusId, x.Id, x.Project.JiraProjectId, x.TestCaseId, x.CycleId, x.Version.JiraVersionId ?? x.VersionId, x.Comment));
}