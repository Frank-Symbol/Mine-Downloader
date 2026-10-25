#Requires -Version 5.1
param([ValidateSet('Debug','Release','Snapshot')][string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe"
if (Test-Path $vswhere) {
    $found = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\amd64\MSBuild.exe' | Select-Object -First 1
    if ($found) { $msbuild = $found }
}
if (-not (Test-Path $msbuild)) { throw 'Install Visual Studio 2022 MSBuild and .NET desktop build tools.' }
$sdk = Get-ChildItem "$env:ProgramFiles\dotnet\sdk" -Directory | Sort-Object { [version](($_.Name -split '-')[0]) } -Descending | Select-Object -First 1
if (-not $sdk) { throw 'Install a .NET SDK.' }
$env:MSBuildSDKsPath = Join-Path $sdk.FullName 'Sdks'
$env:MSBuildEnableWorkloadResolver = 'false'
$common = @('-m:1','-nr:false','-p:UseSharedCompilation=false',"-p:Configuration=$Configuration",'-p:MSBuildEnableWorkloadResolver=false','-p:SignManifests=false','-p:SignAssembly=false','-v:m','-nologo')
$refs = Join-Path $env:USERPROFILE '.nuget\packages\microsoft.netframework.referenceassemblies.net48\1.0.3\build'
if (Test-Path $refs) { $common += "-p:TargetFrameworkRootPath=$refs" }
Push-Location $root
try {
    # Build these first: the WPF Resources.resx embeds PCLCS/bin/*.dll.
    & $msbuild 'PCLCS\PCLCS.csproj' -restore @common
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & $msbuild 'Plain Craft Launcher 2\Plain Craft Launcher 2.vbproj' @common
    exit $LASTEXITCODE
} finally {
    Pop-Location
}
