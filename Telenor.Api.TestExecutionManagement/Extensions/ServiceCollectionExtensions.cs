using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telenor.Api.TestExecutionManagement.Infrastructure;

namespace Telenor.Api.TestExecutionManagement.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTestExecutionManagement(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		services.AddInfrastructure(connectionString);

		return services;
	}

	public static IMvcBuilder ConfigureInvalidModelBehavior(this IMvcBuilder mvcBuilder)
	{
		mvcBuilder.ConfigureApiBehaviorOptions(options =>
		{
			options.InvalidModelStateResponseFactory = context =>
			{
				var errors = context.ModelState
					.Where(e => e.Value?.Errors.Count > 0)
					.Select(e => $"{e.Key}: {string.Join(", ", e.Value!.Errors.Select(err => err.ErrorMessage))}")
					.ToList();

				var apiError = new Models.ApiError(
					Code: "VALIDATION_ERROR",
					Reason: "Model validation has failed",
					Message: string.Join("; ", errors));

				return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(apiError);
			};
		});

		return mvcBuilder;
	}
}