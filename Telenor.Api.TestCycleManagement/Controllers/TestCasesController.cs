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
public class TestCasesController(ITestCaseRepository repository) : ControllerBase
{
	[HttpGet]
	[SwaggerOperation("Searches test cases")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<List<TestCaseResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TestCaseResponse>>> Search(
		[FromQuery, SwaggerParameter(Required = true, Description = "Project ID")] int projectId,
		[FromQuery, SwaggerParameter(Description = "Search term for Jira key or summary")] string? search,
		CancellationToken ct)
	{
		return Ok(await repository.SearchTestCasesAsync(projectId, search, ct));
	}

	[HttpGet("by-jira-key/{key}")]
	[SwaggerOperation("Retrieves a test case by Jira issue key")]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<TestCaseResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<TestCaseResponse>> GetByJiraKey(
		[SwaggerParameter(Required = true, Description = "Jira issue key, e.g. OSE-12345")] string key,
		CancellationToken ct)
	{
		var tc = await repository.GetByJiraKeyAsync(key, ct);
		if (tc is null) return NotFound();
		return Ok(tc);
	}

	[HttpPost]
	[SwaggerOperation("Creates a new test case")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<TestCaseResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<TestCaseResponse>> Create([FromBody] CreateTestCaseRequest request, CancellationToken ct)
	{
		var tc = await repository.CreateTestCaseAsync(request, ct);
		return CreatedAtAction(nameof(GetByJiraKey), new { key = tc.JiraIssueKey }, tc);
	}

	[HttpPut("{id}")]
	[SwaggerOperation("Updates a test case")]
	[Consumes(MediaTypeNames.Application.Json)]
	[Produces(MediaTypeNames.Application.Json)]
	[ProducesResponseType<TestCaseResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
	[ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<TestCaseResponse>> Update(
		[SwaggerParameter(Required = true)] string id,
		[FromBody] UpdateTestCaseRequest request,
		CancellationToken ct)
	{
		var tc = await repository.UpdateTestCaseAsync(id, request, ct);
		if (tc is null) return NotFound();
		return Ok(tc);
	}
}