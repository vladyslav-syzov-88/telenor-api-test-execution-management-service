using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Models;

namespace Telenor.Api.TestExecutionManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class ProjectsController(IProjectRepository repository) : ControllerBase
{
	[HttpGet]
	[SwaggerOperation("Retrieves all projects")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<List<ProjectResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<ProjectResponse>>> GetProjects(CancellationToken ct)
	{
		return Ok(await repository.GetProjectsAsync(ct));
	}

	[HttpPost]
	[SwaggerOperation("Creates a new project")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<ProjectResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ProjectResponse>> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct)
	{
		var project = await repository.CreateProjectAsync(request, ct);
		return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, project);
	}

	[HttpGet("{projectId:int}/versions")]
	[SwaggerOperation("Retrieves all versions for a project")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<List<VersionResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<VersionResponse>>> GetVersions(
		[SwaggerParameter(Required = true)] int projectId,
		CancellationToken ct)
	{
		return Ok(await repository.GetVersionsAsync(projectId, ct));
	}
}