using System.ComponentModel.DataAnnotations;

namespace mssqlMCP.Models;

/// <summary>
/// Request model for creating a new API key
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// Human-readable name for the API key
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the API key's purpose
    /// </summary>
    [StringLength(500)]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// List of endpoints this API key should have access to
    /// </summary>
    [Required]
    public List<string> AllowedEndpoints { get; set; } = new List<string>();

    /// <summary>
    /// Expiration date for the API key (null = no expiration)
    /// </summary>
    public DateTime? ExpiresOn
    {
        get; set;
    }

    /// <summary>
    /// Who is creating this API key
    /// </summary>
    [StringLength(100)]
    public string? CreatedBy
    {
        get; set;
    }
}

/// <summary>
/// Response model for API key creation
/// </summary>
public class CreateApiKeyResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success
    {
        get; set;
    }

    /// <summary>
    /// Result message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The generated API key (only returned once)
    /// </summary>
    public string? ApiKey
    {
        get; set;
    }

    /// <summary>
    /// The API key information (without the actual key)
    /// </summary>
    public ApiKeyInfo? KeyInfo
    {
        get; set;
    }
}

/// <summary>
/// Request model for updating an API key
/// </summary>
public class UpdateApiKeyRequest
{
    /// <summary>
    /// The ID of the API key to update
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// New name for the API key
    /// </summary>
    [StringLength(100)]
    public string? Name
    {
        get; set;
    }

    /// <summary>
    /// New description for the API key
    /// </summary>
    [StringLength(500)]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// New list of allowed endpoints
    /// </summary>
    public List<string>? AllowedEndpoints
    {
        get; set;
    }

    /// <summary>
    /// Whether to activate or deactivate the key
    /// </summary>
    public bool? IsActive
    {
        get; set;
    }

    /// <summary>
    /// New expiration date
    /// </summary>
    public DateTime? ExpiresOn
    {
        get; set;
    }
}

/// <summary>
/// Request model for removing an API key
/// </summary>
public class RemoveApiKeyRequest
{
    /// <summary>
    /// The ID of the API key to remove
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;
}
