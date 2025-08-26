# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DistributedInventory.Workers/*.csproj ./DistributedInventory.Workers/
COPY ../DistributedInventory.Core/*.csproj ./DistributedInventory.Core/
COPY ../DistributedInventory.Infrastructure/*.csproj ./DistributedInventory.Infrastructure/
RUN dotnet restore ./DistributedInventory.Workers/DistributedInventory.Workers.csproj

COPY DistributedInventory.Workers/ ./DistributedInventory.Workers/
COPY ../DistributedInventory.Core/ ./DistributedInventory.Core/
COPY ../DistributedInventory.Infrastructure/ ./DistributedInventory.Infrastructure/
RUN dotnet publish ./DistributedInventory.Workers/DistributedInventory.Workers.csproj -c Release -o /app/publish

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# diretório do SQLite compartilhado
ENV DATA_DIR=/app/data
RUN mkdir -p ${DATA_DIR}

ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "DistributedInventory.Workers.dll"]