# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DistributedInventory.sln ./
COPY src/DistributedInventory.Core/*.csproj ./src/DistributedInventory.Core/
COPY src/DistributedInventory.Infrastructure/*.csproj ./src/DistributedInventory.Infrastructure/
COPY src/DistributedInventory.Workers/*.csproj ./src/DistributedInventory.Workers/
RUN dotnet restore ./src/DistributedInventory.Workers/DistributedInventory.Workers.csproj

COPY src/DistributedInventory.Core/ ./src/DistributedInventory.Core/
COPY src/DistributedInventory.Infrastructure/ ./src/DistributedInventory.Infrastructure/
COPY src/DistributedInventory.Workers/ ./src/DistributedInventory.Workers/

RUN dotnet publish ./src/DistributedInventory.Workers/DistributedInventory.Workers.csproj -c Release -o /app/publish -v minimal

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
ENV DATA_DIR=/app/data
RUN mkdir -p ${DATA_DIR}
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "DistributedInventory.Workers.dll"]
