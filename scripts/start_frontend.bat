@echo off
title Dteam Frontend
set "FRONTEND_DIR=%~dp0..\DteamFrontend\dteam-app"
cd /d "%FRONTEND_DIR%"
echo ===================================================
echo   Starting Dteam Frontend (Vite / Svelte on 5173)
echo ===================================================
npm run dev
pause
