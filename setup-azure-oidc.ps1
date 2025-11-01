# Script tạo Service Principal và Federated Credential cho GitHub Actions
# Chạy trong PowerShell với quyền admin

# Cài Azure CLI nếu chưa có
# winget install -e --id Microsoft.AzureCLI

# Login vào Azure
az login

# Đặt subscription
az account set --subscription '6d6d5629-96f5-4bb9-8b30-1004108e6a99'

# Tạo App Registration
$appName = 'WebRoomEnglish-GitHub-Actions'
$app = az ad app create --display-name $appName --query '{appId:appId, objectId:id}' -o json | ConvertFrom-Json

Write-Host 'App created:'
Write-Host 'Application (client) ID:' $app.appId
Write-Host 'Object ID:' $app.objectId

# Tạo Service Principal
$sp = az ad sp create --id $app.appId --query '{objectId:id}' -o json | ConvertFrom-Json
Write-Host 'Service Principal Object ID:' $sp.objectId

# Gán quyền Contributor cho WebRoomEnglish
$resourceGroup = 'ResourceGroupPhong'
az role assignment create --assignee $app.appId --role Contributor --scope /subscriptions/6d6d5629-96f5-4bb9-8b30-1004108e6a99/resourceGroups/$resourceGroup

# Tạo Federated Credential cho GitHub
$repoOwner = 'phongvovan08'
$repoName = 'RoomEnglish'
$branch = 'main'

$fedCred = @{
    name = 'github-main-branch'
    issuer = 'https://token.actions.githubusercontent.com'
    subject = \"repo:$repoOwner/$repoName:ref:refs/heads/$branch\"
    audiences = @('api://AzureADTokenExchange')
} | ConvertTo-Json

az ad app federated-credential create --id $app.appId --parameters \"$fedCred\"

# Lấy Tenant ID
$tenantId = az account show --query tenantId -o tsv

Write-Host "
=== GitHub Secrets ==="
Write-Host 'AZURE_CLIENT_ID:' $app.appId
Write-Host 'AZURE_TENANT_ID:' $tenantId
Write-Host 'AZURE_SUBSCRIPTION_ID: 6d6d5629-96f5-4bb9-8b30-1004108e6a99'
