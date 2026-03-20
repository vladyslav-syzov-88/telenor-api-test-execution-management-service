using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telenor.Api.TestExecutionManagement.Core.DTOs;

namespace Telenor.Api.TestExecutionManagement.Core.Interfaces;

public interface ITestCaseRepository
{
	Task<List<TestCaseResponse>> SearchTestCasesAsync(int projectId, string? search, CancellationToken ct = default);
	Task<TestCaseResponse?> GetByJiraKeyAsync(string jiraIssueKey, CancellationToken ct = default);
	Task<TestCaseResponse> CreateTestCaseAsync(CreateTestCaseRequest request, CancellationToken ct = default);
	Task<TestCaseResponse?> UpdateTestCaseAsync(string id, UpdateTestCaseRequest request, CancellationToken ct = default);
}