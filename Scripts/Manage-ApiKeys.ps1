# API Key Management Script for SQL Server MCP
# This script provides functions to manage API keys in the multi-tier authentication system

param(
    [string]$Action = "List",
    [string]$Name,
    [string]$Description,
    [string[]]$AllowedEndpoints,
    [string]$CreatedBy,
    [string]$ExpiresOn,
    [string]$Id,
    [bool]$IsActive,
    [string]$BaseUrl = "http://localhost:3001",
    [string]$MasterKey = $env:MSSQL_MCP_API_KEY
)

# Function to make JSON-RPC requests
function Invoke-McpRequest {
    param(
        [string]$Method,
        [hashtable]$Params = @{},
        [string]$Url = $BaseUrl,
        [string]$ApiKey = $MasterKey
    )
    
    if (-not $ApiKey) {
        Write-Error "Master API key is required. Set MSSQL_MCP_API_KEY environment variable or provide -MasterKey parameter."
        return
    }
    
    $requestBody = @{
        jsonrpc = "2.0"
        id      = Get-Random
        method  = $Method
        params  = $Params
    } | ConvertTo-Json -Depth 10
    
    $headers = @{
        "Content-Type" = "application/json"
        "X-API-Key"    = $ApiKey
    }
    
    try {
        $response = Invoke-RestMethod -Uri $Url -Method POST -Body $requestBody -Headers $headers
        return $response
    }
    catch {
        Write-Error "Request failed: $($_.Exception.Message)"
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Error "Response: $responseBody"
        }
        return $null
    }
}

# Function to create a new API key
function New-ApiKey {
    param(
        [Parameter(Mandatory)]
        [string]$Name,
        
        [string]$Description = "",
        
        [Parameter(Mandatory)]
        [string[]]$AllowedEndpoints,
        
        [string]$CreatedBy = $env:USERNAME,
        
        [string]$ExpiresOn
    )
    
    Write-Host "Creating new API key: $Name" -ForegroundColor Yellow
    
    $params = @{
        name             = $Name
        description      = $Description
        allowedEndpoints = $AllowedEndpoints
        createdBy        = $CreatedBy
    }
    
    if ($ExpiresOn) {
        $params.expiresOn = $ExpiresOn
    }
    
    $response = Invoke-McpRequest -Method "CreateApiKey" -Params $params
    
    if ($response -and $response.result.success) {
        Write-Host "✅ API key created successfully!" -ForegroundColor Green
        Write-Host "🔑 API Key: " -NoNewline -ForegroundColor Cyan
        Write-Host $response.result.apiKey -ForegroundColor White
        Write-Host "📋 ID: $($response.result.keyInfo.id)" -ForegroundColor Gray
        Write-Host "📅 Created: $($response.result.keyInfo.createdOn)" -ForegroundColor Gray
        
        if ($response.result.keyInfo.expiresOn) {
            Write-Host "⏰ Expires: $($response.result.keyInfo.expiresOn)" -ForegroundColor Yellow
        }
        
        Write-Host ""
        Write-Host "💡 Save this API key securely - it cannot be retrieved again!" -ForegroundColor Magenta
        
        return $response.result
    }
    else {
        Write-Error "Failed to create API key: $($response.result.message)"
        return $null
    }
}

# Function to list all API keys
function Get-ApiKeys {
    Write-Host "Retrieving API keys..." -ForegroundColor Yellow
    
    $response = Invoke-McpRequest -Method "ListApiKeys"
    
    if ($response -and $response.result.success) {
        Write-Host "📊 Found $($response.result.count) API keys:" -ForegroundColor Green
        Write-Host ""
        
        foreach ($key in $response.result.apiKeys) {
            Write-Host "🔑 $($key.name)" -ForegroundColor Cyan
            Write-Host "   ID: $($key.id)" -ForegroundColor Gray
            Write-Host "   Description: $($key.description)" -ForegroundColor Gray
            Write-Host "   Status: " -NoNewline -ForegroundColor Gray
            if ($key.isActive) {
                Write-Host "Active" -ForegroundColor Green
            }
            else {
                Write-Host "Inactive" -ForegroundColor Red
            }
            Write-Host "   Endpoints: $($key.allowedEndpoints -join ', ')" -ForegroundColor Gray
            Write-Host "   Created: $($key.createdOn) by $($key.createdBy)" -ForegroundColor Gray
            Write-Host "   Last Used: $($key.lastUsed) (Used $($key.usageCount) times)" -ForegroundColor Gray
            
            if ($key.expiresOn) {
                $expiry = [DateTime]::Parse($key.expiresOn)
                if ($expiry -lt (Get-Date)) {
                    Write-Host "   ⚠️  EXPIRED: $($key.expiresOn)" -ForegroundColor Red
                }
                else {
                    Write-Host "   Expires: $($key.expiresOn)" -ForegroundColor Yellow
                }
            }
            else {
                Write-Host "   Expires: Never" -ForegroundColor Green
            }
            Write-Host ""
        }
        
        return $response.result.apiKeys
    }
    else {
        Write-Error "Failed to retrieve API keys"
        return $null
    }
}

