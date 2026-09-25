@echo off
title Dteam Backend
set "BACKEND_DIR=%~dp0..\DteamBackend\DteamBackend"
cd /d "%BACKEND_DIR%"
echo ===================================================
echo   Starting Dteam Backend (.NET 10 API on 5117)
echo ===================================================
dotnet run
pause
