# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) solution + csprojs (cache de restore)
COPY DistributedInventory.sln ./
COPY src/DistributedInventory.Core/*.csproj ./src/DistributedInventory.Core/
COPY src/DistributedInventory.Infrastructure/*.csproj ./src/DistributedInventory.Infrastructure/
COPY src/DistributedInventory.Api/*.csproj ./src/DistributedInventory.Api/
RUN dotnet restore ./src/DistributedInventory.Api/DistributedInventory.Api.csproj

# 2) copiar SOMENTE os diretórios que a API usa
COPY src/DistributedInventory.Core/ ./src/DistributedInventory.Core/
COPY src/DistributedInventory.Infrastructure/ ./src/DistributedInventory.Infrastructure/
COPY src/DistributedInventory.Api/ ./src/DistributedInventory.Api/

# 3) build/publish
RUN dotnet publish ./src/DistributedInventory.Api/DistributedInventory.Api.csproj -c Release -o /app/publish -v minimal

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV DATA_DIR=/app/data
RUN mkdir -p ${DATA_DIR}
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "DistributedInventory.Api.dll"]
