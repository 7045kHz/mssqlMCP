namespace mssqlMCP.Models;

/// <summary>
/// Available MCP endpoints for API key permissions
/// </summary>
public static class McpEndpoints
{
    // Core SQL Server Tools
    public const string Initialize = "Initialize";
    public const string ExecuteQuery = "ExecuteQuery";
    public const string GetTableMetadata = "GetTableMetadata";
    public const string GetDatabaseObjectsMetadata = "GetDatabaseObjectsMetadata";
    public const string GetDatabaseObjectsByType = "GetDatabaseObjectsByType";

    // Specialized Metadata Tools
    public const string GetSqlServerAgentJobs = "GetSqlServerAgentJobs";
    public const string GetSqlServerAgentJobDetails = "GetSqlServerAgentJobDetails";
    public const string GetSsisCatalogInfo = "GetSsisCatalogInfo";
    public const string GetAzureDevOpsInfo = "GetAzureDevOpsInfo";

    // Connection Management Tools
    public const string ListConnections = "ListConnections";
    public const string AddConnection = "AddConnection";
    public const string UpdateConnection = "UpdateConnection";
    public const string RemoveConnection = "RemoveConnection";
    public const string TestConnection = "TestConnection";

    // Security Tools
    public const string GenerateSecureKey = "GenerateSecureKey";
    public const string MigrateConnectionsToEncrypted = "MigrateConnectionsToEncrypted";
    public const string RotateKey = "RotateKey";

    // API Key Management (Master key only)
    public const string CreateApiKey = "CreateApiKey";
    public const string ListApiKeys = "ListApiKeys";
    public const string UpdateApiKey = "UpdateApiKey";
    public const string RemoveApiKey = "RemoveApiKey";
    public const string GetApiKeyInfo = "GetApiKeyInfo";

    /// <summary>
    /// Get all available endpoints
    /// </summary>
    public static readonly string[] AllEndpoints = {
        Initialize,
        ExecuteQuery,
        GetTableMetadata,
        GetDatabaseObjectsMetadata,
        GetDatabaseObjectsByType,
        GetSqlServerAgentJobs,
        GetSqlServerAgentJobDetails,
        GetSsisCatalogInfo,
        GetAzureDevOpsInfo,
        ListConnections,
        AddConnection,
        UpdateConnection,
        RemoveConnection,
        TestConnection,
        GenerateSecureKey,
        MigrateConnectionsToEncrypted,
        RotateKey,
        CreateApiKey,
        ListApiKeys,
        UpdateApiKey,
        RemoveApiKey,
        GetApiKeyInfo
    };

    /// <summary>
    /// Endpoints that require master key access
    /// </summary>
    public static readonly string[] MasterKeyOnlyEndpoints = {
        CreateApiKey,
        ListApiKeys,
        UpdateApiKey,
        RemoveApiKey,
        GetApiKeyInfo,
        GenerateSecureKey,
        MigrateConnectionsToEncrypted,
        RotateKey
    };

    /// <summary>
    /// Get endpoints available for regular API keys
    /// </summary>
    public static string[] GetRegularEndpoints()
    {
        return AllEndpoints.Except(MasterKeyOnlyEndpoints).ToArray();
    }

    /// <summary>
    /// Check if an endpoint is valid
    /// </summary>
    public static bool IsValidEndpoint(string endpoint)
    {
        return AllEndpoints.Contains(endpoint);
    }

    /// <summary>
    /// Check if an endpoint requires master key access
    /// </summary>
    public static bool RequiresMasterKey(string endpoint)
    {
        return MasterKeyOnlyEndpoints.Contains(endpoint);
    }
}
