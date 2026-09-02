@echo off
chcp 65001 >nul
echo ===================================================
echo       Запуск Dteam Frontend (Vite / Svelte 5)
echo ===================================================

set FRONTEND_DIR=%~dp0..\DteamFrontend\dteam-app
cd /d "%FRONTEND_DIR%"
npm run dev
