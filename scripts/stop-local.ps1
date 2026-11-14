$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'stop-local-web.ps1')
$pg=Join-Path $projectRoot '.local\pgsql\bin\pg_ctl.exe'
if(Test-Path $pg){ & $pg -D (Join-Path $projectRoot '.local\pgdata') -m fast -w stop }
