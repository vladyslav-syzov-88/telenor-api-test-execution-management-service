using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Telenor.Api.TestExecutionManagement.Core.Interfaces;

namespace Telenor.Api.TestExecutionManagement.Connect;

public class AtlassianJwtMiddleware(RequestDelegate next, ILogger<AtlassianJwtMiddleware> logger)
{
	private static readonly string[] SkipPaths =
	[
		"/atlassian-connect.json",
		"/api/connect/installed",
		"/api/connect/uninstalled",
		"/swagger",
		"/api/"
	];

	public async Task InvokeAsync(HttpContext context)
	{
		string path = context.Request.Path.Value ?? string.Empty;

		// Skip JWT verification for non-Connect paths and lifecycle endpoints
		if (!path.StartsWith("/connect/", StringComparison.OrdinalIgnoreCase) ||
			SkipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
		{
			await next(context);
			return;
		}

		// Extract JWT from query string (Atlassian sends it as ?jwt=...)
		string? jwtToken = context.Request.Query["jwt"].FirstOrDefault();

		if (string.IsNullOrEmpty(jwtToken))
		{
			// Also check Authorization header
			string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();
			if (authHeader is not null && authHeader.StartsWith("JWT ", StringComparison.OrdinalIgnoreCase))
			{
				jwtToken = authHeader["JWT ".Length..];
			}
		}

		if (string.IsNullOrEmpty(jwtToken))
		{
			logger.LogWarning("Connect iframe request to {Path} missing JWT token", path);
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}

		// Decode JWT without verification first to get the issuer (clientKey)
		var handler = new JwtSecurityTokenHandler();
		JwtSecurityToken unverifiedToken;
		try
		{
			unverifiedToken = handler.ReadJwtToken(jwtToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Failed to read JWT token for Connect request to {Path}", path);
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}

		string? clientKey = unverifiedToken.Issuer;
		if (string.IsNullOrEmpty(clientKey))
		{
			logger.LogWarning("JWT token missing issuer for Connect request to {Path}", path);
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}

		// Look up the shared secret for this tenant
		var tenantRepository = context.RequestServices.GetRequiredService<IConnectTenantRepository>();
		var tenant = await tenantRepository.GetByClientKeyAsync(clientKey);

		if (tenant is null)
		{
			logger.LogWarning("Unknown Connect tenant {ClientKey} for request to {Path}", clientKey, path);
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}

		// Verify the JWT signature
		try
		{
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tenant.SharedSecret)),
				ValidateIssuer = true,
				ValidIssuer = clientKey,
				ValidateAudience = false,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.FromMinutes(2)
			};

			ClaimsPrincipal principal = handler.ValidateToken(jwtToken, validationParameters, out _);
			context.User = principal;
			context.Items["ConnectClientKey"] = clientKey;
			context.Items["ConnectBaseUrl"] = tenant.BaseUrl;
		}
		catch (SecurityTokenException ex)
		{
			logger.LogWarning(ex, "JWT validation failed for tenant {ClientKey} on {Path}", clientKey, path);
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}

		await next(context);
	}
}