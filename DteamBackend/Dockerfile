FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 5117
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
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DteamBackend.dll"]
