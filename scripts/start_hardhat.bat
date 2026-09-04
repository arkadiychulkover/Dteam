@echo off
chcp 65001 >nul
echo ===================================================
echo       Запуск Hardhat Network (Node RPC)
echo ===================================================

set BLOCKCHAIN_DIR=%~dp0..\blockchain
cd /d "%BLOCKCHAIN_DIR%"
npx hardhat node
