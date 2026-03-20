using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Telenor.Api.TestExecutionManagement.Import.Configuration;
using Telenor.Api.TestExecutionManagement.Import.Models;

namespace Telenor.Api.TestExecutionManagement.Import.ZephyrClient;

public class ZephyrApiClient
{
	private readonly HttpClient _httpClient;
	private readonly ZephyrImportSettings _settings;
	private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

	public ZephyrApiClient(HttpClient httpClient, IOptions<ZephyrImportSettings> settings)
	{
		_httpClient = httpClient;
		_settings = settings.Value;
	}

	public async Task<List<ZephyrCycle>> GetCyclesAsync(string projectId, string versionId, CancellationToken ct)
	{
		var queryParams = new Dictionary<string, string>
		{
			{ "expand", "action" },
			{ "projectId", projectId },
			{ "versionId", versionId }
		};

		const string endpoint = "/public/rest/api/1.0/cycles/search";
		var request = BuildRequest(HttpMethod.Get, endpoint, queryParams);
		var response = await _httpClient.SendAsync(request, ct);
		await EnsureAuthenticatedAsync(response, ct);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync(ct);
		return JsonSerializer.Deserialize<List<ZephyrCycle>>(json, JsonOptions) ?? [];
	}

	public async Task<List<ZephyrFolder>> GetFoldersAsync(string cycleId, string projectId, string versionId, CancellationToken ct)
	{
		var queryParams = new Dictionary<string, string>
		{
			{ "cycleId", cycleId },
			{ "projectId", projectId },
			{ "versionId", versionId }
		};

		const string endpoint = "/public/rest/api/1.0/folders";
		var request = BuildRequest(HttpMethod.Get, endpoint, queryParams);
		var response = await _httpClient.SendAsync(request, ct);
		await EnsureAuthenticatedAsync(response, ct);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync(ct);
		return JsonSerializer.Deserialize<List<ZephyrFolder>>(json, JsonOptions) ?? [];
	}

	public async Task<List<ZephyrTestExecution>> GetExecutionsForFolderAsync(
		string folderId, string cycleId, string projectId, string versionId, CancellationToken ct)
	{
		var apiEndpoint = $"/public/rest/api/2.0/executions/search/folder/{folderId}";
		var result = new List<ZephyrTestExecution>();
		var offset = 0;

		for (var iteration = 0; iteration < 20; iteration++)
		{
			var queryParams = new Dictionary<string, string>
			{
				{ "cycleId", cycleId },
				{ "offset", offset.ToString() },
				{ "projectId", projectId },
				{ "versionId", versionId }
			};

			var request = BuildRequest(HttpMethod.Get, apiEndpoint, queryParams);
			var response = await _httpClient.SendAsync(request, ct);
			await EnsureAuthenticatedAsync(response, ct);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync(ct);
			var searchResult = JsonSerializer.Deserialize<ZephyrExecutionsSearchResult>(json, JsonOptions);
			if (searchResult?.SearchResult.SearchObjectList is null || searchResult.SearchResult.SearchObjectList.Count == 0)
				break;

			result.AddRange(searchResult.SearchResult.SearchObjectList);

			if (result.Count >= searchResult.SearchResult.TotalCount)
				break;

			offset += searchResult.SearchResult.SearchObjectList.Count;
		}

		return result;
	}

	public async Task<List<JiraVersion>> GetJiraVersionsAsync(string projectId, CancellationToken ct)
	{
		if (string.IsNullOrEmpty(_settings.JiraBaseUrl) || string.IsNullOrEmpty(_settings.JiraUser))
			return [];

		using var jiraClient = new HttpClient();
		jiraClient.BaseAddress = new Uri(_settings.JiraBaseUrl.TrimEnd('/'));
		var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_settings.JiraUser));
		jiraClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
		jiraClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		var response = await jiraClient.GetAsync($"/rest/api/latest/project/{projectId}/versions", ct);
		if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
		{
			var body = await response.Content.ReadAsStringAsync(ct);
			throw new ImportConfigurationException(
				$"Jira API returned {(int)response.StatusCode} {response.StatusCode}. " +
				$"Check JiraBaseUrl and JiraUser credentials. Response: {body}");
		}
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync(ct);
		return JsonSerializer.Deserialize<List<JiraVersion>>(json, JsonOptions) ?? [];
	}

	public void ValidateConfiguration()
	{
		var missing = new List<string>();
		if (string.IsNullOrWhiteSpace(_settings.AccessKey)) missing.Add("AccessKey");
		if (string.IsNullOrWhiteSpace(_settings.SecretKey)) missing.Add("SecretKey");
		if (string.IsNullOrWhiteSpace(_settings.AccountId)) missing.Add("AccountId");

		if (missing.Count > 0)
			throw new ImportConfigurationException(
				$"Zephyr credentials not configured. Missing: {string.Join(", ", missing)}. " +
				"Set them in appsettings.json under ZephyrImport or via environment variables.");
	}

	private HttpRequestMessage BuildRequest(HttpMethod method, string endpoint, Dictionary<string, string> queryParams)
	{
		// JWT QSH is computed from the relative endpoint path (without base URL)
		var jwtToken = ZephyrJwtGenerator.GenerateJwtToken(_settings, method.Method, endpoint, queryParams);

		var baseUrl = _settings.BaseUrl.TrimEnd('/');
		var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
		var fullUrl = string.IsNullOrEmpty(queryString)
			? $"{baseUrl}{endpoint}"
			: $"{baseUrl}{endpoint}?{queryString}";

		var request = new HttpRequestMessage(method, fullUrl);
		request.Headers.Authorization = new AuthenticationHeaderValue("JWT", jwtToken);
		request.Headers.Add("zapiAccessKey", _settings.AccessKey);

		return request;
	}

	private static async Task EnsureAuthenticatedAsync(HttpResponseMessage response, CancellationToken ct)
	{
		if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
		{
			var body = await response.Content.ReadAsStringAsync(ct);
			throw new ImportConfigurationException(
				$"Zephyr API returned {(int)response.StatusCode} {response.StatusCode}. " +
				$"Check your credentials (AccessKey, SecretKey, AccountId). Response: {body}");
		}
	}
}