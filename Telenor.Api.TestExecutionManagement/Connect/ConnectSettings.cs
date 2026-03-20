namespace Telenor.Api.TestExecutionManagement.Connect;

public record ConnectSettings
{
	public string Key { get; init; } = "telenor-test-execution-management";
	public string Name { get; init; } = "Test Execution Management";
	public string Description { get; init; } = "Internal replacement for Zephyr Squad — test execution management, tracking and reporting. Aligned with TMF708.";
	public string BaseUrl { get; init; } = string.Empty;
	public string VendorName { get; init; } = "Telenor A/S";
	public string VendorUrl { get; init; } = "https://www.telenor.dk";
}
