using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Models;

namespace Telenor.Api.TestExecutionManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class VersionsController(IProjectRepository repository) : ControllerBase
{
	[HttpPost]
	[SwaggerOperation("Creates a new version")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<VersionResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<VersionResponse>> CreateVersion([FromBody] CreateVersionRequest request, CancellationToken ct)
	{
		var version = await repository.CreateVersionAsync(request, ct);
		return CreatedAtAction(nameof(UpdateVersion), new { id = version.Id }, version);
	}

	[HttpPut("{id:int}")]
	[SwaggerOperation("Updates a version")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<VersionResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<VersionResponse>> UpdateVersion(
		[SwaggerParameter(Required = true)] int id,
		[FromBody] UpdateVersionRequest request,
		CancellationToken ct)
	{
		var version = await repository.UpdateVersionAsync(id, request, ct);
		if (version is null) return NotFound();
		return Ok(version);
	}
}