# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/DragonEssentials/*" -Force -Recurse
dotnet publish "./DragonEssentials.csproj" -c Release -o "$env:RELOADEDIIMODS/DragonEssentials" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location