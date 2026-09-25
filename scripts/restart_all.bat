@echo off
chcp 65001 >nul
echo ===================================================
echo       Перезапуск сервісів Dteam...
echo ===================================================

call "%~dp0stop_all.bat"
timeout /t 1 >nul
call "%~dp0start_all.bat"
