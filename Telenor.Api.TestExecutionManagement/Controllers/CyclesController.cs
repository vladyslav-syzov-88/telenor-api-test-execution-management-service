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
public class CyclesController(ICycleRepository repository, IFolderRepository folderRepository) : ControllerBase
{
	/// <remarks>Lists test cycles filtered by project and optionally by version.</remarks>
	[HttpGet]
	[SwaggerOperation("Retrieves test cycles")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<List<CycleResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<CycleResponse>>> GetCycles(
		[FromQuery, SwaggerParameter(Required = true, Description = "Project ID")] int projectId,
		[FromQuery, SwaggerParameter(Description = "Optional version ID filter")] int? versionId,
		CancellationToken ct)
	{
		return Ok(await repository.GetCyclesAsync(projectId, versionId, ct));
	}

	[HttpGet("{id}")]
	[SwaggerOperation("Retrieves a single test cycle")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<CycleResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CycleResponse>> GetCycle(
		[SwaggerParameter(Required = true)] string id,
		CancellationToken ct)
	{
		var cycle = await repository.GetCycleByIdAsync(id, ct);
		if (cycle is null) return NotFound();
		return Ok(cycle);
	}

	[HttpPost]
	[SwaggerOperation("Creates a new test cycle")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<CycleResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<CycleResponse>> CreateCycle([FromBody] CreateCycleRequest request, CancellationToken ct)
	{
		var cycle = await repository.CreateCycleAsync(request, ct);
		return CreatedAtAction(nameof(GetCycle), new { id = cycle.Id }, cycle);
	}

	[HttpPut("{id}")]
	[SwaggerOperation("Updates a test cycle")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<CycleResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<CycleResponse>> UpdateCycle(
		[SwaggerParameter(Required = true)] string id,
		[FromBody] UpdateCycleRequest request,
		CancellationToken ct)
	{
		var cycle = await repository.UpdateCycleAsync(id, request, ct);
		if (cycle is null) return NotFound();
		return Ok(cycle);
	}

	[HttpDelete("{id}")]
	[SwaggerOperation("Deletes a test cycle")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteCycle(
		[SwaggerParameter(Required = true)] string id,
		CancellationToken ct)
	{
		var deleted = await repository.DeleteCycleAsync(id, ct);
		if (!deleted) return NotFound();
		return NoContent();
	}

	/// <remarks>Clones a test cycle with all its folders and creates unexecuted entries for all test cases.</remarks>
	[HttpPost("{id}/clone")]
	[SwaggerOperation("Clones a test cycle to a target version")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<CycleResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CycleResponse>> CloneCycle(
		[SwaggerParameter(Required = true)] string id,
		[FromQuery, SwaggerParameter(Required = true, Description = "Target version ID for the cloned cycle")] int targetVersionId,
		CancellationToken ct)
	{
		var cycle = await repository.CloneCycleAsync(id, targetVersionId, ct);
		if (cycle is null) return NotFound();
		return CreatedAtAction(nameof(GetCycle), new { id = cycle.Id }, cycle);
	}

	[HttpGet("{cycleId}/folders")]
	[SwaggerOperation("Retrieves folders within a test cycle")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<List<FolderResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<FolderResponse>>> GetFolders(
		[SwaggerParameter(Required = true)] string cycleId,
		CancellationToken ct)
	{
		return Ok(await folderRepository.GetFoldersAsync(cycleId, ct));
	}

	[HttpPost("{cycleId}/folders")]
	[SwaggerOperation("Creates a folder within a test cycle")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<FolderResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<FolderResponse>> CreateFolder(
		[SwaggerParameter(Required = true)] string cycleId,
		[FromBody] CreateFolderRequest request,
		CancellationToken ct)
	{
		var folder = await folderRepository.CreateFolderAsync(cycleId, request, ct);
		return Created($"/api/cycles/{cycleId}/folders/{folder.Id}", folder);
	}
}