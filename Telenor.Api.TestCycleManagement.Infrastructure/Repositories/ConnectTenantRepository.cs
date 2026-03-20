using Microsoft.EntityFrameworkCore;
using Telenor.Api.TestExecutionManagement.Core.Entities;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

public class ConnectTenantRepository(AppDbContext db) : IConnectTenantRepository
{
	public async Task<ConnectTenant?> GetByClientKeyAsync(string clientKey, CancellationToken ct = default)
	{
		return await db.ConnectTenants
			.FirstOrDefaultAsync(t => t.ClientKey == clientKey && t.IsActive, ct);
	}

	public async Task UpsertAsync(ConnectTenant tenant, CancellationToken ct = default)
	{
		var existing = await db.ConnectTenants
			.FirstOrDefaultAsync(t => t.ClientKey == tenant.ClientKey, ct);

		if (existing is not null)
		{
			existing.SharedSecret = tenant.SharedSecret;
			existing.BaseUrl = tenant.BaseUrl;
			existing.DisplayUrl = tenant.DisplayUrl;
			existing.ProductType = tenant.ProductType;
			existing.Description = tenant.Description;
			existing.IsActive = true;
			existing.UpdatedAt = DateTime.UtcNow;
		}
		else
		{
			db.ConnectTenants.Add(tenant);
		}

		await db.SaveChangesAsync(ct);
	}

	public async Task DeactivateAsync(string clientKey, CancellationToken ct = default)
	{
		var tenant = await db.ConnectTenants
			.FirstOrDefaultAsync(t => t.ClientKey == clientKey, ct);

		if (tenant is not null)
		{
			tenant.IsActive = false;
			tenant.UpdatedAt = DateTime.UtcNow;
			await db.SaveChangesAsync(ct);
		}
	}
}