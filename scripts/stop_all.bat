@echo off
chcp 65001 >nul
echo ===================================================
echo       Зупинка всіх процесів Dteam...
echo ===================================================

echo [1/3] Зупинка порт 5117 (Backend HTTP)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5117" ^| findstr "LISTENING"') do (
    taskkill /F /PID %%a 2>nul
)

echo [2/3] Зупинка порт 7264 (Backend HTTPS)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":7264" ^| findstr "LISTENING"') do (
    taskkill /F /PID %%a 2>nul
)

echo [3/3] Зупинка порт 5173 (Frontend Vite)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5173" ^| findstr "LISTENING"') do (
    taskkill /F /PID %%a 2>nul
)

echo.
echo ===================================================
echo  Всі сервіси Dteam зупинено!
echo ===================================================
timeout /t 2 >nul
