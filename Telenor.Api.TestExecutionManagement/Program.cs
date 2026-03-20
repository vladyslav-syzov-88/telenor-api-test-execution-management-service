using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telenor.Api.TestExecutionManagement.Connect;
using Telenor.Api.TestExecutionManagement.Extensions;
using Telenor.Api.TestExecutionManagement.Import;
using Telenor.Api.TestExecutionManagement.Import.Configuration;
using Telenor.Api.TestExecutionManagement.Import.ZephyrClient;
using Telenor.Api.TestExecutionManagement.Infrastructure.Data;

namespace Telenor.Api.TestExecutionManagement;

[ExcludeFromCodeCoverage]
public static class Program
{
	public static void Main(string[] args)
	{
		const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		IServiceCollection services = builder.Services;
		ConfigurationManager configuration = builder.Configuration;

		services
			.AddHttpContextAccessor()
			.AddTestExecutionManagement(configuration);

		// Zephyr import services
		services.Configure<ZephyrImportSettings>(configuration.GetSection("ZephyrImport"));
		services.AddHttpClient<ZephyrApiClient>();
		services.AddScoped<ZephyrImportService>();

		// Atlassian Connect
		services.Configure<ConnectSettings>(configuration.GetSection("AtlassianConnect"));

		services
			.AddCors(options =>
			{
				options.AddPolicy(myAllowSpecificOrigins,
					policyBuilder => { policyBuilder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod(); });
			})
			.AddControllers()
			.ConfigureInvalidModelBehavior();

		services.ConfigureSwagger(
			title: "Test Execution Management API",
			description: "## API Reference : Test Execution Management\nInternal replacement for Zephyr Squad — test execution management, tracking and reporting. Aligned with TMF708.");

		WebApplication application = builder.Build();

		// Auto-apply migrations in development
		if (application.Environment.IsDevelopment())
		{
			using var scope = application.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.Migrate();
		}

		application
			.UseSwagger()
			.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "Test Execution Management API V1");
				options.DisplayOperationId();
				options.DisplayRequestDuration();
			})
			.UseStaticFiles()
			.UseRouting()
			.UseCors(myAllowSpecificOrigins)
			.UseMiddleware<AtlassianJwtMiddleware>()
			.UseAuthorization();

		if (!application.Environment.IsDevelopment())
		{
			application.UseHttpsRedirection();
		}

		application.MapControllers();

		application.Run();
	}
}