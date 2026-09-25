@echo off
echo ========================================================
echo   Building and Pushing Dteam Backend to Docker Hub...
echo   Target: arkadii228555/dteam-backend:latest
echo ========================================================

docker build -t arkadii228555/dteam-backend:latest -f DteamBackend/Dockerfile DteamBackend
if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Docker build failed. Make sure Docker Desktop is launched and running!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================================
echo   Pushing image to Docker Hub...
echo ========================================================
docker push arkadii228555/dteam-backend:latest

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================================
    echo   SUCCESS! Image pushed to arkadii228555/dteam-backend:latest
    echo ========================================================
) else (
    echo.
    echo [ERROR] Push failed. Check your network or run 'docker login'.
)
pause
