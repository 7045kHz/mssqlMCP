# SQL Server MCP API Endpoints Documentation

This document provides a comprehensive overview of all API endpoints, usage examples, and features available in the SQL Server Model Context Protocol (MCP) server.

## Table of Contents

1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Core SQL Server Tools](#core-sql-server-tools)
4. [Specialized Metadata Tools](#specialized-metadata-tools)
5. [Connection Management Tools](#connection-management-tools)
6. [Security Tools](#security-tools)
7. [Request/Response Format](#requestresponse-format)
8. [Error Handling](#error-handling)
9. [Usage Examples](#usage-examples)
10. [Features Summary](#features-summary)

## Overview

The SQL Server MCP server exposes **17 MCP tools** through a single HTTP endpoint using JSON-RPC 2.0 protocol. All operations are performed via HTTP POST requests to the base URL.

**Base URL**: `http://localhost:3001`  
**Protocol**: JSON-RPC 2.0 over HTTP POST  
**Authentication**: Bearer token or X-API-Key header (optional)

## Authentication

The server supports two authentication methods:

### Bearer Token Authentication

```http
Authorization: Bearer <your-api-key>
```

### X-API-Key Header Authentication

```http
X-API-Key: <your-api-key>
```

**Configuration**:

- Environment variable: `MSSQL_MCP_API_KEY`
- Configuration file: `appsettings.json` under `ApiSecurity.ApiKey`
- Authentication is optional if no API key is configured

## Core SQL Server Tools

### 1. Initialize

**Method**: `Initialize`  
**Description**: Initialize a SQL Server connection with timeout and error handling

**Parameters**:

- `connectionName` (string, optional): Connection name (default: "DefaultConnection")

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "Initialize",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

### 2. ExecuteQuery

**Method**: `ExecuteQuery`  
**Description**: Execute SQL queries with JSON result formatting and cancellation support

**Parameters**:

- `query` (string, required): SQL query to execute
- `connectionName` (string, optional): Connection name (default: "DefaultConnection")

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "ExecuteQuery",
  "params": {
    "query": "SELECT TOP 10 * FROM sys.databases",
    "connectionName": "DefaultConnection"
  }
}
```

### 3. GetTableMetadata

**Method**: `GetTableMetadata`  
**Description**: Get detailed table metadata with columns, primary keys, foreign keys, and schema filtering

**Parameters**:

- `connectionName` (string, optional): Connection name (default: "DefaultConnection")
- `schema` (string, optional): Schema filter (null for all schemas)

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "GetTableMetadata",
  "params": {
    "connectionName": "DefaultConnection",
    "schema": "dbo"
  }
}
```

### 4. GetDatabaseObjectsMetadata

**Method**: `GetDatabaseObjectsMetadata`  
**Description**: Get complete database object metadata with view filtering options

**Parameters**:

- `connectionName` (string, optional): Connection name (default: "DefaultConnection")
- `schema` (string, optional): Schema filter (null for all schemas)
- `includeViews` (boolean, optional): Include views in results (default: true)

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "GetDatabaseObjectsMetadata",
  "params": {
    "connectionName": "DefaultConnection",
    "schema": "dbo",
    "includeViews": true
  }
}
```

### 5. GetDatabaseObjectsByType

**Method**: `GetDatabaseObjectsByType`  
**Description**: Get database objects filtered by specific type (TABLE, VIEW, PROCEDURE, FUNCTION, ALL)

**Parameters**:

- `connectionName` (string, optional): Connection name (default: "DefaultConnection")
- `schema` (string, optional): Schema filter (null for all schemas)
- `objectType` (string, optional): Object type filter (default: "ALL")
  - Valid values: "TABLE", "VIEW", "PROCEDURE", "FUNCTION", "ALL"

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "GetDatabaseObjectsByType",
  "params": {
    "connectionName": "DefaultConnection",
    "objectType": "TABLE",
    "schema": "dbo"
  }
}
```

## Specialized Metadata Tools

### 6. GetSqlServerAgentJobs

**Method**: `GetSqlServerAgentJobs`  
**Description**: Get SQL Server Agent job metadata from msdb with job status and ownership

**Parameters**:

- `connectionName` (string, optional): Connection name (default: "DefaultConnection")

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "method": "GetSqlServerAgentJobs",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

### 7. GetSqlServerAgentJobDetails

**Method**: `GetSqlServerAgentJobDetails`  
**Description**: Get detailed job information including steps, schedules, and execution history

**Parameters**:

- `jobName` (string, required): Name of the SQL Server Agent job
- `connectionName` (string, optional): Connection name (default: "DefaultConnection")

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "method": "GetSqlServerAgentJobDetails",
  "params": {
    "jobName": "DatabaseBackup",
    "connectionName": "DefaultConnection"
  }
}
```

### 8. GetSsisCatalogInfo

**Method**: `GetSsisCatalogInfo`  
**Description**: Get SSIS catalog metadata with Project/Package deployment models, folders, and packages

**Parameters**:

- `connectionName` (string, optional): Connection name (default: "DefaultConnection")

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "GetSsisCatalogInfo",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

### 9. GetAzureDevOpsInfo

**Method**: `GetAzureDevOpsInfo`  
**Description**: Get Azure DevOps analytics including projects, repositories, builds, and work items

**Parameters**:

- `connectionName` (string, optional): Connection name (default: "DefaultConnection")

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "method": "GetAzureDevOpsInfo",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

## Connection Management Tools

### 10. AddConnection

**Method**: `AddConnection`  
**Description**: Add new database connections with validation

**Parameters**:

- `request` (object, required):
  - `name` (string, required): Connection name
  - `connectionString` (string, required): SQL Server connection string
  - `description` (string, optional): Connection description

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "method": "AddConnection",
  "params": {
    "request": {
      "name": "AdventureWorks",
      "connectionString": "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;",
      "description": "AdventureWorks sample database"
    }
  }
}
```

### 11. UpdateConnection

**Method**: `UpdateConnection`  
**Description**: Modify existing connection configurations

**Parameters**:

- `request` (object, required):
  - `name` (string, required): Connection name to update
  - `connectionString` (string, required): New connection string
  - `description` (string, optional): New description

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "method": "UpdateConnection",
  "params": {
    "request": {
      "name": "AdventureWorks",
      "connectionString": "Server=localhost;Database=AdventureWorks2022;Trusted_Connection=True;",
      "description": "Updated AdventureWorks database"
    }
  }
}
```

### 12. RemoveConnection

**Method**: `RemoveConnection`  
**Description**: Delete database connections

**Parameters**:

- `request` (object, required):
  - `name` (string, required): Connection name to remove

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "method": "RemoveConnection",
  "params": {
    "request": {
      "name": "AdventureWorks"
    }
  }
}
```

### 13. ListConnections

**Method**: `ListConnections`  
**Description**: Enumerate all available connections with metadata

**Parameters**: None

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 13,
  "method": "ListConnections",
  "params": {}
}
```

### 14. TestConnection

**Method**: `TestConnection`  
**Description**: Validate connection strings and connectivity

**Parameters**:

- `request` (object, required):
  - `connectionString` (string, required): Connection string to test

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 14,
  "method": "TestConnection",
  "params": {
    "request": {
      "connectionString": "Server=localhost;Database=master;Trusted_Connection=True;"
    }
  }
}
```

## Security Tools

### 15. GenerateSecureKey

**Method**: `GenerateSecureKey`  
**Description**: Generate AES-256 encryption keys for connection security

**Parameters**:

- `length` (integer, optional): Key length in bytes (default: 32, range: 16-64)

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 15,
  "method": "GenerateSecureKey",
  "params": {
    "length": 32
  }
}
```

### 16. MigrateConnectionsToEncrypted

**Method**: `MigrateConnectionsToEncrypted`  
**Description**: Migrate unencrypted connections to encrypted format

**Parameters**: None

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 16,
  "method": "MigrateConnectionsToEncrypted",
  "params": {}
}
```

### 17. RotateKey

**Method**: `RotateKey`  
**Description**: Rotate encryption keys with validation and connection testing

**Parameters**:

- `newKey` (string, required): New encryption key (16-64 bytes)

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 17,
  "method": "RotateKey",
  "params": {
    "newKey": "your-new-encryption-key-here"
  }
}
```

## Request/Response Format

### Request Format

All requests follow the JSON-RPC 2.0 specification:

```json
{
  "jsonrpc": "2.0",
  "id": <unique-identifier>,
  "method": "<method-name>",
  "params": {
    // Method-specific parameters
  }
}
```

### Success Response Format

```json
{
  "jsonrpc": "2.0",
  "id": <request-id>,
  "result": {
    // Method-specific result data
  }
}
```

### Error Response Format

```json
{
  "jsonrpc": "2.0",
  "id": <request-id>,
  "error": {
    "code": <error-code>,
    "message": "<error-message>",
    "data": {
      // Additional error details (optional)
    }
  }
}
```

## Error Handling

### HTTP Status Codes

- **200**: Success (JSON-RPC response)
- **401**: Unauthorized (missing or malformed authentication)
- **403**: Forbidden (invalid authentication credentials)
- **406**: Not Acceptable (invalid content type)

### Common Error Scenarios

1. **Authentication Errors**: Invalid or missing API key
2. **Connection Errors**: Database unreachable or invalid connection string
3. **Validation Errors**: Invalid input parameters
4. **Database Errors**: SQL execution failures or permission issues
5. **Timeout Errors**: Long-running operations that exceed timeout limits

## Usage Examples

### PowerShell Example

```powershell
# Set API key
$apiKey = "your-api-key-here"
$headers = @{
    "Authorization" = "Bearer $apiKey"
    "Content-Type" = "application/json"
}

# Execute query
$body = @{
    jsonrpc = "2.0"
    id = 1
    method = "ExecuteQuery"
    params = @{
        query = "SELECT name FROM sys.databases"
        connectionName = "DefaultConnection"
    }
} | ConvertTo-Json -Depth 3

$response = Invoke-RestMethod -Uri "http://localhost:3001/" -Method Post -Headers $headers -Body $body
```

### curl Example

```bash
# Using Bearer token
curl -X POST http://localhost:3001/ \
  -H "Authorization: Bearer your-api-key-here" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "GetTableMetadata",
    "params": {
      "connectionName": "DefaultConnection",
      "schema": "dbo"
    }
  }'

