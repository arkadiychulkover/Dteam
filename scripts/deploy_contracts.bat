@echo off
title Deploy and Sync Contracts
set "BLOCKCHAIN_DIR=%~dp0..\blockchain"
cd /d "%BLOCKCHAIN_DIR%"
echo ===================================================
echo   Deploying Contracts and Syncing Configurations
echo ===================================================
call npx hardhat run scripts/deployAll.js --network localhost
echo ===================================================
echo   Finished!
echo ===================================================
pause
