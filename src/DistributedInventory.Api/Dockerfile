# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia csproj e restaura dependências
COPY DistributedInventory.Api/*.csproj ./DistributedInventory.Api/
COPY ../DistributedInventory.Core/*.csproj ./DistributedInventory.Core/
COPY ../DistributedInventory.Infrastructure/*.csproj ./DistributedInventory.Infrastructure/
RUN dotnet restore ./DistributedInventory.Api/DistributedInventory.Api.csproj

# Copia tudo e publica (Release)
COPY DistributedInventory.Api/ ./DistributedInventory.Api/
COPY ../DistributedInventory.Core/ ./DistributedInventory.Core/
COPY ../DistributedInventory.Infrastructure/ ./DistributedInventory.Infrastructure/
RUN dotnet publish ./DistributedInventory.Api/DistributedInventory.Api.csproj -c Release -o /app/publish

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# diretório do SQLite compartilhado
ENV DATA_DIR=/app/data
RUN mkdir -p ${DATA_DIR}

# portas e urls
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# performance e globalization (imagem slim)
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "DistributedInventory.Api.dll"]