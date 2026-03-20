using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

namespace Telenor.Api.TestExecutionManagement.Extensions;

public static class SwaggerExtensions
{
	public static IServiceCollection ConfigureSwagger(this IServiceCollection services, string title, string description)
	{
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = title,
				Description = description + $"\n### Copyright \u00a9 Telenor A/S 2020-{DateTime.Today.Year}.",
				Version = "v1"
			});

			options.CustomOperationIds(apiDescription =>
				apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
					? controllerActionDescriptor.ActionName
					: null);

			options.EnableAnnotations(
				enableAnnotationsForInheritance: true,
				enableAnnotationsForPolymorphism: true);

			options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
			{
				Name = "X-Api-Key",
				Description = "API key for authentication",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "ApiKey"
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				[new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "ApiKey"
					}
				}] = []
			});

			var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
			if (File.Exists(xmlPath))
			{
				options.IncludeXmlComments(xmlPath);
			}
		});

		return services;
	}
}