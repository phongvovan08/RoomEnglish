# Start backend server
Push-Location $PSScriptRoot
Write-Host "Starting server from: $PWD"
dotnet run --no-build
Pop-Location
