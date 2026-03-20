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
public class DashboardController(IDashboardRepository repository) : ControllerBase
{
	/// <remarks>Returns pass/fail/blocked/WIP/unexecuted counts for each folder in the cycle.</remarks>
	[HttpGet("cycles/{cycleId}/summary")]
	[SwaggerOperation("Retrieves cycle execution summary")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<CycleSummaryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CycleSummaryResponse>> GetCycleSummary(
		[SwaggerParameter(Required = true)] string cycleId,
		CancellationToken ct)
	{
		var summary = await repository.GetCycleSummaryAsync(cycleId, ct);
		if (summary is null) return NotFound();
		return Ok(summary);
	}

	/// <remarks>Returns all cycles for a version with their execution status breakdown.</remarks>
	[HttpGet("versions/{versionId:int}/summary")]
	[SwaggerOperation("Retrieves version execution summary")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<VersionSummaryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<VersionSummaryResponse>> GetVersionSummary(
		[SwaggerParameter(Required = true)] int versionId,
		CancellationToken ct)
	{
		var summary = await repository.GetVersionSummaryAsync(versionId, ct);
		if (summary is null) return NotFound();
		return Ok(summary);
	}

	/// <remarks>Returns released vs unreleased versions overview with cycle status breakdown for each.</remarks>
	[HttpGet("versions")]
	[SwaggerOperation("Retrieves versions overview with cycle summaries")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<List<VersionSummaryResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<VersionSummaryResponse>>> GetVersionsOverview(
		[FromQuery, SwaggerParameter(Required = true, Description = "Project ID")] int projectId,
		CancellationToken ct)
	{
		return Ok(await repository.GetVersionsOverviewAsync(projectId, ct));
	}
}