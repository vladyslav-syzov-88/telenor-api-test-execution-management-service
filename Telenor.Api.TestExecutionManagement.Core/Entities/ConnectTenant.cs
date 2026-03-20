using System;

namespace Telenor.Api.TestExecutionManagement.Core.Entities;

public class ConnectTenant
{
	public int Id { get; set; }
	public string ClientKey { get; set; } = string.Empty;
	public string SharedSecret { get; set; } = string.Empty;
	public string BaseUrl { get; set; } = string.Empty;
	public string? DisplayUrl { get; set; }
	public string? ProductType { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
}