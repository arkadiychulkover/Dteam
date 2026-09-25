@echo off
chcp 65001 >nul
echo ===================================================
echo       Перевірка та компіляція Dteam
echo ===================================================

set ROOT_DIR=%~dp0..
set BACKEND_DIR=%ROOT_DIR%\DteamBackend\DteamBackend
set FRONTEND_DIR=%ROOT_DIR%\DteamFrontend\dteam-app

echo [1/2] Компіляція .NET Backend...
cd /d "%BACKEND_DIR%"
dotnet build
if %ERRORLEVEL% neq 0 (
    echo [ПОМИЛКА] Помилка компіляції Backend!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [2/2] Компіляція Svelte Frontend...
cd /d "%FRONTEND_DIR%"
call npm run build
if %ERRORLEVEL% neq 0 (
    echo [ПОМИЛКА] Помилка збірки Frontend!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ===================================================
echo  [УСПІХ] Всі проєкти зібрані без помилок (0 errors)!
echo ===================================================
pause
