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

internal class ConnectDescriptor
{
	[JsonPropertyName("key")]
	public string Key { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("baseUrl")]
	public string BaseUrl { get; set; } = string.Empty;

	[JsonPropertyName("vendor")]
	public Vendor Vendor { get; set; } = new();

	[JsonPropertyName("authentication")]
	public Authentication Authentication { get; set; } = new();

	[JsonPropertyName("apiVersion")]
	public int ApiVersion { get; set; } = 1;

	[JsonPropertyName("lifecycle")]
	public Lifecycle Lifecycle { get; set; } = new();

	[JsonPropertyName("scopes")]
	public List<string> Scopes { get; set; } = [];

	[JsonPropertyName("modules")]
	public Modules Modules { get; set; } = new();
}

internal class Vendor
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;
}

internal class Authentication
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = "jwt";
}

internal class Lifecycle
{
	[JsonPropertyName("installed")]
	public string Installed { get; set; } = string.Empty;

	[JsonPropertyName("uninstalled")]
	public string Uninstalled { get; set; } = string.Empty;

	[JsonPropertyName("enabled")]
	public string? Enabled { get; set; }

	[JsonPropertyName("disabled")]
	public string? Disabled { get; set; }
}

internal class Modules
{
	[JsonPropertyName("generalPages")]
	public List<GeneralPage> GeneralPages { get; set; } = [];

	[JsonPropertyName("webPanels")]
	public List<WebPanel> WebPanels { get; set; } = [];

	[JsonPropertyName("jiraProjectTabPanels")]
	public List<ProjectTabPanel> JiraProjectTabPanels { get; set; } = [];

	[JsonPropertyName("webhooks")]
	public List<WebhookHandler> WebhookHandlers { get; set; } = [];
}

internal class GeneralPage
{
	[JsonPropertyName("key")]
	public string Key { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public LocalizedText Name { get; set; } = new();

	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

	[JsonPropertyName("location")]
	public string Location { get; set; } = string.Empty;

	[JsonPropertyName("icon")]
	public Icon? Icon { get; set; }
}

internal class WebPanel
{
	[JsonPropertyName("key")]
	public string Key { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public LocalizedText Name { get; set; } = new();

	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

	[JsonPropertyName("location")]
	public string Location { get; set; } = string.Empty;

	[JsonPropertyName("weight")]
	public int Weight { get; set; }
}

internal class ProjectTabPanel
{
	[JsonPropertyName("key")]
	public string Key { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public LocalizedText Name { get; set; } = new();

	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

	[JsonPropertyName("weight")]
	public int Weight { get; set; }
}

internal class WebhookHandler
{
	[JsonPropertyName("event")]
	public string Event { get; set; } = string.Empty;

	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;
}

internal class LocalizedText
{
	[JsonPropertyName("value")]
	public string Value { get; set; } = string.Empty;
}

internal class Icon
{
	[JsonPropertyName("width")]
	public int Width { get; set; }

	[JsonPropertyName("height")]
	public int Height { get; set; }

	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;
}

#endregion