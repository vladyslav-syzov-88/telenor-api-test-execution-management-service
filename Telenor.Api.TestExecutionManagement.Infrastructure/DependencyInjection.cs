using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;
using Telenor.Api.TestExecutionManagement.Infrastructure.Repositories;

namespace Telenor.Api.TestExecutionManagement.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
	{
		services.AddDbContext<AppDbContext>(options =>
			options.UseSqlServer(connectionString));

		services.AddScoped<IProjectRepository, ProjectRepository>();
		services.AddScoped<ICycleRepository, CycleRepository>();
		services.AddScoped<IFolderRepository, FolderRepository>();
		services.AddScoped<IExecutionRepository, ExecutionRepository>();
		services.AddScoped<ITestCaseRepository, TestCaseRepository>();
		services.AddScoped<IDashboardRepository, DashboardRepository>();
		services.AddScoped<IConnectTenantRepository, ConnectTenantRepository>();

		return services;
	}
}