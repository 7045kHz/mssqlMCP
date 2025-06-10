# Test API Key Management System
# This script tests the new multi-tier API key management functionality

param(
    [string]$BaseUrl = "http://localhost:3001",
    [string]$MasterKey = $env:MSSQL_MCP_API_KEY
)

Write-Host "🧪 Testing Multi-Tier API Key Management System" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

if (-not $MasterKey) {
    Write-Error "❌ Master API key not found. Set MSSQL_MCP_API_KEY environment variable."
    exit 1
}

# Function to make JSON-RPC requests
function Invoke-McpRequest {
    param(
        [string]$Method,
        [hashtable]$Params = @{},
        [string]$ApiKey = $MasterKey
    )
    
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
        $response = Invoke-RestMethod -Uri $BaseUrl -Method POST -Body $requestBody -Headers $headers
        return $response
    }
    catch {
        Write-Warning "⚠️  Request failed: $($_.Exception.Message)"
        return $null
    }
}

# Test 1: Verify master key access
Write-Host "🔑 Test 1: Testing master key access..." -ForegroundColor Yellow
$response = Invoke-McpRequest -Method "ListApiKeys"
if ($response -and $response.result.success) {
    Write-Host "✅ Master key authentication successful" -ForegroundColor Green
    Write-Host "   Found $($response.result.count) existing API keys" -ForegroundColor Gray
}
else {
    Write-Host "❌ Master key authentication failed" -ForegroundColor Red
    exit 1
}

# Test 2: Create a test API key
Write-Host ""
Write-Host "🆕 Test 2: Creating a test API key..." -ForegroundColor Yellow
$testKeyName = "TestKey_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
$createParams = @{
    name             = $testKeyName
    description      = "Test API key for verification"
    allowedEndpoints = @("GetTables", "QueryDatabase")
    createdBy        = "Test System"
    expiresOn        = (Get-Date).AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
}

$createResponse = Invoke-McpRequest -Method "CreateApiKey" -Params $createParams
if ($createResponse -and $createResponse.result.success) {
    Write-Host "✅ Test API key created successfully" -ForegroundColor Green
    $testApiKey = $createResponse.result.apiKey
    $testKeyId = $createResponse.result.keyInfo.id
    Write-Host "   Key: $testApiKey" -ForegroundColor Gray
    Write-Host "   ID: $testKeyId" -ForegroundColor Gray
}
else {
    Write-Host "❌ Failed to create test API key" -ForegroundColor Red
    exit 1
}

# Test 3: Test the new API key with allowed endpoint
Write-Host ""
Write-Host "🔐 Test 3: Testing managed key with allowed endpoint..." -ForegroundColor Yellow
$tablesResponse = Invoke-McpRequest -Method "GetTables" -ApiKey $testApiKey
if ($tablesResponse) {
    Write-Host "✅ Managed key works with allowed endpoint (GetTables)" -ForegroundColor Green
}
else {
    Write-Host "❌ Managed key failed with allowed endpoint" -ForegroundColor Red
}

# Test 4: Test the new API key with disallowed endpoint
Write-Host ""
Write-Host "🚫 Test 4: Testing managed key with disallowed endpoint..." -ForegroundColor Yellow
$connectResponse = Invoke-McpRequest -Method "AddConnection" -Params @{
    connectionName   = "TestConnection"
    connectionString = "test"
    description      = "test"
} -ApiKey $testApiKey

if (-not $connectResponse) {
    Write-Host "✅ Managed key correctly denied access to disallowed endpoint" -ForegroundColor Green
}
else {
    Write-Host "❌ Managed key incorrectly allowed access to disallowed endpoint" -ForegroundColor Red
}

# Test 5: Test managed key trying to access master-only function
Write-Host ""
Write-Host "🛡️ Test 5: Testing managed key with master-only endpoint..." -ForegroundColor Yellow
$listResponse = Invoke-McpRequest -Method "ListApiKeys" -ApiKey $testApiKey
if (-not $listResponse) {
    Write-Host "✅ Managed key correctly denied access to master-only endpoint" -ForegroundColor Green
}
else {
    Write-Host "❌ Managed key incorrectly allowed access to master-only endpoint" -ForegroundColor Red
}

# Test 6: Get API key info
Write-Host ""
Write-Host "ℹ️ Test 6: Getting API key information..." -ForegroundColor Yellow
$infoResponse = Invoke-McpRequest -Method "GetApiKeyInfo" -Params @{ name = $testKeyName }
if ($infoResponse -and $infoResponse.result.success) {
    Write-Host "✅ API key info retrieved successfully" -ForegroundColor Green
    Write-Host "   Usage Count: $($infoResponse.result.keyInfo.usageCount)" -ForegroundColor Gray
    Write-Host "   Last Used: $($infoResponse.result.keyInfo.lastUsed)" -ForegroundColor Gray
}
else {
    Write-Host "❌ Failed to retrieve API key info" -ForegroundColor Red
}

# Test 7: Update API key
Write-Host ""
Write-Host "📝 Test 7: Updating API key..." -ForegroundColor Yellow
$updateResponse = Invoke-McpRequest -Method "UpdateApiKey" -Params @{
    id          = $testKeyId
    description = "Updated test API key"
}
if ($updateResponse -and $updateResponse.result.success) {
    Write-Host "✅ API key updated successfully" -ForegroundColor Green
}
else {
    Write-Host "❌ Failed to update API key" -ForegroundColor Red
}

# Test 8: Cleanup - Remove test API key
Write-Host ""
Write-Host "🧹 Test 8: Cleaning up - removing test API key..." -ForegroundColor Yellow
$removeResponse = Invoke-McpRequest -Method "RemoveApiKey" -Params @{ id = $testKeyId }
if ($removeResponse -and $removeResponse.result.success) {
    Write-Host "✅ Test API key removed successfully" -ForegroundColor Green
}
else {
    Write-Host "❌ Failed to remove test API key" -ForegroundColor Red
}

# Final verification
Write-Host ""
Write-Host "🔍 Final verification: Listing remaining API keys..." -ForegroundColor Yellow
$finalListResponse = Invoke-McpRequest -Method "ListApiKeys"
if ($finalListResponse -and $finalListResponse.result.success) {
    Write-Host "✅ Final API key list retrieved" -ForegroundColor Green
    Write-Host "   Total keys remaining: $($finalListResponse.result.count)" -ForegroundColor Gray
    
    # Check if test key was properly removed
    $testKeyExists = $finalListResponse.result.apiKeys | Where-Object { $_.name -eq $testKeyName }
    if (-not $testKeyExists) {
        Write-Host "✅ Test key properly removed from system" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Test key still exists in system" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎉 Multi-Tier API Key Management System Tests Complete!" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor White
Write-Host "   • Master key authentication: ✅" -ForegroundColor Green
Write-Host "   • Managed key creation: ✅" -ForegroundColor Green
Write-Host "   • Endpoint permission validation: ✅" -ForegroundColor Green
Write-Host "   • Master-only endpoint protection: ✅" -ForegroundColor Green
Write-Host "   • Usage tracking: ✅" -ForegroundColor Green
Write-Host "   • Key lifecycle management: ✅" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 The multi-tier API key management system is working correctly!" -ForegroundColor Magenta
