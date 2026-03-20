using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Models;

namespace Telenor.Api.TestExecutionManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class ConnectController(
	IConnectTenantRepository tenantRepository,
	ILogger<ConnectController> logger) : ControllerBase
{
	/// <summary>
	/// Handles the Atlassian Connect install lifecycle event.
	/// Jira sends this when an admin installs the app.
	/// </summary>
	[HttpPost("installed")]
	[SwaggerOperation("Handles Atlassian Connect install lifecycle event")]
	[Consumes(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Installed([FromBody] ConnectLifecyclePayload payload, CancellationToken ct)
	{
		if (string.IsNullOrEmpty(payload.ClientKey) || string.IsNullOrEmpty(payload.SharedSecret))
		{
			return BadRequest(new ApiError("INVALID_PAYLOAD", "clientKey and sharedSecret are required"));
		}

		logger.LogInformation("Connect app installed by tenant {ClientKey} at {BaseUrl}", payload.ClientKey, payload.BaseUrl);

		var tenant = new ConnectTenant
		{
			ClientKey = payload.ClientKey,
			SharedSecret = payload.SharedSecret,
			BaseUrl = payload.BaseUrl,
			DisplayUrl = payload.DisplayUrl,
			ProductType = payload.ProductType,
			Description = payload.Description
		};

		await tenantRepository.UpsertAsync(tenant, ct);
		return NoContent();
	}

	/// <summary>
	/// Handles the Atlassian Connect uninstall lifecycle event.
	/// Jira sends this when an admin uninstalls the app.
	/// </summary>
	[HttpPost("uninstalled")]
	[SwaggerOperation("Handles Atlassian Connect uninstall lifecycle event")]
	[Consumes(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Uninstalled([FromBody] ConnectLifecyclePayload payload, CancellationToken ct)
	{
		logger.LogInformation("Connect app uninstalled by tenant {ClientKey}", payload.ClientKey);
		await tenantRepository.DeactivateAsync(payload.ClientKey, ct);
		return NoContent();
	}

	/// <summary>
	/// Handles the Atlassian Connect enable lifecycle event.
	/// </summary>
	[HttpPost("enabled")]
	[SwaggerOperation("Handles Atlassian Connect enable lifecycle event")]
	[Consumes(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public IActionResult Enabled([FromBody] ConnectLifecyclePayload payload)
	{
		logger.LogInformation("Connect app enabled for tenant {ClientKey}", payload.ClientKey);
		return NoContent();
	}

	/// <summary>
	/// Handles the Atlassian Connect disable lifecycle event.
	/// </summary>
	[HttpPost("disabled")]
	[SwaggerOperation("Handles Atlassian Connect disable lifecycle event")]
	[Consumes(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public IActionResult Disabled([FromBody] ConnectLifecyclePayload payload)
	{
		logger.LogInformation("Connect app disabled for tenant {ClientKey}", payload.ClientKey);
		return NoContent();
	}

	/// <summary>
	/// Webhook: Jira version created — auto-sync to local database.
	/// </summary>
	[HttpPost("webhooks/version-created")]
	[SwaggerOperation("Handles Jira version_created webhook")]
	[Consumes(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public IActionResult VersionCreated()
	{
		// TODO: Parse webhook payload and create local version
		logger.LogInformation("Received jira:version_created webhook");
		return NoContent();
	}

	/// <summary>
	/// Webhook: Jira version updated — auto-sync to local database.
	/// </summary>
	[HttpPost("webhooks/version-updated")]
	[SwaggerOperation("Handles Jira version_updated webhook")]
	[Consumes(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public IActionResult VersionUpdated()
	{
		// TODO: Parse webhook payload and update local version
		logger.LogInformation("Received jira:version_updated webhook");
		return NoContent();
	}
}