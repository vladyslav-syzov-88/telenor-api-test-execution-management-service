using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telenor.Api.TestExecutionManagement.Core.DTOs;

namespace Telenor.Api.TestExecutionManagement.Core.Interfaces;

public interface IExecutionRepository
{
	Task<PaginatedExecutionResponse> GetExecutionsByFolderAsync(string folderId, int offset, int size, CancellationToken ct = default);
	Task<ExecutionSearchResponse> SearchExecutionsAsync(ExecutionSearchRequest request, CancellationToken ct = default);
	Task<ExecutionResponse?> UpdateExecutionAsync(string id, UpdateExecutionRequest request, string changedBy, CancellationToken ct = default);
	Task<int> BulkUpdateExecutionsAsync(BulkUpdateExecutionsRequest request, string changedBy, CancellationToken ct = default);
	Task<List<ExecutionHistoryResponse>> GetExecutionHistoryAsync(string executionId, CancellationToken ct = default);
}