using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telenor.Api.TestExecutionManagement.Core.DTOs;

namespace Telenor.Api.TestExecutionManagement.Core.Interfaces;

public interface IDashboardRepository
{
	Task<CycleSummaryResponse?> GetCycleSummaryAsync(string cycleId, CancellationToken ct = default);
	Task<VersionSummaryResponse?> GetVersionSummaryAsync(int versionId, CancellationToken ct = default);
	Task<List<VersionSummaryResponse>> GetVersionsOverviewAsync(int projectId, CancellationToken ct = default);
}