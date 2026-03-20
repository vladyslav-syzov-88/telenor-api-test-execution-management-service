using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Telenor.Api.TestExecutionManagement.Import.Configuration;

namespace Telenor.Api.TestExecutionManagement.Import.ZephyrClient;

public static class ZephyrJwtGenerator
{
	public static string GenerateJwtToken(
		ZephyrImportSettings settings,
		string httpMethod,
		string apiEndpoint,
		Dictionary<string, string>? queryParams = null)
	{
		var canonicalString = BuildCanonicalString(httpMethod, apiEndpoint, queryParams);
		var qsh = GenerateQsh(canonicalString);

		var payload = new JwtPayload
		{
			{ "sub", settings.AccountId },
			{ "qsh", qsh },
			{ "iss", settings.AccessKey },
			{ "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
			{ "exp", DateTimeOffset.UtcNow.AddSeconds(settings.JwtValiditySeconds).ToUnixTimeSeconds() }
		};

		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
		var header = new JwtHeader(new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
		var secToken = new JwtSecurityToken(header, payload);
		var handler = new JwtSecurityTokenHandler();

		return handler.WriteToken(secToken);
	}

	private static string BuildCanonicalString(
		string httpMethod,
		string apiEndpoint,
		Dictionary<string, string>? queryParams)
	{
		var queryString = new StringBuilder();
		if (queryParams is { Count: > 0 })
		{
			var sorted = new SortedDictionary<string, string>(queryParams);
			foreach (var param in sorted)
			{
				queryString.Append($"{param.Key}={param.Value}&");
			}
			queryString.Length--;
		}

		return $"{httpMethod.ToUpper()}&{apiEndpoint}&{queryString}";
	}

	private static string GenerateQsh(string canonicalString)
	{
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalString));
		return Convert.ToHexStringLower(hash);
	}
}