@echo off
echo ========================================================
echo   Starting Dteam Backend + PostgreSQL in Docker...
echo ========================================================

docker compose -f docker-compose.backend.yml up --build -d

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================================
    echo   Backend & PostgreSQL started successfully!
    echo   - Backend URL:   http://localhost:5117
    echo   - Swagger UI:    http://localhost:5117/swagger
    echo   - Health Check:  http://localhost:5117/api/check-status
    echo   - PostgreSQL:    localhost:5432 (db: dteam_db)
    echo ========================================================
) else (
    echo.
    echo [ERROR] Failed to start containers. Make sure Docker Desktop is running.
)
pause
