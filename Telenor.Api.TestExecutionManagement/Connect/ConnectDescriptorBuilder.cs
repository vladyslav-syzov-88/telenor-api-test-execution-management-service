using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Telenor.Api.TestExecutionManagement.Connect;

public static class ConnectDescriptorBuilder
{
	public static object Build(ConnectSettings settings)
	{
		return new ConnectDescriptor
		{
			Key = settings.Key,
			Name = settings.Name,
			Description = settings.Description,
			BaseUrl = settings.BaseUrl,
			Vendor = new Vendor { Name = settings.VendorName, Url = settings.VendorUrl },
			Authentication = new Authentication { Type = "jwt" },
			ApiVersion = 1,
			Lifecycle = new Lifecycle
			{
				Installed = "/api/connect/installed",
				Uninstalled = "/api/connect/uninstalled",
				Enabled = "/api/connect/enabled",
				Disabled = "/api/connect/disabled"
			},
			Scopes = ["READ", "WRITE"],
			Modules = new Modules
			{
				GeneralPages =
				[
					new GeneralPage
					{
						Key = "tcm-cycle-management",
						Name = new LocalizedText { Value = "Test Cycles" },
						Url = "/connect/pages/cycles?projectId={project.id}",
						Location = "system.top.navigation.bar",
						Icon = new Icon { Width = 24, Height = 24, Url = "/connect/assets/icon-cycles.svg" }
					},
					new GeneralPage
					{
						Key = "tcm-dashboard",
						Name = new LocalizedText { Value = "Test Dashboard" },
						Url = "/connect/pages/dashboard?projectId={project.id}",
						Location = "system.top.navigation.bar",
						Icon = new Icon { Width = 24, Height = 24, Url = "/connect/assets/icon-dashboard.svg" }
					}
				],
				WebPanels =
				[
					new WebPanel
					{
						Key = "tcm-issue-test-panel",
						Name = new LocalizedText { Value = "Test Executions" },
						Url = "/connect/panels/issue?issueKey={issue.key}",
						Location = "atl.jira.view.issue.right.context",
						Weight = 200
					}
				],
				JiraProjectTabPanels =
				[
					new ProjectTabPanel
					{
						Key = "tcm-project-summary",
						Name = new LocalizedText { Value = "Test Summary" },
						Url = "/connect/pages/project-summary?projectId={project.id}",
						Weight = 100
					}
				],
				WebhookHandlers =
				[
					new WebhookHandler
					{
						Event = "jira:version_created",
						Url = "/api/connect/webhooks/version-created"
					},
					new WebhookHandler
					{
						Event = "jira:version_updated",
						Url = "/api/connect/webhooks/version-updated"
					}
				]
			}
		};
	}
}

#region Descriptor Model

internal record ConnectDescriptor
{
	[JsonPropertyName("key")]
	public string Key { get; init; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; init; } = string.Empty;

	[JsonPropertyName("baseUrl")]
	public string BaseUrl { get; init; } = string.Empty;

	[JsonPropertyName("vendor")]
	public Vendor Vendor { get; init; } = new();

	[JsonPropertyName("authentication")]
	public Authentication Authentication { get; init; } = new();

	[JsonPropertyName("apiVersion")]
	public int ApiVersion { get; init; } = 1;

	[JsonPropertyName("lifecycle")]
	public Lifecycle Lifecycle { get; init; } = new();

	[JsonPropertyName("scopes")]
	public List<string> Scopes { get; init; } = [];

	[JsonPropertyName("modules")]
	public Modules Modules { get; init; } = new();
}

internal record Vendor
{
	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName("url")]
	public string Url { get; init; } = string.Empty;
}

internal record Authentication
{
	[JsonPropertyName("type")]
	public string Type { get; init; } = "jwt";
}

internal record Lifecycle
{
	[JsonPropertyName("installed")]
	public string Installed { get; init; } = string.Empty;

	[JsonPropertyName("uninstalled")]
	public string Uninstalled { get; init; } = string.Empty;

	[JsonPropertyName("enabled")]
	public string? Enabled { get; init; }

	[JsonPropertyName("disabled")]
	public string? Disabled { get; init; }
}

internal record Modules
{
	[JsonPropertyName("generalPages")]
	public List<GeneralPage> GeneralPages { get; init; } = [];

	[JsonPropertyName("webPanels")]
	public List<WebPanel> WebPanels { get; init; } = [];

	[JsonPropertyName("jiraProjectTabPanels")]
	public List<ProjectTabPanel> JiraProjectTabPanels { get; init; } = [];

	[JsonPropertyName("webhooks")]
	public List<WebhookHandler> WebhookHandlers { get; init; } = [];
}

internal record GeneralPage
{
	[JsonPropertyName("key")]
	public string Key { get; init; } = string.Empty;

	[JsonPropertyName("name")]
	public LocalizedText Name { get; init; } = new();

	[JsonPropertyName("url")]
	public string Url { get; init; } = string.Empty;

	[JsonPropertyName("location")]
	public string Location { get; init; } = string.Empty;

	[JsonPropertyName("icon")]
	public Icon? Icon { get; init; }
}

internal record WebPanel
{
	[JsonPropertyName("key")]
	public string Key { get; init; } = string.Empty;

	[JsonPropertyName("name")]
	public LocalizedText Name { get; init; } = new();

	[JsonPropertyName("url")]
	public string Url { get; init; } = string.Empty;

	[JsonPropertyName("location")]
	public string Location { get; init; } = string.Empty;

	[JsonPropertyName("weight")]
	public int Weight { get; init; }
}

internal record ProjectTabPanel
{
	[JsonPropertyName("key")]
	public string Key { get; init; } = string.Empty;

	[JsonPropertyName("name")]
	public LocalizedText Name { get; init; } = new();

	[JsonPropertyName("url")]
	public string Url { get; init; } = string.Empty;

	[JsonPropertyName("weight")]
	public int Weight { get; init; }
}

internal record WebhookHandler
{
	[JsonPropertyName("event")]
	public string Event { get; init; } = string.Empty;

	[JsonPropertyName("url")]
	public string Url { get; init; } = string.Empty;
}

internal record LocalizedText
{
	[JsonPropertyName("value")]
	public string Value { get; init; } = string.Empty;
}

internal record Icon
{
	[JsonPropertyName("width")]
	public int Width { get; init; }

	[JsonPropertyName("height")]
	public int Height { get; init; }

	[JsonPropertyName("url")]
	public string Url { get; init; } = string.Empty;
}

#endregion
