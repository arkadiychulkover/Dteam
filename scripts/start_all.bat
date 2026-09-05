@echo off
echo ===================================================
echo   Starting Dteam: Hardhat, Backend, Frontend
echo ===================================================

echo [1/3] Starting Hardhat Network...
start "Hardhat Network" "%~dp0start_hardhat.bat"
ping 127.0.0.1 -n 3 >nul

echo [2/3] Starting Dteam Backend...
start "Dteam Backend" "%~dp0start_backend.bat"
ping 127.0.0.1 -n 3 >nul

echo [3/3] Starting Dteam Frontend...
start "Dteam Frontend" "%~dp0start_frontend.bat"

echo.
echo ===================================================
echo  All services launched in separate windows!
echo  Hardhat:  http://127.0.0.1:8545
echo  Backend:  http://localhost:5117
echo  Frontend: http://localhost:5173
echo ===================================================
ping 127.0.0.1 -n 4 >nul
