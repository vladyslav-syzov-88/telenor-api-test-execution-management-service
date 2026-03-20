using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Telenor.Api.TestExecutionManagement.Controllers;

/// <summary>
/// Serves Connect iframe pages embedded in Jira.
/// These endpoints return HTML that Jira loads inside iframes.
/// </summary>
[ApiController]
[Route("connect")]
[ExcludeFromCodeCoverage]
[ApiExplorerSettings(IgnoreApi = true)]
public class ConnectPagesController : ControllerBase
{
	[HttpGet("pages/cycles")]
	[SwaggerOperation("Cycle management page (Jira iframe)")]
	public IActionResult CyclesPage([FromQuery] string? projectId)
	{
		return PhysicalFile(
			Path.Combine(AppContext.BaseDirectory, "wwwroot", "connect", "pages", "cycles.html"),
			"text/html");
	}

	[HttpGet("pages/dashboard")]
	[SwaggerOperation("Test dashboard page (Jira iframe)")]
	public IActionResult DashboardPage([FromQuery] string? projectId)
	{
		return PhysicalFile(
			Path.Combine(AppContext.BaseDirectory, "wwwroot", "connect", "pages", "dashboard.html"),
			"text/html");
	}

	[HttpGet("pages/project-summary")]
	[SwaggerOperation("Project test summary tab (Jira iframe)")]
	public IActionResult ProjectSummaryPage([FromQuery] string? projectId)
	{
		return PhysicalFile(
			Path.Combine(AppContext.BaseDirectory, "wwwroot", "connect", "pages", "project-summary.html"),
			"text/html");
	}

	[HttpGet("panels/issue")]
	[SwaggerOperation("Issue test executions panel (Jira iframe)")]
	public IActionResult IssuePanel([FromQuery] string? issueKey)
	{
		return PhysicalFile(
			Path.Combine(AppContext.BaseDirectory, "wwwroot", "connect", "panels", "issue.html"),
			"text/html");
	}
}