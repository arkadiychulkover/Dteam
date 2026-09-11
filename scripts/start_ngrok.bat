@echo off
title Ngrok Hardhat Tunnel
echo ===================================================
echo   Starting Ngrok Tunnel for Hardhat (Port 8545)
echo ===================================================
ngrok http --url=goldmine-unloved-capsule.ngrok-free.dev 8545
pause
