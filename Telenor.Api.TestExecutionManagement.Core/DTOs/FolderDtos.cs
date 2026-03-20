namespace Telenor.Api.TestExecutionManagement.Core.DTOs;

public record FolderResponse(string Id, string Name, string CycleId, int SortOrder);

public record CreateFolderRequest(string Name, int SortOrder = 0);

public record UpdateFolderRequest(string? Name = null, int? SortOrder = null);