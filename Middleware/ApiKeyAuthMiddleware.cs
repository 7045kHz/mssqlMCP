using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace mssqlMCP.Middleware;

/// <summary>
/// Middleware that validates Bearer token authentication for incoming requests
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private readonly string _apiKey;

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY") ??
            configuration["ApiSecurity:ApiKey"] ??
            "";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth check if API key is not configured
        if (string.IsNullOrEmpty(_apiKey))
        {
            System.Console.WriteLine(@$"API key authentication is disabled. No API key configured MSSQL_MCP_API_KEY = {Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY")}.");
            _logger.LogWarning("API key authentication is disabled. No API key configured.");
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var apiKeyHeader = context.Request.Headers["X-API-Key"].ToString();

        // Check for either Bearer token or X-API-Key
        if (string.IsNullOrEmpty(authHeader) && string.IsNullOrEmpty(apiKeyHeader))
        {
            _logger.LogWarning("No authentication provided - neither Bearer token nor X-API-Key present");
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Authentication required",
                message = "Please provide either a Bearer token in the Authorization header or an API key in the X-API-Key header"
            });
            return;
        }

        // Check Bearer token if present
        if (!string.IsNullOrEmpty(authHeader))
        {
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Authorization header format");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid Authorization format",
                    message = "Authorization header must use Bearer scheme"
                });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.Equals(token, _apiKey))
            {
                _logger.LogInformation("Successfully authenticated using Bearer token");
                await _next(context);
                return;
            }
        }

        // Check X-API-Key if present
        if (!string.IsNullOrEmpty(apiKeyHeader))
        {
            if (string.Equals(apiKeyHeader, _apiKey))
            {
                _logger.LogInformation("Successfully authenticated using X-API-Key");
                await _next(context);
                return;
            }
        }

        // If we get here, neither authentication method was successful
        _logger.LogWarning("Invalid authentication credentials provided");
        context.Response.StatusCode = 403; // Forbidden
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Invalid authentication",
            message = "The provided authentication credentials are not valid"
        });
    }
}