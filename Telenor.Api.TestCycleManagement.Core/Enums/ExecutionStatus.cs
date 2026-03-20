namespace Telenor.Api.TestExecutionManagement.Core.Enums;

/// <summary>
/// Matches TestState enum IDs from the UI test framework.
/// </summary>
public enum ExecutionStatus
{
	UnExecuted = -1,
	Pass = 1,
	Fail = 2,
	WIP = 3,
	Blocked = 4,
	PartiallyPassed = 5,
	FailedWithIssue = 6
}