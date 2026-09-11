FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_22.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends nodejs \
    && curl -sSL https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-amd64.tgz -o /tmp/ngrok.tgz \
    && tar -xzf /tmp/ngrok.tgz -C /usr/local/bin \
    && rm -f /tmp/ngrok.tgz \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 5117
EXPOSE 8080
EXPOSE 8545
ENV ASPNETCORE_URLS=http://+:5117

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["DteamBackend/DteamBackend.csproj", "DteamBackend/"]
RUN dotnet restore "DteamBackend/DteamBackend.csproj"

COPY . .
WORKDIR "/src/DteamBackend"
RUN dotnet build "DteamBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DteamBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/wwwroot/uploads /app/wwwroot/game_images /app/wwwroot/comunity /app/blockchain
COPY --from=publish /app/publish .
COPY blockchain /app/blockchain
RUN cd /app/blockchain && (npm ci || npm install)
ENTRYPOINT ["dotnet", "DteamBackend.dll"]
