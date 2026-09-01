@echo off
chcp 65001 >nul
echo ===================================================
echo       Запуск Dteam (Backend + Frontend)
echo ===================================================

set ROOT_DIR=%~dp0..
set BACKEND_DIR=%ROOT_DIR%\DteamBackend\DteamBackend
set FRONTEND_DIR=%ROOT_DIR%\DteamFrontend\dteam-app

echo [1/2] Запуск .NET Backend (API на http://localhost:5117)...
start "Dteam Backend (API)" cmd /k "cd /d "%BACKEND_DIR%" && dotnet run"

echo [2/2] Запуск Svelte Frontend (Vite на http://localhost:5173)...
start "Dteam Frontend (Vite)" cmd /k "cd /d "%FRONTEND_DIR%" && npm run dev"

echo.
echo ===================================================
echo  Сервіси успішно запущені в окремих вікнах!
echo  🌐 Frontend: http://localhost:5173
echo  ⚙️  Backend:  http://localhost:5117
echo ===================================================
timeout /t 3 >nul
