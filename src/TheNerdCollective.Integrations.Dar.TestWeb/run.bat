@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "PORT=5095"

echo Killing process on port %PORT%...
for /f "tokens=5" %%P in ('netstat -aon ^| findstr /R /C:":%PORT% .*LISTENING"') do (
    if not "%%P"=="0" (
        echo   Stopping PID %%P
        taskkill /F /PID %%P >nul 2>&1
    )
)

ping -n 2 127.0.0.1 >nul

echo Starting TestWeb on http://localhost:%PORT% ...
dotnet run --launch-profile http

endlocal
