using Telenor.Api.TestExecutionManagement.Core.DTOs;

namespace Telenor.Api.TestExecutionManagement.Core.Interfaces;

public interface IFolderRepository
{
	Task<List<FolderResponse>> GetFoldersAsync(string cycleId, CancellationToken ct = default);
	Task<FolderResponse> CreateFolderAsync(string cycleId, CreateFolderRequest request, CancellationToken ct = default);
	Task<FolderResponse?> UpdateFolderAsync(string id, UpdateFolderRequest request, CancellationToken ct = default);
	Task<bool> DeleteFolderAsync(string id, CancellationToken ct = default);
}