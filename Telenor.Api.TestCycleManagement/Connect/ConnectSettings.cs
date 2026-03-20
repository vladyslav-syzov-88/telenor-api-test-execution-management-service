namespace Telenor.Api.TestExecutionManagement.Connect;

public class ConnectSettings
{
	public string Key { get; set; } = "telenor-test-execution-management";
	public string Name { get; set; } = "Test Execution Management";
	public string Description { get; set; } = "Internal replacement for Zephyr Squad — test execution management, tracking and reporting. Aligned with TMF708.";
	public string BaseUrl { get; set; } = string.Empty;
	public string VendorName { get; set; } = "Telenor A/S";
	public string VendorUrl { get; set; } = "https://www.telenor.dk";
}