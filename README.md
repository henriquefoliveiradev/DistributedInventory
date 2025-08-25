# DistributedInventory – Teste mercado livre


Sistema distribuído de estoque com .NET 8 + SQLite.


## Como rodar
```bash
cd src/DistributedInventory.Api
DOTNET_ENVIRONMENT=Development dotnet run
```


Banco SQLite é criado em `./data/inventory.db` automaticamente.


## Endpoints


- `GET /v1/stores/{storeId}/stock/{sku}`
- `POST /v1/stock/reserve` → body `{ sku, storeId, qty, clientId, ttlSeconds }`


## Decisões


- **CQRS** simplificado.
- **Controle otimista** via campo `Version`.
- **Outbox pattern** com worker de publicação.


## Testes


```bash
cd tests/DistributedInventory.Tests
dotnet test
```