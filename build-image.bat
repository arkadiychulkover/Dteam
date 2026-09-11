@echo off
echo ========================================================
echo   Building Docker Image for Dteam Backend...
echo ========================================================

docker build -t dteam-backend:latest -f DteamBackend/Dockerfile DteamBackend

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================================
    echo   Image successfully built!
    echo   Local image name: dteam-backend:latest
    echo.
    echo   To push to Docker Hub:
    echo     1. docker tag dteam-backend:latest YOUR_DOCKERHUB_USERNAME/dteam-backend:latest
    echo     2. docker push YOUR_DOCKERHUB_USERNAME/dteam-backend:latest
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed. Make sure Docker Desktop is launched and running.
)
pause
