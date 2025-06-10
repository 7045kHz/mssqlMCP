using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using mssqlMCP.Models;

namespace mssqlMCP.Services;

/// <summary>
/// Service for managing API keys
/// </summary>
public class ApiKeyManager
{
    private readonly ILogger<ApiKeyManager> _logger;
    private readonly string _dataDirectory;
    private readonly string _apiKeysFilePath;
    private readonly string _encryptionKey;
    private List<ApiKeyInfo> _apiKeys;
    private readonly object _lockObject = new object();

    public ApiKeyManager(ILogger<ApiKeyManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        _apiKeysFilePath = Path.Combine(_dataDirectory, "apikeys.json");
        _encryptionKey = Environment.GetEnvironmentVariable("MSSQL_MCP_KEY") ??
                        configuration["ApiSecurity:EncryptionKey"] ??
                        "DefaultEncryptionKey123456789012345678901234567890";

        Directory.CreateDirectory(_dataDirectory);
        _apiKeys = LoadApiKeys();
    }

    /// <summary>
    /// Create a new API key
    /// </summary>
    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        try
        {
            // Validate endpoints
            var invalidEndpoints = request.AllowedEndpoints.Where(e => !McpEndpoints.IsValidEndpoint(e)).ToList();
            if (invalidEndpoints.Any())
            {
                return new CreateApiKeyResponse
                {
                    Success = false,
                    Message = $"Invalid endpoints: {string.Join(", ", invalidEndpoints)}"
                };
            }

            // Check for master-only endpoints
            var masterOnlyEndpoints = request.AllowedEndpoints.Where(McpEndpoints.RequiresMasterKey).ToList();
            if (masterOnlyEndpoints.Any())
            {
                return new CreateApiKeyResponse
                {
                    Success = false,
                    Message = $"Master key only endpoints cannot be assigned to regular API keys: {string.Join(", ", masterOnlyEndpoints)}"
                };
            }

            // Generate new API key
            var apiKey = GenerateApiKey();
            var keyHash = HashApiKey(apiKey);

            var keyInfo = new ApiKeyInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                KeyHash = keyHash,
                AllowedEndpoints = request.AllowedEndpoints,
                CreatedBy = request.CreatedBy,
                ExpiresOn = request.ExpiresOn,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };

