@echo off
echo Stopping Dteam Backend containers...
docker compose -f docker-compose.backend.yml down
echo Done!
pause
