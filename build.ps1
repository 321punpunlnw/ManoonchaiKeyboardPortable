$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root "src"
$payloadDir = Join-Path $src "payload"
$payloadZip = Join-Path $env:TEMP "manoonchai-keyboard-portable-payload.zip"
$out = Join-Path $root "ManoonchaiKeyboardPortable.exe"
$icon = Join-Path $payloadDir "layouts\manoonchai\manoonchai.ico"
$manifest = Join-Path $src "app.manifest"
$launcher = Join-Path $src "Launcher.cs"

if (Test-Path $payloadZip) {
    Remove-Item -Force $payloadZip
}

Compress-Archive -Path (Join-Path $payloadDir "*") -DestinationPath $payloadZip -Force

$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    throw "Could not find csc.exe at $csc"
}

$args = @(
    "/nologo",
    "/target:winexe",
    "/platform:x86",
    "/optimize+",
    "/out:$out",
    "/win32icon:$icon",
    "/win32manifest:$manifest",
    "/resource:$payloadZip,Manoonchai.Payload.zip",
    "/reference:System.IO.Compression.dll",
    "/reference:System.IO.Compression.FileSystem.dll",
    "/reference:System.Windows.Forms.dll",
    $launcher
)

& $csc @args
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

Remove-Item -Force $payloadZip
Write-Host "Built $out"
