@echo off
chcp 65001 >nul
echo ===================================================
echo       Запуск Dteam Backend (.NET 9)
echo ===================================================

set BACKEND_DIR=%~dp0..\DteamBackend\DteamBackend
cd /d "%BACKEND_DIR%"
dotnet run
