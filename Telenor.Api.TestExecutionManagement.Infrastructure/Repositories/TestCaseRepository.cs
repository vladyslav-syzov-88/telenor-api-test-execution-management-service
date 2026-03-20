using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.DTOs;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

public class TestCaseRepository(AppDbContext db) : ITestCaseRepository
{
	public async Task<List<TestCaseResponse>> SearchTestCasesAsync(int projectId, string? search, CancellationToken ct = default)
	{
		var query = db.TestCases.Where(t => t.ProjectId == projectId);

		if (!string.IsNullOrWhiteSpace(search))
			query = query.Where(t => t.JiraIssueKey.Contains(search) || t.Summary.Contains(search));

		return await query
			.OrderBy(t => t.JiraIssueKey)
			.Take(200)
			.Select(t => ToResponse(t))
			.ToListAsync(ct);
	}

	public async Task<TestCaseResponse?> GetByJiraKeyAsync(string jiraIssueKey, CancellationToken ct = default)
	{
		var tc = await db.TestCases.AsNoTracking().FirstOrDefaultAsync(t => t.JiraIssueKey == jiraIssueKey, ct);
		return tc is null ? null : ToResponse(tc);
	}

	public async Task<TestCaseResponse> CreateTestCaseAsync(CreateTestCaseRequest request, CancellationToken ct = default)
	{
		var tc = new TestCase
		{
			ProjectId = request.ProjectId,
			JiraIssueKey = request.JiraIssueKey,
			JiraIssueId = request.JiraIssueId,
			Summary = request.Summary
		};
		db.TestCases.Add(tc);
		await db.SaveChangesAsync(ct);
		return ToResponse(tc);
	}

	public async Task<TestCaseResponse?> UpdateTestCaseAsync(string id, UpdateTestCaseRequest request, CancellationToken ct = default)
	{
		var tc = await db.TestCases.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
		if (tc is null) return null;

		var updated = tc with
		{
			JiraIssueKey = request.JiraIssueKey ?? tc.JiraIssueKey,
			Summary = request.Summary ?? tc.Summary,
			JiraIssueId = request.JiraIssueId ?? tc.JiraIssueId,
			UpdatedAt = DateTime.UtcNow
		};

		db.TestCases.Update(updated);
		await db.SaveChangesAsync(ct);
		return ToResponse(updated);
	}

	private static TestCaseResponse ToResponse(TestCase t) =>
		new(t.Id, t.ProjectId, t.JiraIssueKey, t.JiraIssueId, t.Summary, t.CreatedAt, t.UpdatedAt);
}
