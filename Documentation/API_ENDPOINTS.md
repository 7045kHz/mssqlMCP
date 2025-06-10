# SQL Server MCP API Endpoints Documentation

This document provides a comprehensive overview of all API endpoints, usage examples, and features available in the SQL Server Model Context Protocol (MCP) server.

## Table of Contents

1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Core SQL Server Tools](#core-sql-server-tools)
4. [Specialized Metadata Tools](#specialized-metadata-tools)
5. [Connection Management Tools](#connection-management-tools)
6. [Security Tools](#security-tools)
7. [API Key Management Tools](#api-key-management-tools)
8. [Request/Response Format](#requestresponse-format)
9. [Error Handling](#error-handling)
10. [Usage Examples](#usage-examples)
11. [Features Summary](#features-summary)

## Overview

The SQL Server MCP server exposes **22 MCP tools** through a single HTTP endpoint using JSON-RPC 2.0 protocol. All operations are performed via HTTP POST requests to the base URL.

**Base URL**: `http://localhost:3001`  
**Protocol**: JSON-RPC 2.0 over HTTP POST  
**Authentication**: Bearer token or X-API-Key header (optional)

## Authentication

The server supports a **multi-tier API key management system** with two types of authentication:

### Master Key Authentication

The master key provides **full access** to all endpoints, including API key management functions.

#### Bearer Token Authentication

```http
Authorization: Bearer <your-master-api-key>
```

#### X-API-Key Header Authentication

```http
X-API-Key: <your-master-api-key>
```

**Master Key Configuration**:

- Environment variable: `MSSQL_MCP_API_KEY`
- Configuration file: `appsettings.json` under `ApiSecurity.ApiKey`
- Authentication is optional if no API key is configured

### Managed API Keys

Managed API keys provide **granular access control** with endpoint-specific permissions. These keys are created and managed through the master key.

#### Features:

- **Endpoint-specific permissions**: Only access allowed endpoints
- **Expiration dates**: Keys can have expiration times
- **Usage tracking**: Monitor key usage and last access times
- **Activation/deactivation**: Keys can be enabled or disabled
- **Audit trail**: Track who created keys and when

#### Creating Managed API Keys:

Use the `CreateApiKey` tool (requires master key access) to create managed keys with specific permissions.

#### Using Managed API Keys:

```http
Authorization: Bearer <your-managed-api-key>
# OR
X-API-Key: <your-managed-api-key>
```

**Important Notes**:

- Managed API keys are validated against their allowed endpoints on each request
- Master key always takes precedence during validation
- Invalid or expired keys receive 403 Forbidden responses
- Keys without endpoint permissions receive 403 Insufficient Permissions responses

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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "success": true,
    "message": "Connection initialized successfully",
    "connectionName": "DefaultConnection",
    "serverVersion": "Microsoft SQL Server 2022 (RTM) - 16.0.1000.6",
    "databaseName": "master"
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": [
    {
      "database_id": 1,
      "name": "master",
      "source_database_id": null,
      "owner_sid": "0x01",
      "create_date": "2022-04-08T15:13:58.453",
      "compatibility_level": 160,
      "collation_name": "SQL_Latin1_General_CP1_CI_AS",
      "user_access": 0,
      "user_access_desc": "MULTI_USER",
      "is_read_only": false,
      "is_auto_close_on": false,
      "is_auto_shrink_on": false,
      "state": 0,
      "state_desc": "ONLINE"
    },
    {
      "database_id": 2,
      "name": "tempdb",
      "source_database_id": null,
      "owner_sid": "0x01",
      "create_date": "2024-06-10T08:30:15.123",
      "compatibility_level": 160,
      "collation_name": "SQL_Latin1_General_CP1_CI_AS",
      "user_access": 0,
      "user_access_desc": "MULTI_USER",
      "is_read_only": false,
      "is_auto_close_on": false,
      "is_auto_shrink_on": false,
      "state": 0,
      "state_desc": "ONLINE"
    },
    {
      "database_id": 3,
      "name": "model",
      "source_database_id": null,
      "owner_sid": "0x01",
      "create_date": "2022-04-08T15:13:58.457",
      "compatibility_level": 160,
      "collation_name": "SQL_Latin1_General_CP1_CI_AS",
      "user_access": 0,
      "user_access_desc": "MULTI_USER",
      "is_read_only": false,
      "is_auto_close_on": false,
      "is_auto_shrink_on": false,
      "state": 0,
      "state_desc": "ONLINE"
    },
    {
      "database_id": 4,
      "name": "msdb",
      "source_database_id": null,
      "owner_sid": "0x01",
      "create_date": "2022-04-08T15:13:58.457",
      "compatibility_level": 160,
      "collation_name": "SQL_Latin1_General_CP1_CI_AS",
      "user_access": 0,
      "user_access_desc": "MULTI_USER",
      "is_read_only": false,
      "is_auto_close_on": false,
      "is_auto_shrink_on": false,
      "state": 0,
      "state_desc": "ONLINE"
    },
    {
      "database_id": 5,
      "name": "AdventureWorks2022",
      "source_database_id": null,
      "owner_sid": "0x01020000000000052000000021020000",
      "create_date": "2023-05-15T10:22:33.890",
      "compatibility_level": 160,
      "collation_name": "SQL_Latin1_General_CP1_CI_AS",
      "user_access": 0,
      "user_access_desc": "MULTI_USER",
      "is_read_only": false,
      "is_auto_close_on": false,
      "is_auto_shrink_on": false,
      "state": 0,
      "state_desc": "ONLINE"
    }
  ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": [
    {
      "tableName": "Customers",
      "schemaName": "dbo",
      "columns": [
        {
          "columnName": "CustomerID",
          "dataType": "int",
          "isNullable": false,
          "maxLength": null,
          "precision": 10,
          "scale": 0
        },
        {
          "columnName": "CustomerName",
          "dataType": "nvarchar",
          "isNullable": false,
          "maxLength": 100,
          "precision": null,
          "scale": null
        },
        {
          "columnName": "Email",
          "dataType": "nvarchar",
          "isNullable": true,
          "maxLength": 255,
          "precision": null,
          "scale": null
        },
        {
          "columnName": "CreatedDate",
          "dataType": "datetime2",
          "isNullable": false,
          "maxLength": null,
          "precision": 27,
          "scale": 7
        }
      ],
      "primaryKeys": ["CustomerID"],
      "foreignKeys": []
    },
    {
      "tableName": "Orders",
      "schemaName": "dbo",
      "columns": [
        {
          "columnName": "OrderID",
          "dataType": "int",
          "isNullable": false,
          "maxLength": null,
          "precision": 10,
          "scale": 0
        },
        {
          "columnName": "CustomerID",
          "dataType": "int",
          "isNullable": false,
          "maxLength": null,
          "precision": 10,
          "scale": 0
        },
        {
          "columnName": "OrderDate",
          "dataType": "datetime2",
          "isNullable": false,
          "maxLength": null,
          "precision": 27,
          "scale": 7
        },
        {
          "columnName": "TotalAmount",
          "dataType": "decimal",
          "isNullable": true,
          "maxLength": null,
          "precision": 18,
          "scale": 2
        }
      ],
      "primaryKeys": ["OrderID"],
      "foreignKeys": [
        {
          "constraintName": "FK_Orders_Customers",
          "columnName": "CustomerID",
          "referencedTable": "Customers",
          "referencedColumn": "CustomerID"
        }
      ]
    }
  ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "result": {
    "tables": [
      {
        "objectName": "Customers",
        "objectType": "TABLE",
        "schemaName": "dbo",
        "createDate": "2023-05-15T10:22:33.890",
        "modifyDate": "2023-05-15T10:22:33.890",
        "rowCount": 1250
      },
      {
        "objectName": "Orders",
        "objectType": "TABLE",
        "schemaName": "dbo",
        "createDate": "2023-05-15T10:22:33.890",
        "modifyDate": "2024-06-09T14:30:22.123",
        "rowCount": 5420
      }
    ],
    "views": [
      {
        "objectName": "CustomerOrderSummary",
        "objectType": "VIEW",
        "schemaName": "dbo",
        "createDate": "2023-06-20T09:15:45.678",
        "modifyDate": "2024-01-15T11:20:33.456",
        "definition": "SELECT c.CustomerID, c.CustomerName, COUNT(o.OrderID) as OrderCount FROM dbo.Customers c LEFT JOIN dbo.Orders o ON c.CustomerID = o.CustomerID GROUP BY c.CustomerID, c.CustomerName"
      }
    ],
    "procedures": [
      {
        "objectName": "GetCustomerOrders",
        "objectType": "PROCEDURE",
        "schemaName": "dbo",
        "createDate": "2023-07-10T16:45:12.789",
        "modifyDate": "2024-03-22T13:55:18.234",
        "definition": "CREATE PROCEDURE [dbo].[GetCustomerOrders] @CustomerID int AS BEGIN SELECT * FROM Orders WHERE CustomerID = @CustomerID END"
      }
    ],
    "functions": [
      {
        "objectName": "CalculateOrderTotal",
        "objectType": "FUNCTION",
        "schemaName": "dbo",
        "createDate": "2023-08-05T14:20:30.567",
        "modifyDate": "2023-08-05T14:20:30.567",
        "definition": "CREATE FUNCTION [dbo].[CalculateOrderTotal](@OrderID int) RETURNS decimal(18,2) AS BEGIN DECLARE @Total decimal(18,2) SELECT @Total = SUM(Quantity * UnitPrice) FROM OrderDetails WHERE OrderID = @OrderID RETURN ISNULL(@Total, 0) END"
      }
    ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "result": [
    {
      "objectName": "Customers",
      "objectType": "TABLE",
      "schemaName": "dbo",
      "objectId": 245575913,
      "createDate": "2023-05-15T10:22:33.890",
      "modifyDate": "2023-05-15T10:22:33.890",
      "rowCount": 1250,
      "dataLength": 125000,
      "indexLength": 8192,
      "hasIdentity": true,
      "hasPrimaryKey": true,
      "hasForeignKey": false,
      "hasCheckConstraint": true,
      "hasDefaultConstraint": true
    },
    {
      "objectName": "Orders",
      "objectType": "TABLE",
      "schemaName": "dbo",
      "objectId": 261575970,
      "createDate": "2023-05-15T10:22:33.890",
      "modifyDate": "2024-06-09T14:30:22.123",
      "rowCount": 5420,
      "dataLength": 542000,
      "indexLength": 16384,
      "hasIdentity": true,
      "hasPrimaryKey": true,
      "hasForeignKey": true,
      "hasCheckConstraint": false,
      "hasDefaultConstraint": true
    },
    {
      "objectName": "OrderDetails",
      "objectType": "TABLE",
      "schemaName": "dbo",
      "objectId": 277576027,
      "createDate": "2023-05-15T10:22:33.890",
      "modifyDate": "2024-06-10T09:45:11.567",
      "rowCount": 18760,
      "dataLength": 1876000,
      "indexLength": 32768,
      "hasIdentity": true,
      "hasPrimaryKey": true,
      "hasForeignKey": true,
      "hasCheckConstraint": true,
      "hasDefaultConstraint": false
    }
  ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "result": [
    {
      "jobId": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
      "jobName": "DatabaseBackup",
      "enabled": true,
      "description": "Daily backup of all user databases",
      "owner": "sa",
      "categoryName": "Database Maintenance",
      "dateCreated": "2023-05-15T10:22:33.890",
      "dateModified": "2024-06-01T08:30:15.123",
      "lastRunDate": "2024-06-09T23:00:00.000",
      "lastRunStatus": "Succeeded",
      "lastRunDuration": 1245,
      "nextRunDate": "2024-06-10T23:00:00.000",
      "currentExecutionStatus": "Idle",
      "stepCount": 3,
      "scheduleCount": 1
    },
    {
      "jobId": "B2C3D4E5-F6G7-8901-BCDE-F23456789012",
      "jobName": "IndexMaintenance",
      "enabled": true,
      "description": "Weekly index rebuild and reorganization",
      "owner": "domain\\sqladmin",
      "categoryName": "Database Maintenance",
      "dateCreated": "2023-06-20T14:15:22.456",
      "dateModified": "2024-05-15T16:45:33.789",
      "lastRunDate": "2024-06-08T02:00:00.000",
      "lastRunStatus": "Succeeded",
      "lastRunDuration": 3672,
      "nextRunDate": "2024-06-15T02:00:00.000",
      "currentExecutionStatus": "Idle",
      "stepCount": 5,
      "scheduleCount": 1
    },
    {
      "jobId": "C3D4E5F6-G7H8-9012-CDEF-345678901234",
      "jobName": "DataArchival",
      "enabled": false,
      "description": "Archive old transaction data",
      "owner": "domain\\dataadmin",
      "categoryName": "Data Collector",
      "dateCreated": "2023-08-10T09:30:45.678",
      "dateModified": "2024-04-20T11:20:18.234",
      "lastRunDate": "2024-04-15T01:00:00.000",
      "lastRunStatus": "Failed",
      "lastRunDuration": 156,
      "nextRunDate": null,
      "currentExecutionStatus": "Idle",
      "stepCount": 2,
      "scheduleCount": 0
    }
  ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "result": {
    "job": {
      "jobId": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
      "jobName": "DatabaseBackup",
      "enabled": true,
      "description": "Daily backup of all user databases",
      "owner": "sa",
      "categoryName": "Database Maintenance",
      "dateCreated": "2023-05-15T10:22:33.890",
      "dateModified": "2024-06-01T08:30:15.123"
    },
    "steps": [
      {
        "stepId": 1,
        "stepName": "Backup User Databases",
        "stepType": "T-SQL",
        "command": "EXEC sp_BackupDatabases @BackupDirectory = 'C:\\Backups\\', @BackupType = 'FULL'",
        "onSuccessAction": "Go to next step",
        "onFailureAction": "Quit job reporting failure",
        "retryAttempts": 3,
        "retryInterval": 5,
        "lastRunDate": "2024-06-09T23:00:15.123",
        "lastRunDuration": 845,
        "lastRunStatus": "Succeeded"
      },
      {
        "stepId": 2,
        "stepName": "Verify Backup Files",
        "stepType": "T-SQL",
        "command": "EXEC sp_VerifyBackupFiles @BackupDirectory = 'C:\\Backups\\'",
        "onSuccessAction": "Go to next step",
        "onFailureAction": "Quit job reporting failure",
        "retryAttempts": 1,
        "retryInterval": 0,
        "lastRunDate": "2024-06-09T23:14:20.456",
        "lastRunDuration": 123,
        "lastRunStatus": "Succeeded"
      },
      {
        "stepId": 3,
        "stepName": "Send Notification",
        "stepType": "PowerShell",
        "command": "Send-MailMessage -To 'dba@company.com' -Subject 'Backup Completed' -Body 'Database backup completed successfully'",
        "onSuccessAction": "Quit job reporting success",
        "onFailureAction": "Quit job reporting success",
        "retryAttempts": 0,
        "retryInterval": 0,
        "lastRunDate": "2024-06-09T23:16:35.789",
        "lastRunDuration": 12,
        "lastRunStatus": "Succeeded"
      }
    ],
    "schedules": [
      {
        "scheduleId": 12,
        "scheduleName": "Daily at 11 PM",
        "enabled": true,
        "frequencyType": "Daily",
        "frequencyInterval": 1,
        "activeStartDate": "2023-05-15",
        "activeEndDate": null,
        "activeStartTime": "23:00:00",
        "activeEndTime": null,
        "nextRunDate": "2024-06-10T23:00:00.000"
      }
    ],
    "executionHistory": [
      {
        "runDate": "2024-06-09T23:00:00.000",
        "runDuration": 1245,
        "runStatus": "Succeeded",
        "message": "The job succeeded. The Job was invoked by Schedule 12 (Daily at 11 PM). The last step to run was step 3 (Send Notification)."
      },
      {
        "runDate": "2024-06-08T23:00:00.000",
        "runDuration": 1156,
        "runStatus": "Succeeded",
        "message": "The job succeeded. The Job was invoked by Schedule 12 (Daily at 11 PM). The last step to run was step 3 (Send Notification)."
      },
      {
        "runDate": "2024-06-07T23:00:00.000",
        "runDuration": 1389,
        "runStatus": "Succeeded",
        "message": "The job succeeded. The Job was invoked by Schedule 12 (Daily at 11 PM). The last step to run was step 3 (Send Notification)."
      }
    ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "result": {
    "catalogExists": true,
    "catalogVersion": "16.0.1000.6",
    "folders": [
      {
        "folderId": 1,
        "folderName": "ETL_Projects",
        "description": "Production ETL processes",
        "createdBy": "domain\\etladmin",
        "createdTime": "2023-05-15T10:22:33.890",
        "projectCount": 3
      },
      {
        "folderId": 2,
        "folderName": "DataMigration",
        "description": "One-time data migration packages",
        "createdBy": "domain\\dataadmin",
        "createdTime": "2023-08-20T14:15:45.678",
        "projectCount": 1
      }
    ],
    "projects": [
      {
        "projectId": 101,
        "projectName": "CustomerDataETL",
        "folderId": 1,
        "folderName": "ETL_Projects",
        "description": "Daily customer data extraction and transformation",
        "deployedBy": "domain\\etladmin",
        "deployedTime": "2024-06-01T09:30:22.456",
        "packageCount": 5,
        "lastExecutionTime": "2024-06-10T06:00:15.123",
        "deploymentModel": "Project"
      },
      {
        "projectId": 102,
        "projectName": "SalesReporting",
        "folderId": 1,
        "folderName": "ETL_Projects",
        "description": "Weekly sales data aggregation",
        "deployedBy": "domain\\reportadmin",
        "deployedTime": "2024-05-15T11:45:18.789",
        "packageCount": 3,
        "lastExecutionTime": "2024-06-09T18:00:00.000",
        "deploymentModel": "Project"
      }
    ],
    "packages": [
      {
        "packageId": 1001,
        "packageName": "ExtractCustomers.dtsx",
        "projectId": 101,
        "projectName": "CustomerDataETL",
        "description": "Extract customer data from source systems",
        "entryPoint": true,
        "validationStatus": "Success",
        "lastExecutionTime": "2024-06-10T06:00:15.123",
        "lastExecutionResult": "Success",
        "executionDuration": 125
      },
      {
        "packageId": 1002,
        "packageName": "TransformCustomers.dtsx",
        "projectId": 101,
        "projectName": "CustomerDataETL",
        "description": "Transform and clean customer data",
        "entryPoint": false,
        "validationStatus": "Success",
        "lastExecutionTime": "2024-06-10T06:02:20.456",
        "lastExecutionResult": "Success",
        "executionDuration": 89
      }
    ],
    "environments": [
      {
        "environmentId": 10,
        "environmentName": "Production",
        "folderId": 1,
        "folderName": "ETL_Projects",
        "description": "Production environment variables",
        "createdBy": "domain\\etladmin",
        "createdTime": "2023-05-15T10:22:33.890",
        "variableCount": 15
      },
      {
        "environmentId": 11,
        "environmentName": "Development",
        "folderId": 1,
        "folderName": "ETL_Projects",
        "description": "Development environment variables",
        "createdBy": "domain\\devadmin",
        "createdTime": "2023-05-20T16:30:45.234",
        "variableCount": 12
      }
    ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "result": {
    "organization": "MyCompany",
    "projects": [
      {
        "projectId": "12345678-1234-1234-1234-123456789012",
        "projectName": "DatabaseProject",
        "description": "Main database development project",
        "state": "wellFormed",
        "visibility": "private",
        "lastUpdateTime": "2024-06-09T15:30:22.456",
        "repositoryCount": 3,
        "buildDefinitionCount": 5,
        "workItemCount": 127
      },
      {
        "projectId": "87654321-4321-4321-4321-210987654321",
        "projectName": "DataWarehouse",
        "description": "Data warehouse and BI project",
        "state": "wellFormed",
        "visibility": "private",
        "lastUpdateTime": "2024-06-08T11:45:18.789",
        "repositoryCount": 2,
        "buildDefinitionCount": 3,
        "workItemCount": 89
      }
    ],
    "repositories": [
      {
        "repositoryId": "abcd1234-5678-90ab-cdef-123456789abc",
        "repositoryName": "database-scripts",
        "projectName": "DatabaseProject",
        "defaultBranch": "main",
        "size": 15728640,
        "lastCommitDate": "2024-06-09T14:22:15.123",
        "lastCommitAuthor": "john.doe@company.com",
        "lastCommitMessage": "Update stored procedures for customer module"
      },
      {
        "repositoryId": "efgh5678-90ab-cdef-1234-567890abcdef",
        "repositoryName": "etl-packages",
        "projectName": "DataWarehouse",
        "defaultBranch": "develop",
        "size": 8945132,
        "lastCommitDate": "2024-06-08T16:55:33.789",
        "lastCommitAuthor": "jane.smith@company.com",
        "lastCommitMessage": "Add new customer dimension ETL package"
      }
    ],
    "builds": [
      {
        "buildId": 2547,
        "buildName": "Database-CI",
        "projectName": "DatabaseProject",
        "status": "completed",
        "result": "succeeded",
        "startTime": "2024-06-09T14:30:00.000",
        "finishTime": "2024-06-09T14:35:22.456",
        "requestedBy": "john.doe@company.com",
        "sourceBranch": "refs/heads/feature/customer-updates",
        "sourceVersion": "a1b2c3d4e5f6"
      },
      {
        "buildId": 2546,
        "buildName": "ETL-Deployment",
        "projectName": "DataWarehouse",
        "status": "completed",
        "result": "failed",
        "startTime": "2024-06-08T20:00:00.000",
        "finishTime": "2024-06-08T20:12:45.789",
        "requestedBy": "jane.smith@company.com",
        "sourceBranch": "refs/heads/develop",
        "sourceVersion": "f6e5d4c3b2a1"
      }
    ],
    "workItems": [
      {
        "workItemId": 15423,
        "workItemType": "Bug",
        "title": "Performance issue in customer search stored procedure",
        "state": "Active",
        "assignedTo": "john.doe@company.com",
        "createdBy": "user.tester@company.com",
        "createdDate": "2024-06-05T09:15:30.123",
        "changedDate": "2024-06-09T11:20:45.678",
        "priority": 2,
        "severity": "3 - Medium"
      },
      {
        "workItemId": 15401,
        "workItemType": "User Story",
        "title": "Implement new customer analytics dashboard",
        "state": "In Progress",
        "assignedTo": "jane.smith@company.com",
        "createdBy": "product.manager@company.com",
        "createdDate": "2024-05-28T14:30:22.456",
        "changedDate": "2024-06-07T16:45:18.234",
        "priority": 1,
        "storyPoints": 8
      }
    ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "result": {
    "success": true,
    "message": "Connection 'AdventureWorks' added successfully",
    "connectionName": "AdventureWorks",
    "encrypted": true,
    "testResult": {
      "success": true,
      "serverVersion": "Microsoft SQL Server 2022 (RTM) - 16.0.1000.6",
      "databaseName": "AdventureWorks",
      "connectionTime": 145
    },
    "createdOn": "2024-06-10T10:30:15.123Z"
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "result": {
    "success": true,
    "message": "Connection 'AdventureWorks' updated successfully",
    "connectionName": "AdventureWorks",
    "changes": {
      "connectionString": "Updated",
      "description": "Updated"
    },
    "testResult": {
      "success": true,
      "serverVersion": "Microsoft SQL Server 2022 (RTM) - 16.0.1000.6",
      "databaseName": "AdventureWorks2022",
      "connectionTime": 132
    },
    "modifiedOn": "2024-06-10T10:35:45.789Z"
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "result": {
    "success": true,
    "message": "Connection 'AdventureWorks' removed successfully",
    "connectionName": "AdventureWorks",
    "removedOn": "2024-06-10T10:40:22.456Z"
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 13,
  "result": [
    {
      "name": "DefaultConnection",
      "description": "Default SQL Server connection",
      "type": "SqlServer",
      "encrypted": true,
      "lastUsed": "2024-06-10T09:45:22.456Z",
      "createdOn": "2024-05-26T10:53:13.890Z",
      "serverInfo": {
        "serverName": "localhost",
        "databaseName": "master",
        "version": "Microsoft SQL Server 2022 (RTM) - 16.0.1000.6"
      },
      "status": "Connected"
    },
    {
      "name": "P330_SA_SSISDB",
      "description": "SSIS Database on P330",
      "type": "SqlServer",
      "encrypted": true,
      "lastUsed": "2024-05-26T14:42:03.234Z",
      "createdOn": "2024-05-26T10:53:13.890Z",
      "serverInfo": {
        "serverName": "P330.domain.local",
        "databaseName": "SSISDB",
        "version": "Microsoft SQL Server 2019 (RTM-CU18) - 15.0.4261.1"
      },
      "status": "Available"
    },
    {
      "name": "TestEnvironment",
      "description": "Testing database connection",
      "type": "SqlServer",
      "encrypted": true,
      "lastUsed": "2024-06-09T16:30:18.789Z",
      "createdOn": "2024-06-01T08:15:45.678Z",
      "serverInfo": {
        "serverName": "testserver.domain.local",
        "databaseName": "TestDB",
        "version": "Microsoft SQL Server 2022 (RTM) - 16.0.1000.6"
      },
      "status": "Available"
    }
  ]
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 14,
  "result": {
    "success": true,
    "message": "Connection test successful",
    "connectionTime": 89,
    "serverInfo": {
      "serverName": "localhost",
      "serverVersion": "Microsoft SQL Server 2022 (RTM) - 16.0.1000.6",
      "edition": "Developer Edition (64-bit)",
      "productLevel": "RTM",
      "productVersion": "16.0.1000.6",
      "databaseName": "master",
      "collation": "SQL_Latin1_General_CP1_CI_AS",
      "isClusteredInstance": false,
      "isCaseSensitive": false
    },
    "authentication": {
      "authenticationType": "Windows Authentication",
      "loginName": "DOMAIN\\currentuser",
      "isAdmin": true
    },
    "capabilities": {
      "supportsSSIS": true,
      "supportsAgent": true,
      "supportsFullTextSearch": true,
      "supportsAlwaysOn": false,
      "supportsColumnStore": true
    },
    "testedOn": "2024-06-10T10:45:33.123Z"
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 15,
  "result": {
    "key": "A1B2C3D4E5F6789012345678901234567890ABCDEF1234567890ABCDEF123456",
    "length": 32,
    "algorithm": "AES-256",
    "encoding": "Base64",
    "createdOn": "2024-06-10T10:50:15.123Z",
    "expiryRecommendation": "2025-06-10T10:50:15.123Z",
    "usage": "For encrypting connection strings in mssqlMCP"
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 16,
  "result": {
    "success": true,
    "message": "Migration to encrypted format completed successfully",
    "summary": {
      "totalConnections": 3,
      "migratedConnections": 2,
      "alreadyEncrypted": 1,
      "failedMigrations": 0
    },
    "details": [
      {
        "connectionName": "DefaultConnection",
        "status": "Already Encrypted",
        "message": "Connection was already in encrypted format"
      },
      {
        "connectionName": "P330_SA_SSISDB",
        "status": "Migrated",
        "message": "Connection string encrypted successfully"
      },
      {
        "connectionName": "TestEnvironment",
        "status": "Migrated",
        "message": "Connection string encrypted successfully"
      }
    ],
    "encryptionInfo": {
      "algorithm": "AES-256",
      "keyLength": 32,
      "migrationTime": "2024-06-10T10:55:22.789Z"
    },
    "recommendations": [
      "Verify all connections are working properly after migration",
      "Update backup procedures to include encryption keys",
      "Consider implementing key rotation schedule"
    ]
  }
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

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 17,
  "result": {
    "success": true,
    "message": "Encryption key rotated successfully",
    "summary": {
      "totalConnections": 3,
      "reencryptedConnections": 3,
      "failedReencryptions": 0,
      "connectionTestsPassed": 3
    },
    "rotationDetails": {
      "oldKeyHash": "sha256:a1b2c3d4e5f6...",
      "newKeyHash": "sha256:f6e5d4c3b2a1...",
      "rotationTime": "2024-06-10T11:00:45.456Z",
      "algorithm": "AES-256"
    },
    "connectionResults": [
      {
        "connectionName": "DefaultConnection",
        "reencrypted": true,
        "tested": true,
        "testResult": "Success",
        "message": "Connection re-encrypted and tested successfully"
      },
      {
        "connectionName": "P330_SA_SSISDB",
        "reencrypted": true,
        "tested": true,
        "testResult": "Success",
        "message": "Connection re-encrypted and tested successfully"
      },
      {
        "connectionName": "TestEnvironment",
        "reencrypted": true,
        "tested": true,
        "testResult": "Success",
        "message": "Connection re-encrypted and tested successfully"
      }
    ],
    "securityRecommendations": [
      "Store the new encryption key securely",
      "Update backup procedures with new key information",
      "Document the key rotation in security logs",
      "Schedule next key rotation"
    ]
  }
}
```

## API Key Management Tools

### 18. CreateApiKey

**Method**: `CreateApiKey` _(Requires Master Key)_  
**Description**: Create a new managed API key with specific endpoint permissions

**Parameters**:

- `name` (string, required): Human-readable name for the API key
- `description` (string, optional): Description of the API key's purpose
- `allowedEndpoints` (array, required): List of endpoints this key can access
- `createdBy` (string, optional): Who created this API key
- `expiresOn` (datetime, optional): Expiration date for the API key

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 18,
  "method": "CreateApiKey",
  "params": {
    "name": "ReadOnlyKey",
    "description": "API key for read-only database operations",
    "allowedEndpoints": ["GetTables", "GetTableInfo", "QueryDatabase"],
    "createdBy": "admin@company.com",
    "expiresOn": "2025-12-31T23:59:59Z"
  }
}
```

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 18,
  "result": {
    "success": true,
    "message": "API key created successfully",
    "apiKey": "mcp_ABC123DEF456GHI789JKL012MNO345PQ",
    "keyInfo": {
      "id": "12345678-1234-1234-1234-123456789012",
      "name": "ReadOnlyKey",
      "description": "API key for read-only database operations",
      "allowedEndpoints": ["GetTables", "GetTableInfo", "QueryDatabase"],
      "createdOn": "2024-06-10T12:00:00.000Z",
      "expiresOn": "2025-12-31T23:59:59.000Z"
    }
  }
}
```

### 19. ListApiKeys

**Method**: `ListApiKeys` _(Requires Master Key)_  
**Description**: List all managed API keys with their metadata (keys themselves are hidden)

**Parameters**: None

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 19,
  "method": "ListApiKeys",
  "params": {}
}
```

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 19,
  "result": {
    "success": true,
    "count": 2,
    "apiKeys": [
      {
        "id": "12345678-1234-1234-1234-123456789012",
        "name": "ReadOnlyKey",
        "description": "API key for read-only database operations",
        "allowedEndpoints": ["GetTables", "GetTableInfo", "QueryDatabase"],
        "isActive": true,
        "createdOn": "2024-06-10T12:00:00.000Z",
        "modifiedOn": "2024-06-10T12:00:00.000Z",
        "lastUsed": "2024-06-10T14:30:00.000Z",
        "createdBy": "admin@company.com",
        "expiresOn": "2025-12-31T23:59:59.000Z",
        "usageCount": 157
      },
      {
        "id": "87654321-4321-4321-4321-210987654321",
        "name": "ReportingKey",
        "description": "API key for automated reporting",
        "allowedEndpoints": ["QueryDatabase", "GetTableInfo"],
        "isActive": true,
        "createdOn": "2024-06-08T09:15:00.000Z",
        "modifiedOn": "2024-06-08T09:15:00.000Z",
        "lastUsed": "2024-06-10T13:45:00.000Z",
        "createdBy": "reporting-service",
        "expiresOn": null,
        "usageCount": 1234
      }
    ]
  }
}
```

### 20. UpdateApiKey

**Method**: `UpdateApiKey` _(Requires Master Key)_  
**Description**: Update an existing managed API key's properties

**Parameters**:

- `id` (string, required): ID of the API key to update
- `name` (string, optional): New name for the API key
- `description` (string, optional): New description
- `allowedEndpoints` (array, optional): New list of allowed endpoints
- `isActive` (boolean, optional): Whether the key is active
- `expiresOn` (datetime, optional): New expiration date

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "UpdateApiKey",
  "params": {
    "id": "12345678-1234-1234-1234-123456789012",
    "name": "ReadOnlyKeyUpdated",
    "isActive": false,
    "expiresOn": "2024-12-31T23:59:59Z"
  }
}
```

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "result": {
    "success": true,
    "message": "API key updated successfully"
  }
}
```

### 21. RemoveApiKey

**Method**: `RemoveApiKey` _(Requires Master Key)_  
**Description**: Remove a managed API key permanently

**Parameters**:

- `id` (string, required): ID of the API key to remove

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 21,
  "method": "RemoveApiKey",
  "params": {
    "id": "12345678-1234-1234-1234-123456789012"
  }
}
```

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 21,
  "result": {
    "success": true,
    "message": "API key removed successfully"
  }
}
```

### 22. GetApiKeyInfo

**Method**: `GetApiKeyInfo` _(Requires Master Key)_  
**Description**: Get detailed information about a specific API key by name

**Parameters**:

- `name` (string, required): Name of the API key to retrieve

**Example**:

```json
{
  "jsonrpc": "2.0",
  "id": 22,
  "method": "GetApiKeyInfo",
  "params": {
    "name": "ReadOnlyKey"
  }
}
```

**Response**:

```json
{
  "jsonrpc": "2.0",
  "id": 22,
  "result": {
    "success": true,
    "keyInfo": {
      "id": "12345678-1234-1234-1234-123456789012",
      "name": "ReadOnlyKey",
      "description": "API key for read-only database operations",
      "allowedEndpoints": ["GetTables", "GetTableInfo", "QueryDatabase"],
      "isActive": true,
      "createdOn": "2024-06-10T12:00:00.000Z",
      "modifiedOn": "2024-06-10T12:00:00.000Z",
      "lastUsed": "2024-06-10T14:30:00.000Z",
      "createdBy": "admin@company.com",
      "expiresOn": "2025-12-31T23:59:59.000Z",
      "usageCount": 157
    }
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

### Error Response Examples

**Authentication Error (401)**:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "error": {
    "code": -32001,
    "message": "Authentication required",
    "data": {
      "details": "Please provide either a Bearer token in the Authorization header or an API key in the X-API-Key header",
      "supportedMethods": ["Bearer", "X-API-Key"]
    }
  }
}
```

**Invalid Credentials (403)**:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "error": {
    "code": -32002,
    "message": "Invalid authentication credentials",
    "data": {
      "details": "The provided authentication credentials are not valid"
    }
  }
}
```

**Validation Error**:

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "error": {
    "code": -32003,
    "message": "Invalid input parameters",
    "data": {
      "details": "Query parameter is required and cannot be empty",
      "parameter": "query",
      "validationErrors": [
        "Query cannot be null or empty",
        "Query must be a valid SQL statement"
      ]
    }
  }
}
```

**Database Connection Error**:

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "error": {
    "code": -32004,
    "message": "Database connection failed",
    "data": {
      "details": "Unable to connect to SQL Server instance",
      "serverName": "localhost",
      "databaseName": "master",
      "errorCode": 2,
      "sqlState": "08001",
      "suggestions": [
        "Verify server name and port",
        "Check network connectivity",
        "Ensure SQL Server service is running"
      ]
    }
  }
}
```

**SQL Execution Error**:

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "error": {
    "code": -32005,
    "message": "SQL query execution failed",
    "data": {
      "details": "Invalid object name 'NonExistentTable'",
      "sqlErrorNumber": 208,
      "severity": 16,
      "state": 1,
      "lineNumber": 1,
      "query": "SELECT * FROM NonExistentTable"
    }
  }
}
```

**Timeout Error**:

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "error": {
    "code": -32006,
    "message": "Operation timeout",
    "data": {
      "details": "The SQL query execution timed out after 90 seconds",
      "timeoutDuration": 90,
      "operation": "ExecuteQuery",
      "suggestions": [
        "Optimize the query for better performance",
        "Increase the timeout duration if needed",
        "Check for blocking processes"
      ]
    }
  }
}
```

**Connection Not Found Error**:

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "error": {
    "code": -32007,
    "message": "Connection not found",
    "data": {
      "details": "No connection found with name 'NonExistentConnection'",
      "requestedConnection": "NonExistentConnection",
      "availableConnections": [
        "DefaultConnection",
        "P330_SA_SSISDB",
        "TestEnvironment"
      ]
    }
  }
}
```

**Method Not Found Error**:

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "error": {
    "code": -32601,
    "message": "Method not found",
    "data": {
      "details": "The requested method 'InvalidMethod' does not exist",
      "requestedMethod": "InvalidMethod",
      "availableMethods": [
        "Initialize",
        "ExecuteQuery",
        "GetTableMetadata",
        "GetDatabaseObjectsMetadata",
        "GetDatabaseObjectsByType",
        "GetSqlServerAgentJobs",
        "GetSqlServerAgentJobDetails",
        "GetSsisCatalogInfo",
        "GetAzureDevOpsInfo",
        "ListConnections",
        "AddConnection",
        "UpdateConnection",
        "RemoveConnection",
        "TestConnection",
        "GenerateSecureKey",
        "MigrateConnectionsToEncrypted",
        "RotateKey"
      ]
    }
  }
}
```

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

### C# Example

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class McpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public McpApiClient(string baseUrl, string apiKey)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
        _apiKey = apiKey;

        // Set authentication header (choose one)
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        // Or use X-API-Key header:
        // _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }

    public async Task<T> CallMcpApiAsync<T>(string method, object parameters = null)
    {
        var request = new
        {
            jsonrpc = "2.0",
            id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            method = method,
            @params = parameters ?? new { }
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_baseUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(responseJson);

        if (jsonResponse.RootElement.TryGetProperty("error", out var error))
        {
            var errorMessage = error.GetProperty("message").GetString();
            var errorCode = error.GetProperty("code").GetInt32();
            throw new Exception($"MCP Error {errorCode}: {errorMessage}");
        }

        var result = jsonResponse.RootElement.GetProperty("result");
        return JsonSerializer.Deserialize<T>(result.GetRawText(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Usage examples
public class Program
{
    public static async Task Main(string[] args)
    {
        var client = new McpApiClient("http://localhost:3001/", "your-api-key-here");

        try
        {
            // 1. Initialize connection
            await client.CallMcpApiAsync<object>("Initialize", new
            {
                connectionName = "DefaultConnection"
            });
            Console.WriteLine("Connection initialized successfully");

            // 2. List all connections
            var connections = await client.CallMcpApiAsync<dynamic>("ListConnections");
            Console.WriteLine($"Found {connections} connections");

            // 3. Execute a simple query
            var queryResult = await client.CallMcpApiAsync<dynamic>("ExecuteQuery", new
            {
                query = "SELECT name, database_id FROM sys.databases WHERE database_id > 4",
                connectionName = "DefaultConnection"
            });
            Console.WriteLine($"Query result: {queryResult}");

            // 4. Get table metadata
            var tableMetadata = await client.CallMcpApiAsync<dynamic>("GetTableMetadata", new
            {
                connectionName = "DefaultConnection",
                schema = "dbo"
            });
            Console.WriteLine($"Table metadata retrieved");

            // 5. Get database objects by type
            var tables = await client.CallMcpApiAsync<dynamic>("GetDatabaseObjectsByType", new
            {
                connectionName = "DefaultConnection",
                objectType = "TABLE",
                schema = "dbo"
            });
            Console.WriteLine($"Tables found");

            // 6. Add a new connection
            await client.CallMcpApiAsync<object>("AddConnection", new
            {
                request = new
                {
                    name = "TestConnection",
                    connectionString = "Server=localhost;Database=TestDB;Trusted_Connection=True;",
                    description = "Test database connection"
                }
            });
            Console.WriteLine("Connection added successfully");

            // 7. Test the new connection
            var testResult = await client.CallMcpApiAsync<dynamic>("TestConnection", new
            {
                request = new
                {
                    connectionString = "Server=localhost;Database=TestDB;Trusted_Connection=True;"
                }
            });
            Console.WriteLine($"Connection test result: {testResult}");

            // 8. Get SQL Server Agent jobs
            var agentJobs = await client.CallMcpApiAsync<dynamic>("GetSqlServerAgentJobs", new
            {
                connectionName = "DefaultConnection"
            });
            Console.WriteLine("SQL Server Agent jobs retrieved");

            // 9. Get SSIS catalog information
            var ssisCatalog = await client.CallMcpApiAsync<dynamic>("GetSsisCatalogInfo", new
            {
                connectionName = "DefaultConnection"
            });
            Console.WriteLine("SSIS catalog information retrieved");

            // 10. Generate a secure key
            var secureKey = await client.CallMcpApiAsync<dynamic>("GenerateSecureKey", new
            {
                length = 32
            });
            Console.WriteLine($"Generated secure key");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}
```

### Advanced C# Example with Strongly Typed Models

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Response models
public class McpResponse<T>
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("result")]
    public T Result { get; set; }

    [JsonPropertyName("error")]
    public McpError Error { get; set; }
}

public class McpError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public object Data { get; set; }
}

public class ConnectionInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("lastUsed")]
    public DateTime? LastUsed { get; set; }

    [JsonPropertyName("createdOn")]
    public DateTime CreatedOn { get; set; }
}

public class TableInfo
{
    [JsonPropertyName("tableName")]
    public string TableName { get; set; }

    [JsonPropertyName("schemaName")]
    public string SchemaName { get; set; }

    [JsonPropertyName("columns")]
    public List<ColumnInfo> Columns { get; set; }

    [JsonPropertyName("primaryKeys")]
    public List<string> PrimaryKeys { get; set; }

    [JsonPropertyName("foreignKeys")]
    public List<ForeignKeyInfo> ForeignKeys { get; set; }
}

public class ColumnInfo
{
    [JsonPropertyName("columnName")]
    public string ColumnName { get; set; }

    [JsonPropertyName("dataType")]
    public string DataType { get; set; }

    [JsonPropertyName("isNullable")]
    public bool IsNullable { get; set; }

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("precision")]
    public int? Precision { get; set; }

    [JsonPropertyName("scale")]
    public int? Scale { get; set; }
}

public class ForeignKeyInfo
{
    [JsonPropertyName("constraintName")]
    public string ConstraintName { get; set; }

    [JsonPropertyName("columnName")]
    public string ColumnName { get; set; }

    [JsonPropertyName("referencedTable")]
    public string ReferencedTable { get; set; }

    [JsonPropertyName("referencedColumn")]
    public string ReferencedColumn { get; set; }
}

// Strongly typed MCP client
public class TypedMcpApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public TypedMcpApiClient(string baseUrl, string apiKey)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task<T> CallApiAsync<T>(string method, object parameters = null)
    {
        var request = new
        {
            jsonrpc = "2.0",
            id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            method = method,
            @params = parameters ?? new { }
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_baseUrl, content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HTTP {response.StatusCode}: {responseJson}");
        }

        var mcpResponse = JsonSerializer.Deserialize<McpResponse<T>>(responseJson, _jsonOptions);

        if (mcpResponse.Error != null)
        {
            throw new Exception($"MCP Error {mcpResponse.Error.Code}: {mcpResponse.Error.Message}");
        }

        return mcpResponse.Result;
    }

    // Strongly typed methods
    public async Task<List<ConnectionInfo>> ListConnectionsAsync()
    {
        return await CallApiAsync<List<ConnectionInfo>>("ListConnections");
    }

    public async Task<List<TableInfo>> GetTableMetadataAsync(string connectionName = "DefaultConnection", string schema = null)
    {
        return await CallApiAsync<List<TableInfo>>("GetTableMetadata", new
        {
            connectionName,
            schema
        });
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, string connectionName = "DefaultConnection")
    {
        return await CallApiAsync<List<Dictionary<string, object>>>("ExecuteQuery", new
        {
            query,
            connectionName
        });
    }

    public async Task<bool> AddConnectionAsync(string name, string connectionString, string description = null)
    {
        await CallApiAsync<object>("AddConnection", new
        {
            request = new
            {
                name,
                connectionString,
                description
            }
        });
        return true;
    }

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        var result = await CallApiAsync<Dictionary<string, object>>("TestConnection", new
        {
            request = new { connectionString }
        });
        return result.ContainsKey("success") && (bool)result["success"];
    }

    public async Task InitializeAsync(string connectionName = "DefaultConnection")
    {
        await CallApiAsync<object>("Initialize", new { connectionName });
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Example usage with strongly typed client
public class AdvancedExample
{
    public static async Task RunAdvancedExamplesAsync()
    {
        var client = new TypedMcpApiClient("http://localhost:3001/", "your-api-key-here");

        try
        {
            // Initialize connection
            await client.InitializeAsync();
            Console.WriteLine("✓ Connection initialized");

            // List all connections
            var connections = await client.ListConnectionsAsync();
            Console.WriteLine($"✓ Found {connections.Count} connections:");
            foreach (var conn in connections)
            {
                Console.WriteLine($"  - {conn.Name}: {conn.Description} (Type: {conn.Type})");
            }

            // Get table metadata with strong typing
            var tables = await client.GetTableMetadataAsync("DefaultConnection", "dbo");
            Console.WriteLine($"✓ Retrieved metadata for {tables.Count} tables:");

            foreach (var table in tables)
            {
                Console.WriteLine($"  Table: {table.SchemaName}.{table.TableName}");
                Console.WriteLine($"    Columns: {table.Columns.Count}");
                Console.WriteLine($"    Primary Keys: {string.Join(", ", table.PrimaryKeys)}");
                Console.WriteLine($"    Foreign Keys: {table.ForeignKeys.Count}");
            }

            // Execute queries with strong typing
            var queryResults = await client.ExecuteQueryAsync(
                "SELECT TOP 5 name, create_date FROM sys.databases ORDER BY create_date DESC"
            );

            Console.WriteLine($"✓ Query returned {queryResults.Count} rows:");
            foreach (var row in queryResults)
            {
                Console.WriteLine($"  Database: {row["name"]}, Created: {row["create_date"]}");
            }

            // Test connection
            var isValid = await client.TestConnectionAsync(
                "Server=localhost;Database=master;Trusted_Connection=True;"
            );
            Console.WriteLine($"✓ Connection test: {(isValid ? "PASSED" : "FAILED")}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();        }
    }
}
```

### Python Example

```python
import asyncio
import aiohttp
import json
import time
from typing import Optional, Dict, Any, List
from dataclasses import dataclass
from datetime import datetime

class McpApiClient:
    """Basic Python client for MCP API using aiohttp"""

    def __init__(self, base_url: str, api_key: str):
        self.base_url = base_url
        self.api_key = api_key
        self.session = None

    async def __aenter__(self):
        """Async context manager entry"""
        self.session = aiohttp.ClientSession()
        return self

    async def __aexit__(self, exc_type, exc_val, exc_tb):
        """Async context manager exit"""
        if self.session:
            await self.session.close()

    async def call_mcp_api(self, method: str, parameters: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        """
        Call MCP API endpoint with specified method and parameters

        Args:
            method: MCP method name
            parameters: Method parameters (optional)

        Returns:
            API response result

        Raises:
            Exception: If API call fails or returns error
        """
        if not self.session:
            raise RuntimeError("Client not initialized. Use 'async with' statement.")

        request_data = {
            "jsonrpc": "2.0",
            "id": int(time.time() * 1000),
            "method": method,
            "params": parameters or {}
        }

        headers = {
            "Authorization": f"Bearer {self.api_key}",
            # Alternative: "X-API-Key": self.api_key,
            "Content-Type": "application/json"
        }

        try:
            async with self.session.post(
                self.base_url,
                json=request_data,
                headers=headers
            ) as response:

                if response.status != 200:
                    error_text = await response.text()
                    raise Exception(f"HTTP {response.status}: {error_text}")

                result = await response.json()

                if "error" in result:
                    error = result["error"]
                    raise Exception(f"MCP Error {error['code']}: {error['message']}")

                return result.get("result", {})

        except aiohttp.ClientError as e:
            raise Exception(f"Network error: {str(e)}")

# Usage examples
async def main():
    """Demonstrate basic MCP API usage"""

    async with McpApiClient("http://localhost:3001/", "your-api-key-here") as client:
        try:
            # 1. Initialize connection
            await client.call_mcp_api("Initialize", {
                "connectionName": "DefaultConnection"
            })
            print("✓ Connection initialized successfully")

            # 2. List all connections
            connections = await client.call_mcp_api("ListConnections")
            print(f"✓ Found {len(connections)} connections:")
            for conn in connections:
                print(f"  - {conn['name']}: {conn.get('description', 'No description')}")

            # 3. Execute a simple query
            query_result = await client.call_mcp_api("ExecuteQuery", {
                "query": "SELECT name, database_id FROM sys.databases WHERE database_id > 4",
                "connectionName": "DefaultConnection"
            })
            print(f"✓ Query returned {len(query_result)} rows:")
            for row in query_result[:3]:  # Show first 3 rows
                print(f"  Database: {row['name']}, ID: {row['database_id']}")

            # 4. Get table metadata
            table_metadata = await client.call_mcp_api("GetTableMetadata", {
                "connectionName": "DefaultConnection",
                "schema": "dbo"
            })
            print(f"✓ Retrieved metadata for {len(table_metadata)} tables")

            # 5. Get database objects by type
            tables = await client.call_mcp_api("GetDatabaseObjectsByType", {
                "connectionName": "DefaultConnection",
                "objectType": "TABLE",
                "schema": "dbo"
            })
            print(f"✓ Found {len(tables)} tables in dbo schema")

            # 6. Test a connection
            test_result = await client.call_mcp_api("TestConnection", {
                "request": {
                    "connectionString": "Server=localhost;Database=master;Trusted_Connection=True;"
                }
            })
            print(f"✓ Connection test: {'PASSED' if test_result.get('success') else 'FAILED'}")

            # 7. Generate a secure key
            secure_key = await client.call_mcp_api("GenerateSecureKey", {
                "length": 32
            })
            print(f"✓ Generated secure key: {secure_key['key'][:8]}...")

            # 8. Get SQL Server Agent jobs
            agent_jobs = await client.call_mcp_api("GetSqlServerAgentJobs", {
                "connectionName": "DefaultConnection"
            })
            print(f"✓ Found {len(agent_jobs)} SQL Server Agent jobs")

            # 9. Get SSIS catalog information
            ssis_catalog = await client.call_mcp_api("GetSsisCatalogInfo", {
                "connectionName": "DefaultConnection"
            })
            print(f"✓ SSIS catalog info retrieved")

        except Exception as e:
            print(f"❌ Error: {e}")

# Run the example
if __name__ == "__main__":
    asyncio.run(main())
```

### Advanced Python Example with Type Hints and Data Classes

```python
import asyncio
import aiohttp
import json
import time
from typing import Optional, Dict, Any, List, Union
from dataclasses import dataclass, field
from datetime import datetime
from enum import Enum

# Type definitions and data classes
class ObjectType(Enum):
    """Database object types"""
    TABLE = "TABLE"
    VIEW = "VIEW"
    PROCEDURE = "PROCEDURE"
    FUNCTION = "FUNCTION"
    ALL = "ALL"

@dataclass
class ConnectionInfo:
    """Connection information model"""
    name: str
    description: Optional[str] = None
    type: str = "SqlServer"
    last_used: Optional[datetime] = None
    created_on: Optional[datetime] = None

@dataclass
class ColumnInfo:
    """Column metadata model"""
    column_name: str
    data_type: str
    is_nullable: bool
    max_length: Optional[int] = None
    precision: Optional[int] = None
    scale: Optional[int] = None

@dataclass
class ForeignKeyInfo:
    """Foreign key constraint model"""
    constraint_name: str
    column_name: str
    referenced_table: str
    referenced_column: str

@dataclass
class TableInfo:
    """Table metadata model"""
    table_name: str
    schema_name: str
    columns: List[ColumnInfo] = field(default_factory=list)
    primary_keys: List[str] = field(default_factory=list)
    foreign_keys: List[ForeignKeyInfo] = field(default_factory=list)

@dataclass
class McpError:
    """MCP error response model"""
    code: int
    message: str
    data: Optional[Dict[str, Any]] = None

@dataclass
class McpResponse:
    """MCP response wrapper"""
    jsonrpc: str
    id: int
    result: Optional[Any] = None
    error: Optional[McpError] = None

class TypedMcpApiClient:
    """Advanced Python client with type hints and data models"""

    def __init__(self, base_url: str, api_key: str, use_x_api_key: bool = False):
        self.base_url = base_url
        self.api_key = api_key
        self.use_x_api_key = use_x_api_key
        self.session: Optional[aiohttp.ClientSession] = None

    async def __aenter__(self):
        """Initialize aiohttp session"""
        connector = aiohttp.TCPConnector(limit=100, limit_per_host=30)
        timeout = aiohttp.ClientTimeout(total=30)
        self.session = aiohttp.ClientSession(
            connector=connector,
            timeout=timeout
        )
        return self

    async def __aexit__(self, exc_type, exc_val, exc_tb):
        """Clean up aiohttp session"""
        if self.session:
            await self.session.close()

    def _get_headers(self) -> Dict[str, str]:
        """Get request headers with authentication"""
        headers = {"Content-Type": "application/json"}

        if self.use_x_api_key:
            headers["X-API-Key"] = self.api_key
        else:
            headers["Authorization"] = f"Bearer {self.api_key}"

        return headers

    async def _call_api(self, method: str, parameters: Optional[Dict[str, Any]] = None) -> Any:
        """Internal API call method with error handling"""
        if not self.session:
            raise RuntimeError("Client not initialized. Use 'async with' statement.")

        request_data = {
            "jsonrpc": "2.0",
            "id": int(time.time() * 1000),
            "method": method,
            "params": parameters or {}
        }

        try:
            async with self.session.post(
                self.base_url,
                json=request_data,
                headers=self._get_headers()
            ) as response:

                response_text = await response.text()

                if response.status != 200:
                    raise Exception(f"HTTP {response.status}: {response_text}")

                try:
                    result = json.loads(response_text)
                except json.JSONDecodeError as e:
                    raise Exception(f"Invalid JSON response: {e}")

                if "error" in result and result["error"]:
                    error = result["error"]
                    raise Exception(f"MCP Error {error['code']}: {error['message']}")

                return result.get("result")

        except aiohttp.ClientError as e:
            raise Exception(f"Network error: {str(e)}")

    # Typed API methods
    async def initialize(self, connection_name: str = "DefaultConnection") -> bool:
        """Initialize a database connection"""
        await self._call_api("Initialize", {"connectionName": connection_name})
        return True

    async def list_connections(self) -> List[ConnectionInfo]:
        """Get list of all database connections"""
        result = await self._call_api("ListConnections")
        connections = []

        for conn_data in result:
            connections.append(ConnectionInfo(
                name=conn_data["name"],
                description=conn_data.get("description"),
                type=conn_data.get("type", "SqlServer"),
                last_used=datetime.fromisoformat(conn_data["lastUsed"].replace("Z", "+00:00")) if conn_data.get("lastUsed") else None,
                created_on=datetime.fromisoformat(conn_data["createdOn"].replace("Z", "+00:00")) if conn_data.get("createdOn") else None
            ))

        return connections

    async def execute_query(self, query: str, connection_name: str = "DefaultConnection") -> List[Dict[str, Any]]:
        """Execute SQL query and return results"""
        return await self._call_api("ExecuteQuery", {
            "query": query,
            "connectionName": connection_name
        })

    async def get_table_metadata(self, connection_name: str = "DefaultConnection", schema: Optional[str] = None) -> List[TableInfo]:
        """Get detailed table metadata"""
        result = await self._call_api("GetTableMetadata", {
            "connectionName": connection_name,
            "schema": schema
        })

        tables = []
        for table_data in result:
            # Parse columns
            columns = [
                ColumnInfo(
                    column_name=col["columnName"],
                    data_type=col["dataType"],
                    is_nullable=col["isNullable"],
                    max_length=col.get("maxLength"),
                    precision=col.get("precision"),
                    scale=col.get("scale")
                )
                for col in table_data.get("columns", [])
            ]

            # Parse foreign keys
            foreign_keys = [
                ForeignKeyInfo(
                    constraint_name=fk["constraintName"],
                    column_name=fk["columnName"],
                    referenced_table=fk["referencedTable"],
                    referenced_column=fk["referencedColumn"]
                )
                for fk in table_data.get("foreignKeys", [])
            ]

            tables.append(TableInfo(
                table_name=table_data["tableName"],
                schema_name=table_data["schemaName"],
                columns=columns,
                primary_keys=table_data.get("primaryKeys", []),
                foreign_keys=foreign_keys
            ))

        return tables

    async def get_database_objects_by_type(
        self,
        object_type: ObjectType = ObjectType.ALL,
        connection_name: str = "DefaultConnection",
        schema: Optional[str] = None
    ) -> List[Dict[str, Any]]:
        """Get database objects filtered by type"""
        return await self._call_api("GetDatabaseObjectsByType", {
            "connectionName": connection_name,
            "objectType": object_type.value,
            "schema": schema
        })

    async def add_connection(self, name: str, connection_string: str, description: Optional[str] = None) -> bool:
        """Add a new database connection"""
        await self._call_api("AddConnection", {
            "request": {
                "name": name,
                "connectionString": connection_string,
                "description": description
            }
        })
        return True

    async def test_connection(self, connection_string: str) -> bool:
        """Test database connection"""
        result = await self._call_api("TestConnection", {
            "request": {"connectionString": connection_string}
        })
        return result.get("success", False)

    async def get_sql_server_agent_jobs(self, connection_name: str = "DefaultConnection") -> List[Dict[str, Any]]:
        """Get SQL Server Agent jobs"""
        return await self._call_api("GetSqlServerAgentJobs", {
            "connectionName": connection_name
        })

    async def get_sql_server_agent_job_details(self, job_name: str, connection_name: str = "DefaultConnection") -> Dict[str, Any]:
        """Get detailed information about a specific SQL Server Agent job"""
        return await self._call_api("GetSqlServerAgentJobDetails", {
            "jobName": job_name,
            "connectionName": connection_name
        })

    async def get_ssis_catalog_info(self, connection_name: str = "DefaultConnection") -> Dict[str, Any]:
        """Get SSIS catalog information"""
        return await self._call_api("GetSsisCatalogInfo", {
            "connectionName": connection_name
        })

    async def get_azure_devops_info(self, connection_name: str = "DefaultConnection") -> Dict[str, Any]:
        """Get Azure DevOps information"""
        return await self._call_api("GetAzureDevOpsInfo", {
            "connectionName": connection_name
        })

    async def generate_secure_key(self, length: int = 32) -> str:
        """Generate a secure encryption key"""
        result = await self._call_api("GenerateSecureKey", {"length": length})
        return result["key"]

# Utility functions
def format_table_info(table: TableInfo) -> str:
    """Format table information for display"""
    lines = [
        f"Table: {table.schema_name}.{table.table_name}",
        f"  Columns ({len(table.columns)}):"
    ]

    for col in table.columns[:5]:  # Show first 5 columns
        nullable = "NULL" if col.is_nullable else "NOT NULL"
        lines.append(f"    - {col.column_name}: {col.data_type} {nullable}")

    if len(table.columns) > 5:
        lines.append(f"    ... and {len(table.columns) - 5} more columns")

    if table.primary_keys:
        lines.append(f"  Primary Keys: {', '.join(table.primary_keys)}")

    if table.foreign_keys:
        lines.append(f"  Foreign Keys ({len(table.foreign_keys)}):")
        for fk in table.foreign_keys[:3]:  # Show first 3 FK
            lines.append(f"    - {fk.column_name} -> {fk.referenced_table}.{fk.referenced_column}")

    return "\n".join(lines)

# Example usage with advanced features
async def advanced_example():
    """Demonstrate advanced MCP API usage with typed client"""

    # Use X-API-Key authentication
    async with TypedMcpApiClient("http://localhost:3001/", "your-api-key-here", use_x_api_key=True) as client:
        try:
            print("🚀 Starting advanced MCP API demonstration...")

            # Initialize connection
            await client.initialize()
            print("✓ Connection initialized")

            # List connections with type safety
            connections = await client.list_connections()
            print(f"✓ Found {len(connections)} connections:")
            for conn in connections:
                last_used = conn.last_used.strftime("%Y-%m-%d %H:%M") if conn.last_used else "Never"
                print(f"  - {conn.name}: {conn.description or 'No description'} (Last used: {last_used})")

            # Get table metadata with full type safety
            tables = await client.get_table_metadata(schema="dbo")
            print(f"\n✓ Retrieved metadata for {len(tables)} tables:")

            for table in tables[:3]:  # Show first 3 tables
                print(format_table_info(table))
                print()

            # Execute complex queries
            complex_queries = [
                "SELECT COUNT(*) as table_count FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                "SELECT name, create_date FROM sys.databases WHERE database_id > 4 ORDER BY create_date DESC",
                "SELECT name, type_desc FROM sys.objects WHERE type IN ('U', 'V') ORDER BY name"
            ]

            for i, query in enumerate(complex_queries, 1):
                results = await client.execute_query(query)
                print(f"✓ Query {i} returned {len(results)} rows")
                if results:
                    print(f"  Sample result: {results[0]}")

            # Get database objects by type
            for obj_type in [ObjectType.TABLE, ObjectType.VIEW, ObjectType.PROCEDURE]:
                objects = await client.get_database_objects_by_type(obj_type, schema="dbo")
                print(f"✓ Found {len(objects)} {obj_type.value.lower()}s in dbo schema")

            # Test connection
            test_conn_str = "Server=localhost;Database=master;Trusted_Connection=True;"
            is_valid = await client.test_connection(test_conn_str)
            print(f"✓ Connection test: {'PASSED' if is_valid else 'FAILED'}")

            # Generate secure key
            secure_key = await client.generate_secure_key(32)
            print(f"✓ Generated secure key: {secure_key[:16]}...")

            # Get SQL Server Agent jobs
            try:
                agent_jobs = await client.get_sql_server_agent_jobs()
                print(f"✓ Found {len(agent_jobs)} SQL Server Agent jobs")

                if agent_jobs:
                    # Get details for the first job
                    job_name = agent_jobs[0].get("jobName")
                    if job_name:
                        job_details = await client.get_sql_server_agent_job_details(job_name)
                        print(f"✓ Retrieved details for job: {job_name}")
            except Exception as e:
                print(f"⚠️  SQL Server Agent not available: {e}")

            # Get SSIS catalog info
            try:
                ssis_info = await client.get_ssis_catalog_info()
                print("✓ SSIS catalog information retrieved")
            except Exception as e:
                print(f"⚠️  SSIS catalog not available: {e}")

        except Exception as e:
            print(f"❌ Error: {e}")

# Synchronous wrapper for compatibility
def run_example():
    """Run the async example in a synchronous context"""
    asyncio.run(advanced_example())

if __name__ == "__main__":
    # Run basic example
    print("=== Basic Example ===")
    asyncio.run(main())

    print("\n=== Advanced Example ===")
    run_example()
```

### Python Dependencies

To use these examples, install the required dependencies:

```bash
# Using pip
pip install aiohttp

# Using pip with requirements.txt
# Create requirements.txt with:
# aiohttp>=3.8.0
# asyncio (built-in with Python 3.7+)

pip install -r requirements.txt

# Using conda
conda install aiohttp

# Using poetry
poetry add aiohttp
```

### Python Example Features

The Python examples demonstrate:

1. **Basic HTTP Client**: Simple async client using aiohttp
2. **Type Safety**: Full type hints and data classes for all models
3. **Error Handling**: Comprehensive error handling for network and API errors
4. **Authentication**: Support for both Bearer token and X-API-Key methods
5. **Async/Await**: Modern Python async patterns
6. **Context Managers**: Proper resource management with async context managers
7. **Data Models**: Strongly typed models using dataclasses
8. **Enum Support**: Type-safe object type filtering
9. **Utility Functions**: Helper functions for formatting and display
10. **Real-world Usage**: Complete examples showing practical usage patterns

## Features Summary

### Core Capabilities

- **22 MCP Tools**: Comprehensive SQL Server integration with API key management
- **JSON-RPC 2.0**: Standard protocol implementation
- **Multi-tier Authentication**: Master key and managed API keys with granular permissions
- **Connection Management**: Full CRUD operations for database connections
- **Security Features**: Encryption, key rotation, secure key generation, and API key management

### Authentication & Access Control

- **Master Key**: Full access to all endpoints and API key management
- **Managed API Keys**: Granular endpoint-specific permissions
- **Usage Tracking**: Monitor API key usage patterns and statistics
- **Expiration Management**: Time-based key expiration with audit trails
- **Dual Authentication Methods**: Bearer token and X-API-Key header support

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

## Response Data Summary

All API endpoints return structured JSON data following the JSON-RPC 2.0 specification. Here's a quick reference of what each endpoint returns:

| Endpoint                          | Primary Return Type | Key Data Fields                                                          |
| --------------------------------- | ------------------- | ------------------------------------------------------------------------ |
| **Initialize**                    | Status Object       | `success`, `message`, `connectionName`, `serverVersion`                  |
| **ExecuteQuery**                  | Array of Objects    | Query results as array of row objects with column data                   |
| **GetTableMetadata**              | Array of Objects    | `tableName`, `schemaName`, `columns[]`, `primaryKeys[]`, `foreignKeys[]` |
| **GetDatabaseObjectsMetadata**    | Complex Object      | `tables[]`, `views[]`, `procedures[]`, `functions[]`                     |
| **GetDatabaseObjectsByType**      | Array of Objects    | `objectName`, `objectType`, `schemaName`, metadata properties            |
| **GetSqlServerAgentJobs**         | Array of Objects    | `jobId`, `jobName`, `enabled`, `lastRunStatus`, schedule info            |
| **GetSqlServerAgentJobDetails**   | Complex Object      | `job{}`, `steps[]`, `schedules[]`, `executionHistory[]`                  |
| **GetSsisCatalogInfo**            | Complex Object      | `folders[]`, `projects[]`, `packages[]`, `environments[]`                |
| **GetAzureDevOpsInfo**            | Complex Object      | `projects[]`, `repositories[]`, `builds[]`, `workItems[]`                |
| **ListConnections**               | Array of Objects    | `name`, `description`, `type`, `status`, `serverInfo{}`                  |
| **AddConnection**                 | Status Object       | `success`, `message`, `testResult{}`, `createdOn`                        |
| **UpdateConnection**              | Status Object       | `success`, `message`, `changes{}`, `modifiedOn`                          |
| **RemoveConnection**              | Status Object       | `success`, `message`, `removedOn`                                        |
| **TestConnection**                | Status Object       | `success`, `serverInfo{}`, `authentication{}`, `capabilities{}`          |
| **GenerateSecureKey**             | Key Object          | `key`, `length`, `algorithm`, `createdOn`                                |
| **MigrateConnectionsToEncrypted** | Migration Object    | `summary{}`, `details[]`, `encryptionInfo{}`                             |
| **RotateKey**                     | Rotation Object     | `summary{}`, `rotationDetails{}`, `connectionResults[]`                  |

## Data Types and Formats

### Common Data Types

- **Timestamps**: ISO 8601 format with Z suffix (UTC): `"2024-06-10T10:30:15.123Z"`
- **Durations**: Seconds as integer: `1245`
- **Boolean Values**: Standard JSON booleans: `true`/`false`
- **Nullable Fields**: `null` for missing/empty values
- **Arrays**: Empty arrays `[]` when no data available
- **Objects**: Empty objects `{}` when no nested data

### Connection Status Values

- `"Connected"`: Currently active connection
- `"Available"`: Connection configured and tested successfully
- `"Error"`: Connection has issues
- `"Testing"`: Connection test in progress

### Object Types

- `"TABLE"`: Database table
- `"VIEW"`: Database view
- `"PROCEDURE"`: Stored procedure
- `"FUNCTION"`: User-defined function

### Job Status Values

- `"Succeeded"`: Job completed successfully
- `"Failed"`: Job failed with errors
- `"Retry"`: Job is retrying after failure
- `"Cancelled"`: Job was cancelled
- `"In Progress"`: Job is currently running

## Rate Limiting and Performance

### Response Size Guidelines

- **Small responses** (< 1KB): Simple status operations, connection tests
- **Medium responses** (1KB - 100KB): Table metadata, job lists, connection lists
- **Large responses** (100KB - 10MB): Query results, detailed object metadata
- **Very large responses** (> 10MB): Complex queries, full database schemas

### Performance Tips

1. **Use schema filtering** when possible to reduce response size
2. **Limit query results** with TOP/LIMIT clauses for large datasets
3. **Cache connection lists** as they change infrequently
4. **Use specific object types** instead of "ALL" when you know what you need
5. **Test connections periodically** rather than with every request

## Integration Examples

### PowerShell Integration

```powershell
# Complete workflow example
$apiKey = "your-api-key"
$baseUrl = "http://localhost:3001"

# 1. Test API connectivity
$listResult = Invoke-RestMethod -Uri $baseUrl -Method Post -Headers @{
    "Authorization" = "Bearer $apiKey"
    "Content-Type" = "application/json"
} -Body (@{
    jsonrpc = "2.0"
    id = 1
    method = "ListConnections"
    params = @{}
} | ConvertTo-Json)

Write-Host "Found $($listResult.result.Count) connections"

# 2. Execute query with error handling
try {
    $queryResult = Invoke-RestMethod -Uri $baseUrl -Method Post -Headers @{
        "Authorization" = "Bearer $apiKey"
        "Content-Type" = "application/json"
    } -Body (@{
        jsonrpc = "2.0"
        id = 2
        method = "ExecuteQuery"
        params = @{
            query = "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES"
            connectionName = "DefaultConnection"
        }
    } | ConvertTo-Json)

    $tableCount = $queryResult.result[0].TableCount
    Write-Host "Database contains $tableCount tables"
} catch {
    Write-Error "Query failed: $($_.Exception.Message)"
}
```

### C# Integration

```csharp
// Example with comprehensive error handling
public async Task<DatabaseSummary> GetDatabaseSummaryAsync(string connectionName)
{
    try
    {
        // Get table count
        var tableQuery = await _mcpClient.CallMcpApiAsync<List<Dictionary<string, object>>>(
            "ExecuteQuery",
            new {
                query = "SELECT COUNT(*) as count FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                connectionName
            });
        var tableCount = Convert.ToInt32(tableQuery[0]["count"]);

        // Get view count
        var viewQuery = await _mcpClient.CallMcpApiAsync<List<Dictionary<string, object>>>(
            "ExecuteQuery",
            new {
                query = "SELECT COUNT(*) as count FROM INFORMATION_SCHEMA.VIEWS",
                connectionName
            });
        var viewCount = Convert.ToInt32(viewQuery[0]["count"]);

        // Get stored procedure count
        var procQuery = await _mcpClient.CallMcpApiAsync<List<Dictionary<string, object>>>(
            "ExecuteQuery",
            new {
                query = "SELECT COUNT(*) as count FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'",
                connectionName
            });
        var procedureCount = Convert.ToInt32(procQuery[0]["count"]);

        return new DatabaseSummary
        {
            TableCount = tableCount,
            ViewCount = viewCount,
            ProcedureCount = procedureCount,
            ConnectionName = connectionName,
            GeneratedAt = DateTime.UtcNow
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to generate database summary for connection {ConnectionName}", connectionName);
        throw;
    }
}
```

### Python Integration

```python
async def get_database_health_report(client: TypedMcpApiClient, connection_name: str) -> dict:
    """Generate a comprehensive database health report"""
    try:
        # Get basic database info
        db_info = await client.execute_query(
            "SELECT name, create_date, compatibility_level FROM sys.databases WHERE name = DB_NAME()",
            connection_name
        )

        # Get table metadata
        tables = await client.get_table_metadata(connection_name)

        # Get SQL Agent jobs if available
        try:
            agent_jobs = await client.get_sql_server_agent_jobs(connection_name)
        except Exception:
            agent_jobs = []  # SQL Agent might not be available

        # Get SSIS info if available
        try:
            ssis_info = await client.get_ssis_catalog_info(connection_name)
        except Exception:
            ssis_info = None  # SSIS might not be configured

        return {
            "database_info": db_info[0] if db_info else None,
            "table_count": len(tables),
            "tables_with_foreign_keys": len([t for t in tables if t.foreign_keys]),
            "tables_with_primary_keys": len([t for t in tables if t.primary_keys]),
            "agent_jobs": {
                "total": len(agent_jobs),
                "enabled": len([j for j in agent_jobs if j.get("enabled", False)]),
                "failed_last_run": len([j for j in agent_jobs if j.get("lastRunStatus") == "Failed"])
            },
            "ssis_deployed": ssis_info is not None,
            "generated_at": datetime.utcnow().isoformat() + "Z"
        }
    except Exception as e:
        raise Exception(f"Failed to generate health report: {str(e)}")
```

This comprehensive documentation provides complete examples of request/response patterns, error handling, and real-world integration scenarios for all SQL Server MCP API endpoints.