# Using X-API-Key header
curl -X POST http://localhost:3001/ \
  -H "X-API-Key: your-api-key-here" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "ListConnections",
    "params": {}
  }'
```

### JavaScript Example

```javascript
async function callMcpApi(method, params = {}) {
  const response = await fetch("http://localhost:3001/", {
    method: "POST",
    headers: {
      Authorization: "Bearer your-api-key-here",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      jsonrpc: "2.0",
      id: Date.now(),
      method: method,
      params: params,
    }),
  });

  return response.json();
}

// Usage
const result = await callMcpApi("GetDatabaseObjectsMetadata", {
  connectionName: "DefaultConnection",
  includeViews: true,
});
```

## Features Summary

### Core Capabilities

- **17 MCP Tools**: Comprehensive SQL Server integration
- **JSON-RPC 2.0**: Standard protocol implementation
- **Dual Authentication**: Bearer token and X-API-Key support
- **Connection Management**: Full CRUD operations for database connections
- **Security Features**: Encryption, key rotation, and secure key generation

### Database Integration

- **Metadata Retrieval**: Tables, views, procedures, functions, and relationships
- **SQL Execution**: Direct query execution with JSON results
- **Schema Filtering**: Granular control over metadata scope
- **Specialized Tools**: SQL Server Agent, SSIS, and Azure DevOps integration

### Enterprise Features

- **AES-256 Encryption**: Connection string encryption at rest
- **Key Rotation**: Automated encryption key management
- **Comprehensive Logging**: Structured logging with Serilog
- **Error Handling**: Robust error handling with appropriate HTTP status codes
- **Input Validation**: Comprehensive parameter validation
- **Timeout Management**: Configurable timeouts for database operations

### Development Tools

- **VS Code Integration**: Copilot Agent support
- **Docker Support**: Containerized deployment
- **CORS Configuration**: Web application integration
- **PowerShell Scripts**: Administrative automation
- **Comprehensive Testing**: Unit tests, integration tests, and validation scripts

### Security Best Practices

- **Optional Authentication**: Flexible security configuration
- **Secure Key Storage**: Environment variable and configuration support
- **SQL Injection Prevention**: Parameterized queries
- **Connection Encryption**: Encrypted database connections
- **Audit Logging**: Security event logging

This documentation covers all available endpoints and features in the SQL Server MCP server. For additional examples and detailed usage information, refer to the example scripts in the `Examples/` directory and the comprehensive documentation in the `Documentation/` folder.
