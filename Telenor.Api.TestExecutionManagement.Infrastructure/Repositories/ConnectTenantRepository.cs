using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.ClientKey == clientKey && t.IsActive, ct);
	}

	public async Task UpsertAsync(ConnectTenant tenant, CancellationToken ct = default)
	{
		var existing = await db.ConnectTenants
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.ClientKey == tenant.ClientKey, ct);

		if (existing is not null)
		{
			var updated = existing with
			{
				SharedSecret = tenant.SharedSecret,
				BaseUrl = tenant.BaseUrl,
				DisplayUrl = tenant.DisplayUrl,
				ProductType = tenant.ProductType,
				Description = tenant.Description,
				IsActive = true,
				UpdatedAt = DateTime.UtcNow
			};
			db.ConnectTenants.Update(updated);
		}
		else
		{
			db.ConnectTenants.Add(tenant);
		}

		await db.SaveChangesAsync(ct);
	}

	public async Task DeactivateAsync(string clientKey, CancellationToken ct = default)
	{
		await db.ConnectTenants
			.Where(t => t.ClientKey == clientKey)
			.ExecuteUpdateAsync(s => s
				.SetProperty(t => t.IsActive, false)
				.SetProperty(t => t.UpdatedAt, DateTime.UtcNow), ct);
	}
}
