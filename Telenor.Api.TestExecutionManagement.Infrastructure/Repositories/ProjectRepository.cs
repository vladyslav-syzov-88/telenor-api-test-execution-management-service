using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;
using Version = Telenor.Api.TestExecutionManagement.Core.Entities.Version;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

public class ProjectRepository(AppDbContext db) : IProjectRepository
{
	public async Task<List<ProjectResponse>> GetProjectsAsync(CancellationToken ct = default)
	{
		return await db.Projects
			.OrderBy(p => p.Name)
			.Select(p => new ProjectResponse(p.Id, p.JiraProjectId, p.Name, p.CreatedAt))
			.ToListAsync(ct);
	}

	public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default)
	{
		var project = new Project
		{
			JiraProjectId = request.JiraProjectId,
			Name = request.Name
		};
		db.Projects.Add(project);
		await db.SaveChangesAsync(ct);
		return new ProjectResponse(project.Id, project.JiraProjectId, project.Name, project.CreatedAt);
	}

	public async Task<List<VersionResponse>> GetVersionsAsync(int projectId, CancellationToken ct = default)
	{
		return await db.Versions
			.Where(v => v.ProjectId == projectId)
			.OrderByDescending(v => v.CreatedAt)
			.Select(v => new VersionResponse(v.Id, v.ProjectId, v.Name, v.IsReleased, v.ReleaseDate, v.JiraVersionId, v.CreatedAt))
			.ToListAsync(ct);
	}

	public async Task<VersionResponse> CreateVersionAsync(CreateVersionRequest request, CancellationToken ct = default)
	{
		var version = new Version
		{
			ProjectId = request.ProjectId,
			Name = request.Name,
			JiraVersionId = request.JiraVersionId,
			IsReleased = request.IsReleased,
			ReleaseDate = request.ReleaseDate
		};
		db.Versions.Add(version);
		await db.SaveChangesAsync(ct);
		return new VersionResponse(version.Id, version.ProjectId, version.Name, version.IsReleased, version.ReleaseDate, version.JiraVersionId, version.CreatedAt);
	}

	public async Task<VersionResponse?> UpdateVersionAsync(int id, UpdateVersionRequest request, CancellationToken ct = default)
	{
		var version = await db.Versions.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
		if (version is null) return null;

		var updated = version with
		{
			Name = request.Name ?? version.Name,
			IsReleased = request.IsReleased ?? version.IsReleased,
			ReleaseDate = request.ReleaseDate ?? version.ReleaseDate
		};

		db.Versions.Update(updated);
		await db.SaveChangesAsync(ct);
		return new VersionResponse(updated.Id, updated.ProjectId, updated.Name, updated.IsReleased, updated.ReleaseDate, updated.JiraVersionId, updated.CreatedAt);
	}
}
