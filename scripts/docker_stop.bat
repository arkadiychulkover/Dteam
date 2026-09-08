@echo off
setlocal
cd /d "%~dp0.."

echo ===================================================
echo [DTEAM] Stopping Docker containers...
echo ===================================================

docker compose down

if errorlevel 1 (
    echo.
    echo [ERROR] Failed to stop containers.
    exit /b 1
)

echo.
echo [OK] Containers stopped successfully (Volumes preserved).
