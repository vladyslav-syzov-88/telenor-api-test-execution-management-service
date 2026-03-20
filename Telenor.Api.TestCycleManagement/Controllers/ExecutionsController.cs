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
public class ExecutionsController(IExecutionRepository repository) : ControllerBase
{
	/// <remarks>Retrieves test executions for a specific folder with pagination support.</remarks>
	[HttpGet("folder/{folderId}")]
	[SwaggerOperation("Retrieves executions by folder")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<PaginatedExecutionResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginatedExecutionResponse>> GetByFolder(
		[SwaggerParameter(Required = true)] string folderId,
		[FromQuery, SwaggerParameter(Description = "Number of records to skip")] int offset = 0,
		[FromQuery, SwaggerParameter(Description = "Page size")] int size = 50,
		CancellationToken ct = default)
	{
		return Ok(await repository.GetExecutionsByFolderAsync(folderId, offset, size, ct));
	}

	/// <remarks>Searches executions using structured filters. Replaces Zephyr ZQL queries.</remarks>
	[HttpPost("search")]
	[SwaggerOperation("Searches executions by structured query")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<ExecutionSearchResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ExecutionSearchResponse>> Search(
		[FromBody] ExecutionSearchRequest request,
		CancellationToken ct)
	{
		return Ok(await repository.SearchExecutionsAsync(request, ct));
	}

	/// <remarks>Updates a single execution status and/or comment. Records change in execution history.</remarks>
	[HttpPut("{id}")]
	[SwaggerOperation("Updates a single execution")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<ExecutionResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ExecutionResponse>> UpdateExecution(
		[SwaggerParameter(Required = true)] string id,
		[FromBody] UpdateExecutionRequest request,
		CancellationToken ct)
	{
		// TODO: Extract changedBy from auth context when authentication is implemented
		var changedBy = "system";
		var execution = await repository.UpdateExecutionAsync(id, request, changedBy, ct);
		if (execution is null) return NotFound();
		return Ok(execution);
	}

	/// <remarks>Updates multiple executions at once. All executions receive the same status. Records changes in execution history.</remarks>
	[HttpPost("bulk")]
	[SwaggerOperation("Bulk-updates execution statuses")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> BulkUpdate([FromBody] BulkUpdateExecutionsRequest request, CancellationToken ct)
	{
		var changedBy = "system";
		var count = await repository.BulkUpdateExecutionsAsync(request, changedBy, ct);
		return Ok(new { UpdatedCount = count });
	}

	[HttpGet("{id}/history")]
	[SwaggerOperation("Retrieves execution status change history")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<List<ExecutionHistoryResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<ExecutionHistoryResponse>>> GetHistory(
		[SwaggerParameter(Required = true)] string id,
		CancellationToken ct)
	{
		return Ok(await repository.GetExecutionHistoryAsync(id, ct));
	}
}