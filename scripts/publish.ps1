$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $root

$configuration = if ($env:CONFIGURATION) { $env:CONFIGURATION } else { 'Release' }
$samplesDir = if ($env:SAMPLES_DIR) { $env:SAMPLES_DIR } else { './samples' }
$publishDir = if ($env:PUBLISH_DIR) { $env:PUBLISH_DIR } else { './build/publish' }

function Get-Rid {
    $os = if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        'win'
    } elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
        'linux'
    } else {
        throw "unsupported OS"
    }

    $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    $archName = switch ($arch) {
        'X64' { 'x64' }
        'Arm64' { 'arm64' }
        default { 'x64' }
    }
    return "$os-$archName"
}

$rid = Get-Rid

foreach ($projectDir in Get-ChildItem (Join-Path $root $samplesDir) -Directory) {
    $csprojs = Get-ChildItem -Path (Join-Path $projectDir.FullName '*.csproj') -File
    foreach ($csproj in $csprojs) {
        if (-not (Select-String -Path $csproj.FullName -Pattern '<OutputType>Exe</OutputType>' -Quiet)) { continue }
        $name = $csproj.BaseName
        $projectPath = $csproj.Directory.FullName
        $source = Join-Path $projectPath "bin/$configuration/net10.0/$rid/publish"
        $out = Join-Path $root "$publishDir/$rid/$name"
        New-Item -ItemType Directory -Force -Path $out | Out-Null

        dotnet publish $csproj.FullName `
            -c $configuration `
            -r $rid `
            --self-contained `
            -p:StripSymbols=true `
            --verbosity minimal
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

        Copy-Item -Path (Join-Path $source '*') -Destination $out -Force
        Write-Host "published: $out"
    }
}
