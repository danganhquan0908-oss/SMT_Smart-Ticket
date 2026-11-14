param([int]$Port = 5088, [switch]$DatabaseOnly, [switch]$PrepareForVisualStudio)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if ($PrepareForVisualStudio) { & (Join-Path $PSScriptRoot 'stop-local-web.ps1') }
$localDir = Join-Path $projectRoot '.local'
New-Item -ItemType Directory -Force -Path $localDir | Out-Null
$pgBin = Join-Path $localDir 'pgsql\bin'
if (-not (Test-Path (Join-Path $pgBin 'pg_ctl.exe'))) {
    $archive = Join-Path $localDir 'postgresql.zip'
    Invoke-WebRequest 'https://get.enterprisedb.com/postgresql/postgresql-18.7-1-windows-x64-binaries.zip' -OutFile $archive
    if ((Get-FileHash $archive -Algorithm SHA256).Hash -ne 'ead0bbc27fb678fa24dcf86b738981323042abd073c744103bf7f3aed23f9a33') { throw 'PostgreSQL checksum mismatch.' }
    tar -xf $archive -C $localDir pgsql/bin pgsql/lib pgsql/share
    if ($LASTEXITCODE) { throw 'PostgreSQL extraction failed.' }
}
$configPath = Join-Path $localDir 'development.json'
if (-not (Test-Path $configPath)) {
    function New-Key {
        $bytes = New-Object byte[] 32
        $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try { $rng.GetBytes($bytes); return -join ($bytes | ForEach-Object { $_.ToString('x2') }) }
        finally { $rng.Dispose() }
    }
    @{ DatabasePassword=(New-Key); TicketKey=(New-Key); DispatchKey=(New-Key); AdminPassword=('Admin!' + (New-Key).Substring(0,20) + 'a') } | ConvertTo-Json | Set-Content $configPath -Encoding utf8
}
$devConfig = Get-Content $configPath -Raw | ConvertFrom-Json
$dataDir = Join-Path $localDir 'pgdata'
if (-not (Test-Path (Join-Path $dataDir 'PG_VERSION'))) {
    $pwFile = Join-Path $localDir 'init-password.tmp'
    try {
        Set-Content $pwFile $devConfig.DatabasePassword -Encoding ascii
        & (Join-Path $pgBin 'initdb.exe') -D $dataDir -U postgres --encoding=UTF8 --locale=C --auth=scram-sha-256 --pwfile=$pwFile | Out-Null
        if ($LASTEXITCODE) { throw 'PostgreSQL initdb failed.' }
    } finally { Remove-Item -LiteralPath $pwFile -ErrorAction SilentlyContinue }
}
& (Join-Path $pgBin 'pg_ctl.exe') status -D $dataDir *> $null
if ($LASTEXITCODE) {
    $pgArgs = '-D "' + $dataDir + '" -l "' + (Join-Path $localDir 'postgres.log') + '" -o "-p 55432 -h 127.0.0.1" -w start'
    $pgProcess = Start-Process -FilePath (Join-Path $pgBin 'pg_ctl.exe') -ArgumentList $pgArgs -PassThru -WindowStyle Hidden
    $pgProcess.WaitForExit()
    if ($pgProcess.ExitCode) { throw 'PostgreSQL failed to start. See .local/postgres.log.' }
}
$previousPgPassword = $env:PGPASSWORD
try {
    $env:PGPASSWORD = $devConfig.DatabasePassword
    $exists = & (Join-Path $pgBin 'psql.exe') -h 127.0.0.1 -p 55432 -U postgres -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='busgo_smartticket'"
    if ($LASTEXITCODE) { throw 'Could not connect to the local database.' }
    if ($exists -ne '1') {
        & (Join-Path $pgBin 'createdb.exe') -h 127.0.0.1 -p 55432 -U postgres busgo_smartticket
        if ($LASTEXITCODE) { throw 'Could not create the isolated development database.' }
    }
} finally { $env:PGPASSWORD = $previousPgPassword }
# Local settings allow Visual Studio F5 to use the same isolated development database.
$localSettings = @{
    ConnectionStrings = @{ DefaultConnection = ('Host=127.0.0.1;Port=55432;Database=busgo_smartticket;Username=postgres;Password=' + $devConfig.DatabasePassword); Redis = '' }
    Tickets = @{ SigningKey = $devConfig.TicketKey }
    Dispatch = @{ SigningKey = $devConfig.DispatchKey }
    Admin = @{ Email = 'admin@busgo.local'; Password = $devConfig.AdminPassword }
}
$localSettings | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $projectRoot 'appsettings.Local.json') -Encoding utf8
if ($DatabaseOnly) { Write-Output 'Development database is ready. Open BusGoSmartTicket.sln and press F5.'; exit 0 }
$url = 'http://127.0.0.1:' + $Port
$pidPath = Join-Path $localDir 'web.pid'
if (Test-Path $pidPath) {
    $existingProcessId = [int](Get-Content $pidPath)
    $existing = Get-CimInstance Win32_Process -Filter "ProcessId=$existingProcessId" -ErrorAction SilentlyContinue
    if ($existing -and $existing.CommandLine.Contains($projectRoot)) { Write-Output ('Already running: ' + $url); exit 0 }
}
dotnet build (Join-Path $projectRoot 'BusGoSmartTicket.csproj') --nologo --verbosity quiet
if ($LASTEXITCODE) { throw 'Build failed.' }
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = $url
$env:ConnectionStrings__DefaultConnection = 'Host=127.0.0.1;Port=55432;Database=busgo_smartticket;Username=postgres;Password=' + $devConfig.DatabasePassword
$env:ConnectionStrings__Redis = ''
$env:Tickets__SigningKey = $devConfig.TicketKey
$env:Dispatch__SigningKey = $devConfig.DispatchKey
$env:Admin__Email = 'admin@busgo.local'
$env:Admin__Password = $devConfig.AdminPassword
$webDll = Join-Path $projectRoot 'bin\Debug\net10.0\BusGoSmartTicket.dll'
$webDir = $projectRoot
$process = Start-Process -FilePath 'dotnet' -ArgumentList ('"' + $webDll + '"') -WorkingDirectory $webDir -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $localDir 'web.log') -RedirectStandardError (Join-Path $localDir 'web-error.log')
$process.Id | Set-Content $pidPath
for($attempt=0;$attempt -lt 60;$attempt++) {
    if($process.HasExited){throw 'Web application exited. See .local/web.log and web-error.log.'}
    try { $health=Invoke-RestMethod ($url+'/health'); if($health.status -eq 'ready'){Write-Output ('Ready: '+$url); exit 0} } catch { }
    Start-Sleep -Seconds 1
}
throw 'Application did not become ready in 60 seconds. See .local/web.log.'
