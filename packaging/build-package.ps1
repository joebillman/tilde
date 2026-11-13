<#
.SYNOPSIS
  Builds the Tilde Keys MSIX packages (x64 and ARM64) and the .msixbundle
  that gets uploaded to the Microsoft Store.

.DESCRIPTION
  For each architecture: publish self-contained, lay out the package folder
  (app files, images, manifest), index the images with MakePri, and pack with
  MakeAppx. Then bundle the packages. Needs the .NET SDK and the Windows SDK
  (for MakeAppx.exe and MakePri.exe). Output goes to packaging\out\.

  The packages are unsigned: the Store signs them after certification.

.PARAMETER Register
  Also install the x64 package folder on this PC for testing. Needs Developer
  Mode (Settings > System > For developers). Replaces any earlier test install.

.EXAMPLE
  pwsh packaging/build-package.ps1
  pwsh packaging/build-package.ps1 -Register
#>
param(
    [switch]$Register,
    [string[]]$Architectures = @('x64', 'arm64')
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $PSScriptRoot 'out'
$packageName = 'JoeBillman.TildeKeys'

# Package versions have four parts; the Store requires the last one to be 0.
[xml]$project = Get-Content (Join-Path $root 'src\Tilde.App\Tilde.App.csproj')
$appVersion = @($project.Project.PropertyGroup.Version | Where-Object { $_ })[0]
$version = "$appVersion.0"

# Use the newest installed Windows SDK tools.
$sdk = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Directory |
    Where-Object { $_.Name -match '^10\.' -and (Test-Path (Join-Path $_.FullName 'x64\makeappx.exe')) } |
    Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (-not $sdk) { throw 'MakeAppx.exe not found. Install the Windows SDK.' }
$makeappx = Join-Path $sdk.FullName 'x64\makeappx.exe'
$makepri = Join-Path $sdk.FullName 'x64\makepri.exe'

function Invoke-Tool([string]$exe, [string[]]$arguments) {
    & $exe @arguments | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "$(Split-Path $exe -Leaf) failed with exit code $LASTEXITCODE." }
}

if ($Register) {
    # The test install runs from out\layout-x64, so remove it before rebuilding over it.
    Get-AppxPackage -Name $packageName | Remove-AppxPackage
}

Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force (Join-Path $out 'msix') | Out-Null

$priConfig = Join-Path $out 'priconfig.xml'
Invoke-Tool $makepri @('createconfig', '/cf', $priConfig, '/dq', 'en-US', '/pv', '10.0.0', '/o')
# The default config moves non-100% images into separate "resource packs". The
# images are tiny, so keep every scale in the main package instead.
[xml]$config = Get-Content $priConfig
$packaging = $config.SelectSingleNode('//packaging')
if ($packaging) { [void]$packaging.ParentNode.RemoveChild($packaging) }
$config.Save($priConfig)

foreach ($arch in $Architectures) {
    Write-Host "== $arch" -ForegroundColor Cyan
    $layout = Join-Path $out "layout-$arch"

    # Self-contained: the laptop needs no .NET installed. English resources only,
    # since the app's text is English; this keeps the package smaller.
    Invoke-Tool 'dotnet' @('publish', (Join-Path $root 'src\Tilde.App'), '-c', 'Release', '-r', "win-$arch",
        '--self-contained', '-o', $layout, '-p:DebugType=None', '-p:SatelliteResourceLanguages=en')

    Copy-Item (Join-Path $PSScriptRoot 'Assets') (Join-Path $layout 'Assets') -Recurse
    $manifest = (Get-Content (Join-Path $PSScriptRoot 'AppxManifest.xml') -Raw).
        Replace('$VERSION$', $version).Replace('$ARCH$', $arch)
    Set-Content (Join-Path $layout 'AppxManifest.xml') $manifest -Encoding utf8

    # resources.pri tells Windows which image to use at each display scale.
    Invoke-Tool $makepri @('new', '/pr', $layout, '/cf', $priConfig,
        '/mn', (Join-Path $layout 'AppxManifest.xml'), '/of', (Join-Path $layout 'resources.pri'), '/o')

    Invoke-Tool $makeappx @('pack', '/d', $layout, '/p', (Join-Path $out "msix\TildeKeys_${version}_$arch.msix"), '/o')
}

$bundle = Join-Path $out "TildeKeys_$version.msixbundle"
Invoke-Tool $makeappx @('bundle', '/d', (Join-Path $out 'msix'), '/p', $bundle, '/bv', $version, '/o')

Write-Host ''
Get-ChildItem (Join-Path $out 'msix'), $bundle | ForEach-Object {
    '{0,-40} {1,8:N1} MB' -f $_.Name, ($_.Length / 1MB)
} | Write-Host

if ($Register) {
    Add-AppxPackage -Register (Join-Path $out 'layout-x64\AppxManifest.xml')
    Write-Host "Installed $packageName for testing. Find 'Tilde Keys' in the Start menu." -ForegroundColor Green
}
