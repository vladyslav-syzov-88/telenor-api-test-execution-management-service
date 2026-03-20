using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

public class FolderRepository(AppDbContext db) : IFolderRepository
{
	public async Task<List<FolderResponse>> GetFoldersAsync(string cycleId, CancellationToken ct = default)
	{
		return await db.CycleFolders
			.Where(f => f.CycleId == cycleId)
			.OrderBy(f => f.SortOrder)
			.Select(f => new FolderResponse(f.Id, f.Name, f.CycleId, f.SortOrder))
			.ToListAsync(ct);
	}

	public async Task<FolderResponse> CreateFolderAsync(string cycleId, CreateFolderRequest request, CancellationToken ct = default)
	{
		var folder = new CycleFolder
		{
			CycleId = cycleId,
			Name = request.Name,
			SortOrder = request.SortOrder
		};
		db.CycleFolders.Add(folder);
		await db.SaveChangesAsync(ct);
		return new FolderResponse(folder.Id, folder.Name, folder.CycleId, folder.SortOrder);
	}

	public async Task<FolderResponse?> UpdateFolderAsync(string id, UpdateFolderRequest request, CancellationToken ct = default)
	{
		var folder = await db.CycleFolders.FindAsync([id], ct);
		if (folder is null) return null;

		if (request.Name is not null) folder.Name = request.Name;
		if (request.SortOrder.HasValue) folder.SortOrder = request.SortOrder.Value;

		await db.SaveChangesAsync(ct);
		return new FolderResponse(folder.Id, folder.Name, folder.CycleId, folder.SortOrder);
	}

	public async Task<bool> DeleteFolderAsync(string id, CancellationToken ct = default)
	{
		var folder = await db.CycleFolders.FindAsync([id], ct);
		if (folder is null) return false;

		db.CycleFolders.Remove(folder);
		await db.SaveChangesAsync(ct);
		return true;
	}
}