# Function to update an API key
function Update-ApiKey {
    param(
        [Parameter(Mandatory)]
        [string]$Id,
        
        [string]$Name,
        [string]$Description,
        [string[]]$AllowedEndpoints,
        [bool]$IsActive,
        [string]$ExpiresOn
    )
    
    Write-Host "Updating API key: $Id" -ForegroundColor Yellow
    
    $params = @{
        id = $Id
    }
    
    if ($Name) { $params.name = $Name }
    if ($Description) { $params.description = $Description }
    if ($AllowedEndpoints) { $params.allowedEndpoints = $AllowedEndpoints }
    if ($PSBoundParameters.ContainsKey('IsActive')) { $params.isActive = $IsActive }
    if ($ExpiresOn) { $params.expiresOn = $ExpiresOn }
    
    $response = Invoke-McpRequest -Method "UpdateApiKey" -Params $params
    
    if ($response -and $response.result.success) {
        Write-Host "✅ API key updated successfully!" -ForegroundColor Green
        return $response.result
    }
    else {
        Write-Error "Failed to update API key: $($response.result.message)"
        return $null
    }
}

# Function to remove an API key
function Remove-ApiKey {
    param(
        [Parameter(Mandatory)]
        [string]$Id,
        
        [switch]$Force
    )
    
    if (-not $Force) {
        $confirm = Read-Host "Are you sure you want to remove API key '$Id'? (y/N)"
        if ($confirm -ne 'y' -and $confirm -ne 'Y') {
            Write-Host "Operation cancelled." -ForegroundColor Yellow
            return
        }
    }
    
    Write-Host "Removing API key: $Id" -ForegroundColor Yellow
    
    $response = Invoke-McpRequest -Method "RemoveApiKey" -Params @{ id = $Id }
    
    if ($response -and $response.result.success) {
        Write-Host "✅ API key removed successfully!" -ForegroundColor Green
        return $response.result
    }
    else {
        Write-Error "Failed to remove API key: $($response.result.message)"
        return $null
    }
}

# Function to get API key info by name
function Get-ApiKeyInfo {
    param(
        [Parameter(Mandatory)]
        [string]$Name
    )
    
    Write-Host "Retrieving API key info: $Name" -ForegroundColor Yellow
    
    $response = Invoke-McpRequest -Method "GetApiKeyInfo" -Params @{ name = $Name }
    
    if ($response -and $response.result.success) {
        $key = $response.result.keyInfo
        Write-Host "🔑 API Key Information:" -ForegroundColor Cyan
        Write-Host "   Name: $($key.name)" -ForegroundColor White
        Write-Host "   ID: $($key.id)" -ForegroundColor Gray
        Write-Host "   Description: $($key.description)" -ForegroundColor Gray
        Write-Host "   Status: " -NoNewline -ForegroundColor Gray
        if ($key.isActive) {
            Write-Host "Active" -ForegroundColor Green
        }
        else {
            Write-Host "Inactive" -ForegroundColor Red
        }
        Write-Host "   Allowed Endpoints: $($key.allowedEndpoints -join ', ')" -ForegroundColor Gray
        Write-Host "   Created: $($key.createdOn) by $($key.createdBy)" -ForegroundColor Gray
        Write-Host "   Modified: $($key.modifiedOn)" -ForegroundColor Gray
        Write-Host "   Last Used: $($key.lastUsed)" -ForegroundColor Gray
        Write-Host "   Usage Count: $($key.usageCount)" -ForegroundColor Gray
        
        if ($key.expiresOn) {
            $expiry = [DateTime]::Parse($key.expiresOn)
            if ($expiry -lt (Get-Date)) {
                Write-Host "   ⚠️  EXPIRED: $($key.expiresOn)" -ForegroundColor Red
            }
            else {
                Write-Host "   Expires: $($key.expiresOn)" -ForegroundColor Yellow
            }
        }
        else {
            Write-Host "   Expires: Never" -ForegroundColor Green
        }
        
        return $key
    }
    else {
        Write-Error "Failed to retrieve API key info: $($response.result.message)"
        return $null
    }
}

