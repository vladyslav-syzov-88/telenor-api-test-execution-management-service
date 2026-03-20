namespace Telenor.Api.TestExecutionManagement.Core.DTOs;

public record ProjectResponse(int Id, int JiraProjectId, string Name, DateTime CreatedAt);

public record CreateProjectRequest(int JiraProjectId, string Name);

public record VersionResponse(int Id, int ProjectId, string Name, bool IsReleased, DateTime? ReleaseDate, int? JiraVersionId, DateTime CreatedAt);

public record CreateVersionRequest(int ProjectId, string Name, int? JiraVersionId = null, bool IsReleased = false, DateTime? ReleaseDate = null);

public record UpdateVersionRequest(string? Name = null, bool? IsReleased = null, DateTime? ReleaseDate = null);