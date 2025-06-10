using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mssqlMCP.Services;
using mssqlMCP.Models;
using System;
using System.Threading.Tasks;

namespace mssqlMCP.Middleware;

/// <summary>
/// Middleware that validates API key authentication for incoming requests.
/// Supports both master key (MSSQL_MCP_API_KEY) and managed API keys with endpoint permissions.
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private readonly string _masterApiKey;
    private readonly ApiKeyManager _apiKeyManager;

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthMiddleware> logger,
        IConfiguration configuration,
        ApiKeyManager apiKeyManager)
    {
        _next = next;
        _logger = logger;
        _apiKeyManager = apiKeyManager;
        _masterApiKey = Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY") ??
            configuration["ApiSecurity:ApiKey"] ??
            "";
    }
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth check if master API key is not configured
        if (string.IsNullOrEmpty(_masterApiKey))
        {
            System.Console.WriteLine(@$"API key authentication is disabled. No master API key configured MSSQL_MCP_API_KEY = {Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY")}.");
            _logger.LogWarning("API key authentication is disabled. No master API key configured.");
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
        string? providedKey = null;
        string? authMethod = null;

        // Extract key from Bearer token if present
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

            providedKey = authHeader.Substring("Bearer ".Length).Trim();
            authMethod = "Bearer token";
        }
        // Use X-API-Key if no Bearer token
        else if (!string.IsNullOrEmpty(apiKeyHeader))
        {
            providedKey = apiKeyHeader;
            authMethod = "X-API-Key";
        }

        // Check master key first
        if (string.Equals(providedKey, _masterApiKey))
        {
            _logger.LogInformation("Successfully authenticated using master key via {AuthMethod}", authMethod);
            // Master key has access to all endpoints - store this in context for downstream use
            context.Items["IsMasterKey"] = true;
            context.Items["AuthenticatedBy"] = "MasterKey";
            await _next(context);
            return;
        }        // Check managed API keys (only if not master key)
        var endpoint = GetCurrentEndpoint(context);
        var (isValid, keyInfo, isMasterKey) = await _apiKeyManager.ValidateApiKeyAsync(providedKey ?? "");

        if (isValid && keyInfo != null)
        {
            // Check endpoint permissions for non-master keys
            if (!isMasterKey && !keyInfo.AllowedEndpoints.Contains(endpoint) && !keyInfo.AllowedEndpoints.Contains("*"))
            {
                _logger.LogWarning("API key {KeyName} does not have permission for endpoint {Endpoint}",
                    keyInfo.Name, endpoint);
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Insufficient permissions",
                    message = $"API key does not have permission to access endpoint: {endpoint}"
                });
                return;
            }

            _logger.LogInformation("Successfully authenticated using {KeyType} via {AuthMethod} for endpoint {Endpoint}",
                isMasterKey ? "master key" : "managed API key", authMethod, endpoint);

            // Store authentication details in context
            context.Items["IsMasterKey"] = isMasterKey;
            context.Items["AuthenticatedBy"] = keyInfo.Name;
            context.Items["ApiKeyInfo"] = keyInfo;

            await _next(context);
            return;
        }

        // If we get here, authentication failed
        _logger.LogWarning("Invalid authentication credentials provided via {AuthMethod} for endpoint {Endpoint}",
            authMethod, endpoint);
        context.Response.StatusCode = 403; // Forbidden
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Invalid authentication",
            message = "The provided authentication credentials are not valid"
        });
    }

    /// <summary>
    /// Extract the current endpoint from the request path
    /// </summary>
    private string GetCurrentEndpoint(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimStart('/');

        // Handle MCP tool endpoints
        if (path?.StartsWith("mcp/", StringComparison.OrdinalIgnoreCase) == true)
        {
            var segments = path.Split('/');
            if (segments.Length >= 2)
            {
                return segments[1]; // Return the tool name after "mcp/"
            }
        }

        return path ?? "unknown";
    }
}