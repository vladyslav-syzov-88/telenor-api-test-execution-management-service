using System;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public record ConnectTenant
{
	public int Id { get; init; }
	public string ClientKey { get; init; } = string.Empty;
	public string SharedSecret { get; init; } = string.Empty;
	public string BaseUrl { get; init; } = string.Empty;
	public string? DisplayUrl { get; init; }
	public string? ProductType { get; init; }
	public string? Description { get; init; }
	public bool IsActive { get; init; } = true;
	public DateTime InstalledAt { get; init; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; init; }
}
