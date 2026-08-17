@echo off
setlocal

set BASEURL=http://localhost:5113
set COUNT=80

if not "%~1"=="" set BASEURL=%~1
if not "%~2"=="" set COUNT=%~2

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0BurstTest.ps1" -BaseUrl "%BASEURL%" -RequestCount %COUNT%

pause
