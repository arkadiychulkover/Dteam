@echo off
chcp 65001 >nul
echo ===================================================
echo   Запуск Dteam (Hardhat + Backend + Frontend)
echo ===================================================

set ROOT_DIR=%~dp0..
set BLOCKCHAIN_DIR=%ROOT_DIR%\blockchain
set BACKEND_DIR=%ROOT_DIR%\DteamBackend\DteamBackend
set FRONTEND_DIR=%ROOT_DIR%\DteamFrontend\dteam-app

echo [1/3] Запуск Hardhat Network (RPC на http://127.0.0.1:8545)...
start "Hardhat Network (RPC)" cmd /k "cd /d "%BLOCKCHAIN_DIR%" && npx hardhat node"

echo [2/3] Запуск .NET Backend (API на http://localhost:5117)...
start "Dteam Backend (API)" cmd /k "cd /d "%BACKEND_DIR%" && dotnet run"

echo [3/3] Запуск Svelte Frontend (Vite на http://localhost:5173)...
start "Dteam Frontend (Vite)" cmd /k "cd /d "%FRONTEND_DIR%" && npm run dev"

echo.
echo ===================================================
echo  Сервіси успішно запущені в окремих вікнах!
echo  ⛓️ Hardhat:  http://127.0.0.1:8545
echo  ⚙️ Backend:  http://localhost:5117
echo  🌐 Frontend: http://localhost:5173
echo ===================================================
timeout /t 3 >nul
