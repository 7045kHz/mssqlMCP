using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace mssqlMCP.Models;

/// <summary>
/// Represents an API key with its permissions and metadata
/// </summary>
public class ApiKeyInfo
{
    /// <summary>
    /// Unique identifier for the API key
    /// </summary>
    [Required]
    public string Id { get; set; } = Guid.NewGuid().ToString();

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
    /// The actual API key (encrypted when stored)
    /// </summary>
    [Required]
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// List of endpoints this API key has access to
    /// </summary>
    public List<string> AllowedEndpoints { get; set; } = new List<string>();

    /// <summary>
    /// Whether this API key is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this API key was created
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this API key was last modified
    /// </summary>
    public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this API key was last used
    /// </summary>
    public DateTime? LastUsed
    {
        get; set;
    }

    /// <summary>
    /// Who created this API key
    /// </summary>
    [StringLength(100)]
    public string? CreatedBy
    {
        get; set;
    }

    /// <summary>
    /// Expiration date for the API key (null = no expiration)
    /// </summary>
    public DateTime? ExpiresOn
    {
        get; set;
    }

    /// <summary>
    /// Number of times this API key has been used
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Whether this is a master key with full access
    /// </summary>
    [JsonIgnore]
    public bool IsMasterKey { get; set; } = false;
}
