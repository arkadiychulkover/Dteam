@echo off
setlocal
cd /d "%~dp0.."

echo ===================================================
echo [DTEAM] Starting full stack in Docker containers...
echo ===================================================

docker compose up -d --build

if errorlevel 1 (
    echo.
    echo [ERROR] Failed to start Dteam stack in Docker.
    echo Please ensure Docker Desktop is running.
    exit /b 1
)

echo.
echo [OK] Dteam stack started successfully!
echo [INFO] Frontend: http://localhost:5173
echo [INFO] Backend:  http://localhost:5117
echo.
docker compose ps
