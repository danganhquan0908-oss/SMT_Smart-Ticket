$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$pidPath = Join-Path $projectRoot '.local\web.pid'
if (-not (Test-Path -LiteralPath $pidPath)) { return }

$recordedWebId = 0
if (-not [int]::TryParse((Get-Content -LiteralPath $pidPath -Raw).Trim(), [ref]$recordedWebId) -or $recordedWebId -le 0) {
    throw 'Invalid .local/web.pid. No process was stopped.'
}
$recordedProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$recordedWebId" -ErrorAction SilentlyContinue
if ($recordedProcess) {
    $webDll = Join-Path $projectRoot 'bin\Debug\net10.0\BusGoSmartTicket.dll'
    $expectedArgument = '(?i)(?:^|\s)"?' + [regex]::Escape($webDll) + '"?(?:\s|$)'
    if ($recordedProcess.Name -ne 'dotnet.exe' -or $recordedProcess.CommandLine -notmatch $expectedArgument) {
        throw 'The recorded PID belongs to another process. It was not stopped.'
    }
    Stop-Process -Id $recordedWebId -ErrorAction Stop
    Wait-Process -Id $recordedWebId -Timeout 10 -ErrorAction SilentlyContinue
    Write-Output 'Stopped the script web instance so Visual Studio can use port 5088.'
}
Remove-Item -LiteralPath $pidPath
