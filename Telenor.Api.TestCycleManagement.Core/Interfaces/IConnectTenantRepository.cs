using Telenor.Api.TestExecutionManagement.Core.Entities;

namespace Telenor.Api.TestExecutionManagement.Core.Interfaces;

public interface IConnectTenantRepository
{
	Task<ConnectTenant?> GetByClientKeyAsync(string clientKey, CancellationToken ct = default);
	Task UpsertAsync(ConnectTenant tenant, CancellationToken ct = default);
	Task DeactivateAsync(string clientKey, CancellationToken ct = default);
}