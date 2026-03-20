using Telenor.Api.TestExecutionManagement.Core.Enums;

namespace Telenor.Api.TestExecutionManagement.Core.DTOs;

public record CycleResponse(string Id, string Name, int ProjectId, int VersionId, string Status, DateTime CreatedAt, DateTime? StartDate, DateTime? EndDate);

public record CreateCycleRequest(string Name, int ProjectId, int VersionId, CycleStatus Status = CycleStatus.Draft, DateTime? StartDate = null, DateTime? EndDate = null);

public record UpdateCycleRequest(string? Name = null, CycleStatus? Status = null, DateTime? StartDate = null, DateTime? EndDate = null);