            lock (_lockObject)
            {
                // Check for duplicate names
                if (_apiKeys.Any(k => k.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return new CreateApiKeyResponse
                    {
                        Success = false,
                        Message = $"API key with name '{request.Name}' already exists"
                    };
                }

                _apiKeys.Add(keyInfo);
                SaveApiKeys();
            }

            _logger.LogInformation("Created new API key: {Name} with {EndpointCount} endpoints",
                request.Name, request.AllowedEndpoints.Count);

            return new CreateApiKeyResponse
            {
                Success = true,
                Message = "API key created successfully",
                ApiKey = apiKey,
                KeyInfo = keyInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key: {Name}", request.Name);
            return new CreateApiKeyResponse
            {
                Success = false,
                Message = $"Error creating API key: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// List all API keys (without the actual keys)
    /// </summary>
    public List<ApiKeyInfo> ListApiKeys()
    {
        lock (_lockObject)
        {
            return _apiKeys.Select(k => new ApiKeyInfo
            {
                Id = k.Id,
                Name = k.Name,
                Description = k.Description,
                AllowedEndpoints = k.AllowedEndpoints,
                IsActive = k.IsActive,
                CreatedOn = k.CreatedOn,
                ModifiedOn = k.ModifiedOn,
                LastUsed = k.LastUsed,
                CreatedBy = k.CreatedBy,
                ExpiresOn = k.ExpiresOn,
                UsageCount = k.UsageCount,
                KeyHash = "***HIDDEN***"
            }).ToList();
        }
    }

    /// <summary>
    /// Update an existing API key
    /// </summary>
    public async Task<bool> UpdateApiKeyAsync(UpdateApiKeyRequest request)
    {
        try
        {
            lock (_lockObject)
            {
                var keyInfo = _apiKeys.FirstOrDefault(k => k.Id == request.Id);
                if (keyInfo == null)
                {
                    _logger.LogWarning("API key not found for update: {Id}", request.Id);
                    return false;
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    // Check for duplicate names
                    if (_apiKeys.Any(k => k.Id != request.Id && k.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Duplicate API key name: {Name}", request.Name);
                        return false;
                    }
                    keyInfo.Name = request.Name;
                }

                if (request.Description != null)
                    keyInfo.Description = request.Description;

                if (request.AllowedEndpoints != null)
                {
                    // Validate endpoints
                    var invalidEndpoints = request.AllowedEndpoints.Where(e => !McpEndpoints.IsValidEndpoint(e)).ToList();
                    if (invalidEndpoints.Any())
                    {
                        _logger.LogWarning("Invalid endpoints in update: {Endpoints}", string.Join(", ", invalidEndpoints));
                        return false;
                    }

                    var masterOnlyEndpoints = request.AllowedEndpoints.Where(McpEndpoints.RequiresMasterKey).ToList();
                    if (masterOnlyEndpoints.Any())
                    {
                        _logger.LogWarning("Master-only endpoints in update: {Endpoints}", string.Join(", ", masterOnlyEndpoints));
                        return false;
                    }

                    keyInfo.AllowedEndpoints = request.AllowedEndpoints;
                }

                if (request.IsActive.HasValue)
                    keyInfo.IsActive = request.IsActive.Value;

                if (request.ExpiresOn.HasValue)
                    keyInfo.ExpiresOn = request.ExpiresOn.Value;

                keyInfo.ModifiedOn = DateTime.UtcNow;
                SaveApiKeys();
            }

            _logger.LogInformation("Updated API key: {Id}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating API key: {Id}", request.Id);
            return false;
        }
    }

    /// <summary>
    /// Remove an API key
    /// </summary>
    public async Task<bool> RemoveApiKeyAsync(string id)
    {
        try
        {
            lock (_lockObject)
            {
                var keyInfo = _apiKeys.FirstOrDefault(k => k.Id == id);
                if (keyInfo == null)
                {
                    _logger.LogWarning("API key not found for removal: {Id}", id);
                    return false;
                }

                _apiKeys.Remove(keyInfo);
                SaveApiKeys();
            }

            _logger.LogInformation("Removed API key: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing API key: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Validate an API key and return its permissions
    /// </summary>
    public async Task<(bool IsValid, ApiKeyInfo? KeyInfo, bool IsMasterKey)> ValidateApiKeyAsync(string apiKey)
    {
        try
        {
            // Check if it's the master key
            var masterKey = Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY");
            if (!string.IsNullOrEmpty(masterKey) && apiKey == masterKey)
            {
                var masterKeyInfo = new ApiKeyInfo
                {
                    Id = "master",
                    Name = "Master Key",
                    Description = "Master API key with full access",
                    AllowedEndpoints = McpEndpoints.AllEndpoints.ToList(),
                    IsActive = true,
                    IsMasterKey = true,
                    LastUsed = DateTime.UtcNow
                };

                return (true, masterKeyInfo, true);
            }

            // Check regular API keys
            var keyHash = HashApiKey(apiKey);

            lock (_lockObject)
            {
                var keyInfo = _apiKeys.FirstOrDefault(k => k.KeyHash == keyHash && k.IsActive);
                if (keyInfo == null)
                {
                    return (false, null, false);
                }

                // Check expiration
                if (keyInfo.ExpiresOn.HasValue && keyInfo.ExpiresOn.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired API key used: {Name}", keyInfo.Name);
                    return (false, null, false);
                }

                // Update usage tracking
                keyInfo.LastUsed = DateTime.UtcNow;
                keyInfo.UsageCount++;
                SaveApiKeys();

                return (true, keyInfo, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key");
            return (false, null, false);
        }
    }

    /// <summary>
    /// Check if an API key has access to a specific endpoint
    /// </summary>
    public bool HasEndpointAccess(ApiKeyInfo keyInfo, string endpoint)
    {
        if (keyInfo.IsMasterKey)
            return true;

        return keyInfo.AllowedEndpoints.Contains(endpoint);
    }

    private string GenerateApiKey()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var apiKey = new string(Enumerable.Repeat(chars, 32)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return $"mcp_{apiKey}";
    }

    private string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var saltedKey = $"{apiKey}_{_encryptionKey}";
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedKey));
        return Convert.ToBase64String(hash);
    }

    private List<ApiKeyInfo> LoadApiKeys()
    {
        try
        {
            if (!File.Exists(_apiKeysFilePath))
            {
                return new List<ApiKeyInfo>();
            }

            var json = File.ReadAllText(_apiKeysFilePath);
            var keys = JsonSerializer.Deserialize<List<ApiKeyInfo>>(json) ?? new List<ApiKeyInfo>();

            _logger.LogInformation("Loaded {Count} API keys from storage", keys.Count);
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading API keys, starting with empty list");
            return new List<ApiKeyInfo>();
        }
    }

    private void SaveApiKeys()
    {
        try
        {
            var json = JsonSerializer.Serialize(_apiKeys, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_apiKeysFilePath, json);
            _logger.LogDebug("Saved {Count} API keys to storage", _apiKeys.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving API keys");
            throw;
        }
    }
}
