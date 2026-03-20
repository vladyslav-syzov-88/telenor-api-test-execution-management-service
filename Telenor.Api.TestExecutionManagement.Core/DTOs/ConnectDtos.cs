using System.Text.Json.Serialization;

namespace Telenor.Api.TestExecutionManagement.Core.DTOs;

public record ConnectLifecyclePayload(
	[property: JsonPropertyName("key")] string Key,
	[property: JsonPropertyName("clientKey")] string ClientKey,
	[property: JsonPropertyName("sharedSecret")] string? SharedSecret,
	[property: JsonPropertyName("baseUrl")] string BaseUrl,
	[property: JsonPropertyName("displayUrl")] string? DisplayUrl,
	[property: JsonPropertyName("productType")] string? ProductType,
	[property: JsonPropertyName("description")] string? Description);