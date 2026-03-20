using Telenor.Api.TestExecutionManagement.Core.DTOs;

namespace Telenor.Api.TestExecutionManagement.Core.Interfaces;

public interface ICycleRepository
{
	Task<List<CycleResponse>> GetCyclesAsync(int projectId, int? versionId, CancellationToken ct = default);
	Task<CycleResponse?> GetCycleByIdAsync(string id, CancellationToken ct = default);
	Task<CycleResponse> CreateCycleAsync(CreateCycleRequest request, CancellationToken ct = default);
	Task<CycleResponse?> UpdateCycleAsync(string id, UpdateCycleRequest request, CancellationToken ct = default);
	Task<bool> DeleteCycleAsync(string id, CancellationToken ct = default);
	Task<CycleResponse?> CloneCycleAsync(string cycleId, int targetVersionId, CancellationToken ct = default);
}