# Main script execution
switch ($Action.ToLower()) {
    "create" {
        if (-not $Name -or -not $AllowedEndpoints) {
            Write-Error "Name and AllowedEndpoints are required for creating an API key"
            Write-Host "Example: .\Manage-ApiKeys.ps1 -Action Create -Name 'MyKey' -AllowedEndpoints @('GetTables','QueryDatabase') -Description 'Read-only access'"
            exit 1
        }
        New-ApiKey -Name $Name -Description $Description -AllowedEndpoints $AllowedEndpoints -CreatedBy $CreatedBy -ExpiresOn $ExpiresOn
    }
    
    "list" {
        Get-ApiKeys
    }
    
    "update" {
        if (-not $Id) {
            Write-Error "ID is required for updating an API key"
            exit 1
        }
        Update-ApiKey -Id $Id -Name $Name -Description $Description -AllowedEndpoints $AllowedEndpoints -IsActive $IsActive -ExpiresOn $ExpiresOn
    }
    
    "remove" {
        if (-not $Id) {
            Write-Error "ID is required for removing an API key"
            exit 1
        }
        Remove-ApiKey -Id $Id
    }
    
    "info" {
        if (-not $Name) {
            Write-Error "Name is required for getting API key info"
            exit 1
        }
        Get-ApiKeyInfo -Name $Name
    }
    
    default {
        Write-Host "SQL Server MCP API Key Management Script" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Usage:" -ForegroundColor Yellow
        Write-Host "  .\Manage-ApiKeys.ps1 -Action <Action> [parameters]" -ForegroundColor White
        Write-Host ""
        Write-Host "Actions:" -ForegroundColor Yellow
        Write-Host "  Create  - Create a new API key" -ForegroundColor White
        Write-Host "  List    - List all API keys" -ForegroundColor White
        Write-Host "  Update  - Update an existing API key" -ForegroundColor White
        Write-Host "  Remove  - Remove an API key" -ForegroundColor White
        Write-Host "  Info    - Get detailed info about an API key" -ForegroundColor White
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Yellow
        Write-Host "  # List all API keys" -ForegroundColor Gray
        Write-Host "  .\Manage-ApiKeys.ps1 -Action List" -ForegroundColor White
        Write-Host ""
        Write-Host "  # Create a read-only API key" -ForegroundColor Gray
        Write-Host "  .\Manage-ApiKeys.ps1 -Action Create -Name 'ReadOnly' -AllowedEndpoints @('GetTables','QueryDatabase') -Description 'Read-only access'" -ForegroundColor White
        Write-Host ""
        Write-Host "  # Update an API key" -ForegroundColor Gray
        Write-Host "  .\Manage-ApiKeys.ps1 -Action Update -Id 'key-id-here' -IsActive `$false" -ForegroundColor White
        Write-Host ""
        Write-Host "  # Get API key info" -ForegroundColor Gray
        Write-Host "  .\Manage-ApiKeys.ps1 -Action Info -Name 'ReadOnly'" -ForegroundColor White
        Write-Host ""
        Write-Host "  # Remove an API key" -ForegroundColor Gray
        Write-Host "  .\Manage-ApiKeys.ps1 -Action Remove -Id 'key-id-here'" -ForegroundColor White
        Write-Host ""
        Write-Host "Available Endpoints:" -ForegroundColor Yellow
        $endpoints = @(
            "Initialize", "ExecuteQuery", "GetTableMetadata", "GetDatabaseObjectsMetadata", "GetDatabaseObjectsByType",
            "GetSqlServerAgentJobs", "GetSqlServerAgentJobDetails", "GetSsisCatalogInfo", "GetAzureDevOpsInfo",
            "AddConnection", "UpdateConnection", "RemoveConnection", "ListConnections", "TestConnection",
            "GenerateSecureKey", "MigrateConnectionsToEncrypted", "RotateKey"
        )
        Write-Host "  $($endpoints -join ', ')" -ForegroundColor White
        Write-Host ""
        Write-Host "Note: Master key access is required for all API key management operations." -ForegroundColor Magenta
    }
}
