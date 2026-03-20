namespace Telenor.Api.TestExecutionManagement.Import.Configuration;

public record ZephyrImportSettings
{
	public string BaseUrl { get; init; } = "https://prod-api.zephyr4jiracloud.com/connect";
	public string AccountId { get; init; } = string.Empty;
	public string AccessKey { get; init; } = string.Empty;
	public string SecretKey { get; init; } = string.Empty;
	public int JwtValiditySeconds { get; init; } = 300;

	public string JiraBaseUrl { get; init; } = string.Empty;
	public string JiraUser { get; init; } = string.Empty;
}
