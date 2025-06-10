using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using mssqlMCP.Models;
using mssqlMCP.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using mssqlMCP.Validation;

namespace mssqlMCP.Tools;

/// <summary>
/// MCP tool for security operations like key rotation and API key management
/// </summary>
[McpServerToolType]
public class SecurityTool
{
    private readonly ILogger<SecurityTool> _logger;
    private readonly IKeyRotationService _keyRotationService;
    private readonly IEncryptionService _encryptionService;
    private readonly ApiKeyManager _apiKeyManager;

    public SecurityTool(
        ILogger<SecurityTool> logger,
        IKeyRotationService keyRotationService,
        IEncryptionService encryptionService,
        ApiKeyManager apiKeyManager)
    {
        _logger = logger;
        _keyRotationService = keyRotationService;
        _encryptionService = encryptionService;
        _apiKeyManager = apiKeyManager;
    }/// <summary>
     /// Rotate the encryption key for all connection strings
     /// </summary>
    [McpServerTool, Description("Rotate encryption key for connection strings")]
    public async Task<object> RotateKeyAsync(string newKey)
    {
        // Validate input parameters
        var validationResult = InputValidator.ValidateEncryptionKey(newKey);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Invalid encryption key: {validationResult.ErrorMessage}";
            _logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        _logger.LogInformation("Request to rotate encryption key received");

        try
        {            // Perform key rotation
            var count = await _keyRotationService.RotateKeyAsync(newKey);

            // Return success response
            var result = new
            {
                count,
                message = "Encryption key rotated successfully. Restart the server with the new key."
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating encryption key");
            throw;
        }
    }        /// <summary>
             /// Migrate unencrypted connection strings to encrypted format
             /// </summary>
    [McpServerTool, Description("Migrate unencrypted connection strings to encrypted format")]
    public async Task<object> MigrateConnectionsToEncryptedAsync()
    {
        _logger.LogInformation("Request to migrate unencrypted connections received");

        try
        {            // Perform migration
            var count = await _keyRotationService.MigrateUnencryptedConnectionsAsync();

            // Return success response
            var result = new
            {
                count,
                message = $"Successfully migrated {count} connection strings to encrypted format"
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating connections to encrypted format");
            throw;
        }
    }    /// <summary>
         /// Generate a secure random key for encryption
         /// </summary>
    [McpServerTool, Description("Generate a secure random key for connection string encryption")]
    public object GenerateSecureKey(int length = 32)
    {
        // Validate input parameters
        if (length < 16 || length > 64)
        {
            var errorMessage = "Key length must be between 16 and 64 bytes";
            _logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage, nameof(length));
        }

        _logger.LogInformation("Request to generate secure key received");

        try
        {            // Generate a secure key
            var key = _encryptionService.GenerateSecureKey(length);

            // Return the generated key
            var result = new
            {
                key,
                length,
                message = "Generated a new secure encryption key. Use this key with the MSSQL_MCP_KEY environment variable."
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating secure key");
            throw;
        }
    }

    /// <summary>
    /// Create a new API key with specified permissions
    /// </summary>
    [McpServerTool, Description("Create a new API key with specified permissions")]
    public async Task<object> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        _logger.LogInformation("Request to create API key: {Name}", request.Name);

        try
        {
            var response = await _apiKeyManager.CreateApiKeyAsync(request);

            if (response.Success)
            {
                return new
                {
                    success = true,
                    message = response.Message,
                    apiKey = response.ApiKey,
                    keyInfo = new
                    {
                        id = response.KeyInfo?.Id,
                        name = response.KeyInfo?.Name,
                        description = response.KeyInfo?.Description,
                        allowedEndpoints = response.KeyInfo?.AllowedEndpoints,
                        createdOn = response.KeyInfo?.CreatedOn,
                        expiresOn = response.KeyInfo?.ExpiresOn
                    }
                };
            }
            else
            {
                return new
                {
                    success = false,
                    message = response.Message
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key: {Name}", request.Name);
            throw;
        }
    }

    /// <summary>
    /// List all API keys (without showing the actual keys)
    /// </summary>
    [McpServerTool, Description("List all API keys with their metadata")]
    public object ListApiKeys()
    {
        _logger.LogInformation("Request to list API keys received");

        try
        {
            var apiKeys = _apiKeyManager.ListApiKeys();

            return new
            {
                success = true,
                count = apiKeys.Count,
                apiKeys = apiKeys.Select(k => new
                {
                    id = k.Id,
                    name = k.Name,
                    description = k.Description,
                    allowedEndpoints = k.AllowedEndpoints,
                    isActive = k.IsActive,
                    createdOn = k.CreatedOn,
                    modifiedOn = k.ModifiedOn,
                    lastUsed = k.LastUsed,
                    createdBy = k.CreatedBy,
                    expiresOn = k.ExpiresOn,
                    usageCount = k.UsageCount
                })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing API keys");
            throw;
        }
    }

    /// <summary>
    /// Update an existing API key
    /// </summary>
    [McpServerTool, Description("Update an existing API key")]
    public async Task<object> UpdateApiKeyAsync(UpdateApiKeyRequest request)
    {
        _logger.LogInformation("Request to update API key: {Name}", request.Name);

        try
        {
            var success = await _apiKeyManager.UpdateApiKeyAsync(request);

            return new
            {
                success,
                message = success ? "API key updated successfully" : "Failed to update API key"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating API key: {Name}", request.Name);
            throw;
        }
    }    /// <summary>
         /// Remove an API key
         /// </summary>
    [McpServerTool, Description("Remove an API key")]
    public async Task<object> RemoveApiKeyAsync(string id)
    {
        _logger.LogInformation("Request to remove API key: {Id}", id);

        try
        {
            var success = await _apiKeyManager.RemoveApiKeyAsync(id);

            return new
            {
                success,
                message = success ? "API key removed successfully" : "Failed to remove API key or key not found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing API key: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Get information about a specific API key
    /// </summary>
    [McpServerTool, Description("Get information about a specific API key")]
    public object GetApiKeyInfo(string name)
    {
        _logger.LogInformation("Request to get API key info: {Name}", name);

        try
        {
            var apiKeys = _apiKeyManager.ListApiKeys();
            var keyInfo = apiKeys.FirstOrDefault(k => k.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (keyInfo != null)
            {
                return new
                {
                    success = true,
                    keyInfo = new
                    {
                        id = keyInfo.Id,
                        name = keyInfo.Name,
                        description = keyInfo.Description,
                        allowedEndpoints = keyInfo.AllowedEndpoints,
                        isActive = keyInfo.IsActive,
                        createdOn = keyInfo.CreatedOn,
                        modifiedOn = keyInfo.ModifiedOn,
                        lastUsed = keyInfo.LastUsed,
                        createdBy = keyInfo.CreatedBy,
                        expiresOn = keyInfo.ExpiresOn,
                        usageCount = keyInfo.UsageCount
                    }
                };
            }
            else
            {
                return new
                {
                    success = false,
                    message = $"API key with name '{name}' not found"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API key info: {Name}", name);
            throw;
        }
    }
}
