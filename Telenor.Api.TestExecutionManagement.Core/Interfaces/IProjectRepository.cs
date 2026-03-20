using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telenor.Api.TestExecutionManagement.Core.DTOs;

namespace Telenor.Api.TestExecutionManagement.Core.Interfaces;

public interface IProjectRepository
{
	Task<List<ProjectResponse>> GetProjectsAsync(CancellationToken ct = default);
	Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default);
	Task<List<VersionResponse>> GetVersionsAsync(int projectId, CancellationToken ct = default);
	Task<VersionResponse> CreateVersionAsync(CreateVersionRequest request, CancellationToken ct = default);
	Task<VersionResponse?> UpdateVersionAsync(int id, UpdateVersionRequest request, CancellationToken ct = default);
}