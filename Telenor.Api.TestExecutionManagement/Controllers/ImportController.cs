using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Telenor.Api.TestExecutionManagement.Import;
using Telenor.Api.TestExecutionManagement.Import.Models;
using Telenor.Api.TestExecutionManagement.Models;

namespace Telenor.Api.TestExecutionManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class ImportController(ZephyrImportService importService) : ControllerBase
{
	/// <remarks>
	/// Imports test cycles, folders, test cases, and executions from Zephyr Squad Cloud into the local database.
	///
	/// **Request body fields:**
	/// - `projectId` (required) — Jira project ID (e.g. "11786")
	/// - `versionId` (optional) — specific Jira version ID to import. If omitted, all versions are fetched from Jira.
	/// - `cycleName` (optional) — filter to import only cycles matching this name (e.g. "TI UI Microservices Tests")
	///
	/// **What gets imported:**
	/// 1. Project (created if not exists, matched by Jira project ID)
	/// 2. Versions (fetched from Jira API or created from the specified versionId)
	/// 3. Test cycles with their folders
	/// 4. Test cases (deduplicated by Jira issue key within the project)
	/// 5. Test executions with their current status and comments
	///
	/// **Idempotency:** Test cases are deduplicated by Jira issue key. However, cycles and executions are always
	/// created as new records. Running import twice for the same data will create duplicate cycles.
	/// </remarks>
	[HttpPost]
	[SwaggerOperation("Imports data from Zephyr Squad Cloud")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<ImportResult>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType<ApiError>(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ImportResult>> Import([FromBody] ImportRequest request, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(request.ProjectId))
			return BadRequest(new ApiError("VALIDATION_ERROR", "ProjectId is required"));

		try
		{
			var result = await importService.ImportAsync(request, ct);
			return Ok(result);
		}
		catch (ImportConfigurationException ex)
		{
			return BadRequest(new ApiError("CONFIGURATION_ERROR", ex.Message));
		}
		catch (Exception ex)
		{
			return StatusCode(StatusCodes.Status500InternalServerError,
				new ApiError("IMPORT_ERROR", "Import failed", ex.Message));
		}
	}
}