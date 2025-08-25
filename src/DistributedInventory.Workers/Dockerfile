FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DistributedInventory.Workers/DistributedInventory.Workers.csproj", "DistributedInventory.Workers/"]
RUN dotnet restore "DistributedInventory.Workers/DistributedInventory.Workers.csproj"
COPY . .
WORKDIR "/src/DistributedInventory.Workers"
RUN dotnet build "./DistributedInventory.Workers.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DistributedInventory.Workers.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DistributedInventory.Workers.dll"]
