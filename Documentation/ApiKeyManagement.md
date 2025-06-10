# API Key Management System

The SQL Server MCP server implements a **multi-tier API key management system** that provides secure access control with granular permissions. This system allows for enterprise-grade API key management with role-based access control.

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Master Key vs Managed Keys](#master-key-vs-managed-keys)
4. [Setting Up API Keys](#setting-up-api-keys)
5. [Managing API Keys](#managing-api-keys)
6. [Endpoint Permissions](#endpoint-permissions)
7. [Usage Tracking](#usage-tracking)
8. [Security Features](#security-features)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

## Overview

The API key management system provides two types of authentication:

- **Master Key**: Full access to all endpoints including API key management
- **Managed API Keys**: Limited access with configurable endpoint permissions

This allows organizations to:

- Grant limited access to specific applications or users
- Track API usage and audit access patterns
- Implement time-based access controls with expiration dates
- Maintain security through granular permission management

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Client App    │    │  Authentication  │    │   API Key       │
│                 │    │   Middleware     │    │   Manager       │
│ Bearer Token or │────▶ Validates Keys   │────▶ Permission     │
│ X-API-Key       │    │ Checks Endpoint  │    │ Validation      │
│                 │    │ Permissions      │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                │
                                ▼
                        ┌──────────────────┐
                        │   MCP Tools      │
                        │  (17 + 5 API     │
                        │   Management)    │
                        └──────────────────┘
```

### Components

1. **ApiKeyAuthMiddleware**: Intercepts requests and validates authentication
2. **ApiKeyManager**: Manages API key lifecycle and validation
3. **SecurityTool**: Provides MCP tools for API key management
4. **McpEndpoints**: Defines endpoint permissions and categories

## Master Key vs Managed Keys

### Master Key (MSSQL_MCP_API_KEY)

**Characteristics:**

- Configured via environment variable or configuration file
- Provides **full access** to all 22 MCP tools
- Can create, update, and delete managed API keys
- Cannot be disabled through the API (only through configuration)
- No expiration or usage limits

**Use Cases:**

- Administrative access
- Initial system setup
- API key management operations
- Full-featured integrations

### Managed API Keys

**Characteristics:**

- Created and managed through the master key
- **Granular permissions** - only specific endpoints
- Can have expiration dates
- Can be activated/deactivated
- Usage tracking and audit trails
- Stored in encrypted format

**Use Cases:**

- Application-specific access
- User-specific permissions
- Temporary access grants
- Read-only integrations
- Reporting services

## Setting Up API Keys

### 1. Configure Master Key

Set the master API key using one of these methods:

**Environment Variable:**

```bash
$env:MSSQL_MCP_API_KEY = "your-secure-master-key-here"
```

**PowerShell Script:**

```powershell
.\Scripts\Set-Api-Key.ps1 -ApiKey "your-secure-master-key-here"
```

**Configuration File (appsettings.json):**

```json
{
  "ApiSecurity": {
    "ApiKey": "your-secure-master-key-here"
  }
}
```

### 2. Start the MCP Server

```bash
dotnet run --project mssqlMCP.csproj
```

The server will start with master key authentication enabled.

### 3. Verify Master Key Access

Test master key access:

```bash
curl -X POST http://localhost:3001 \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-secure-master-key-here" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "ListApiKeys",
    "params": {}
  }'
```

## Managing API Keys

### Creating a New API Key

Use the master key to create managed API keys:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "CreateApiKey",
  "params": {
    "name": "ReportingService",
    "description": "Read-only access for automated reporting",
    "allowedEndpoints": ["GetTables", "GetTableInfo", "QueryDatabase"],
    "createdBy": "admin@company.com",
    "expiresOn": "2025-12-31T23:59:59Z"
  }
}
```

**Response includes the new API key:**

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "success": true,
    "message": "API key created successfully",
    "apiKey": "mcp_ABC123DEF456GHI789JKL012MNO345PQ",
    "keyInfo": {
      "id": "12345678-1234-1234-1234-123456789012",
      "name": "ReportingService",
      "allowedEndpoints": ["GetTables", "GetTableInfo", "QueryDatabase"]
    }
  }
}
```

### Listing API Keys

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "ListApiKeys",
  "params": {}
}
```

### Updating API Keys

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "UpdateApiKey",
  "params": {
    "id": "12345678-1234-1234-1234-123456789012",
    "isActive": false,
    "expiresOn": "2024-12-31T23:59:59Z"
  }
}
```

### Removing API Keys

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "RemoveApiKey",
  "params": {
    "id": "12345678-1234-1234-1234-123456789012"
  }
}
```

## Endpoint Permissions

### Available Endpoints

Managed API keys can be granted access to specific endpoints:

#### Core SQL Operations

- `Initialize` - Initialize database connection
- `ExecuteQuery` - Execute SQL queries
- `GetTableMetadata` - Get table structure information
- `GetDatabaseObjectsMetadata` - Get comprehensive metadata

#### Metadata Operations

- `GetTables` - List database tables
- `GetTableInfo` - Get detailed table information
- `GetViews` - List database views
- `GetProcedures` - List stored procedures
- `GetFunctions` - List user-defined functions

#### Connection Management

- `AddConnection` - Add new database connections
- `ListConnections` - List configured connections
- `UpdateConnection` - Update connection settings
- `RemoveConnection` - Remove connections
- `TestConnection` - Test connection validity

#### Security Operations (Non-Master)

- `GenerateSecureKey` - Generate encryption keys
- `MigrateConnectionsToEncrypted` - Migrate to encrypted format
- `RotateKey` - Rotate encryption keys

#### Master-Only Operations

The following endpoints require master key access and cannot be assigned to managed keys:

- `CreateApiKey` - Create new API keys
- `ListApiKeys` - List all API keys
- `UpdateApiKey` - Update API key properties
- `RemoveApiKey` - Remove API keys
- `GetApiKeyInfo` - Get API key information

### Permission Categories

You can also use permission categories for easier management:

- `"*"` - All available endpoints (not including master-only)
- `"read"` - Read-only operations (queries, metadata retrieval)
- `"write"` - Operations that modify data or configuration
- `"metadata"` - Metadata operations only
- `"connections"` - Connection management operations

## Usage Tracking

The system automatically tracks usage for all managed API keys:

### Tracked Metrics

- **Usage Count**: Total number of requests made with the key
- **Last Used**: Timestamp of the most recent request
- **Created On**: When the key was created
- **Modified On**: When the key was last updated

### Viewing Usage Data

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "GetApiKeyInfo",
  "params": {
    "name": "ReportingService"
  }
}
```

**Response includes usage statistics:**

```json
{
  "result": {
    "keyInfo": {
      "usageCount": 1234,
      "lastUsed": "2024-06-10T14:30:00.000Z",
      "createdOn": "2024-06-01T09:00:00.000Z"
    }
  }
}
```

## Security Features

### Encryption and Hashing

- API keys are **hashed using SHA-256** before storage
- Salted with encryption key for additional security
- Original keys are never stored in plain text

### Access Control

- **Endpoint-level permissions** prevent unauthorized access
- **Master key validation** happens before managed key validation
- **Expired keys** are automatically rejected
- **Inactive keys** can be temporarily disabled

### Audit Trail

- All API key operations are logged with timestamps
- Failed authentication attempts are logged for security monitoring
- Usage patterns can be analyzed for anomaly detection

### Best Practices Implementation

- Keys are prefixed with `mcp_` for easy identification
- Random 32-character key generation
- Comprehensive input validation
- Secure error messages that don't leak sensitive information

## Best Practices

### 1. Master Key Management

- **Use strong, randomly generated master keys**
- **Store master keys in secure environment variables**
- **Rotate master keys periodically**
- **Limit master key usage** to administrative operations only

### 2. Managed Key Strategy

- **Create specific keys for each application or service**
- **Use descriptive names** that identify the key's purpose
- **Set appropriate expiration dates** based on use case
- **Grant minimal required permissions** (principle of least privilege)

### 3. Monitoring and Maintenance

- **Regularly review API key usage** patterns
- **Remove unused or expired keys** promptly
- **Monitor for suspicious access patterns**
- **Implement key rotation schedules**

### 4. Integration Patterns

**For Applications:**

```csharp
// Use managed API keys with specific permissions
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-Key", "mcp_your_managed_key_here");
```

**For Administration:**

```csharp
// Use master key for management operations
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", masterKey);
```

## Troubleshooting

### Common Issues

#### 1. Authentication Failed (401)

**Problem**: No authentication provided
**Solution**: Add either `Authorization: Bearer <key>` or `X-API-Key: <key>` header

#### 2. Invalid Authentication (403)

**Problem**: Wrong key provided
**Solution**: Verify the API key is correct and active

#### 3. Insufficient Permissions (403)

**Problem**: Managed key doesn't have access to requested endpoint
**Solution**: Update key permissions or use master key

#### 4. Key Not Found

**Problem**: API key doesn't exist in the system
**Solution**: Verify key name/ID and check if it was removed

#### 5. Expired Key

**Problem**: API key has passed its expiration date
**Solution**: Update expiration date or create a new key

### Diagnostic Commands

**Check Master Key Configuration:**

```bash
echo $env:MSSQL_MCP_API_KEY
```

**Test Authentication:**

```bash
curl -X POST http://localhost:3001 \
  -H "X-API-Key: your-key-here" \
  -d '{"jsonrpc":"2.0","id":1,"method":"GetTables","params":{}}'
```

**List All Keys (requires master key):**

```bash
curl -X POST http://localhost:3001 \
  -H "X-API-Key: your-master-key" \
  -d '{"jsonrpc":"2.0","id":1,"method":"ListApiKeys","params":{}}'
```

### Log Analysis

Check application logs for authentication events:

- `Successfully authenticated using master key`
- `Successfully authenticated using managed API key`
- `Invalid authentication credentials provided`
- `API key does not have permission for endpoint`

## Migration from Single Key

If upgrading from a previous version with only single key authentication:

1. **Existing key becomes master key** - no changes needed
2. **Create managed keys** for specific applications
3. **Update applications** to use managed keys
4. **Monitor usage** during transition
5. **Decommission direct master key usage** in applications

This ensures a smooth transition while improving security posture.
