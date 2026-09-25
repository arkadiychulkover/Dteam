@echo off
title Hardhat Network
set "BLOCKCHAIN_DIR=%~dp0..\blockchain"
cd /d "%BLOCKCHAIN_DIR%"
echo ===================================================
echo   Starting Hardhat Network with Auto-Deploy & Sync
echo ===================================================
node scripts/startNodeAndDeploy.js
pause
