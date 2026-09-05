@echo off
title Hardhat Network
set "BLOCKCHAIN_DIR=%~dp0..\blockchain"
cd /d "%BLOCKCHAIN_DIR%"
echo ===================================================
echo   Starting Hardhat Network (RPC on 127.0.0.1:8545)
echo ===================================================
npx hardhat node
pause
