using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using Telenor.Api.TestExecutionManagement.Connect;

namespace Telenor.Api.TestExecutionManagement.Controllers;

/// <summary>
/// Serves the Atlassian Connect app descriptor.
/// Jira fetches this when an admin installs the app.
/// </summary>
[ApiController]
[ExcludeFromCodeCoverage]
public class ConnectDescriptorController(IOptions<ConnectSettings> settings) : ControllerBase
{
	[HttpGet("/atlassian-connect.json")]
	[SwaggerOperation("Returns the Atlassian Connect app descriptor")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public IActionResult GetDescriptor()
	{
		ConnectSettings config = settings.Value;

		// Auto-detect base URL if not configured
		if (string.IsNullOrEmpty(config.BaseUrl))
		{
			config.BaseUrl = $"{Request.Scheme}://{Request.Host}";
		}

		return Ok(ConnectDescriptorBuilder.Build(config));
	}
}