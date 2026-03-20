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
public class FoldersController(IFolderRepository repository) : ControllerBase
{
	[HttpPut("{id}")]
	[SwaggerOperation("Updates a folder")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<FolderResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<FolderResponse>> UpdateFolder(
		[SwaggerParameter(Required = true)] string id,
		[FromBody] UpdateFolderRequest request,
		CancellationToken ct)
	{
		var folder = await repository.UpdateFolderAsync(id, request, ct);
		if (folder is null) return NotFound();
		return Ok(folder);
	}

	[HttpDelete("{id}")]
	[SwaggerOperation("Deletes a folder")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteFolder(
		[SwaggerParameter(Required = true)] string id,
		CancellationToken ct)
	{
		var deleted = await repository.DeleteFolderAsync(id, ct);
		if (!deleted) return NotFound();
		return NoContent();
	}
}