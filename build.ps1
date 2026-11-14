param([string]$ElinPath = 'C:\Program Files\Steam\steamapps\common\Elin')
$ErrorActionPreference = 'Stop'
dotnet build "$PSScriptRoot\Orbblade.csproj" -c Release "-p:ElinPath=$ElinPath"
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
$package = Join-Path $PSScriptRoot 'bin\package\Mod_Orbblade'
New-Item -ItemType Directory -Force -Path $package | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot\bin\Release\netstandard2.0\Orbblade.dll" -Destination $package -Force
Copy-Item -LiteralPath "$PSScriptRoot\package.xml" -Destination $package -Force
Compress-Archive -Path $package -DestinationPath "$PSScriptRoot\bin\Orbblade-0.1.0.zip" -Force
Write-Output "Package created at $PSScriptRoot\bin\Orbblade-0.1.0.zip"
