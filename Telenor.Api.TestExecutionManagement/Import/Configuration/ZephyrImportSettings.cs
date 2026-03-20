namespace Telenor.Api.TestExecutionManagement.Import.Configuration;

public class ZephyrImportSettings
{
	public string BaseUrl { get; set; } = "https://prod-api.zephyr4jiracloud.com/connect";
	public string AccountId { get; set; } = string.Empty;
	public string AccessKey { get; set; } = string.Empty;
	public string SecretKey { get; set; } = string.Empty;
	public int JwtValiditySeconds { get; set; } = 300;

	public string JiraBaseUrl { get; set; } = string.Empty;
	public string JiraUser { get; set; } = string.Empty;
}