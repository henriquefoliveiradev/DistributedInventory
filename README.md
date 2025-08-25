# DistributedInventory – Teste mercado livre


Sistema distribuído de estoque com .NET 8 + SQLite.


## Como rodar
```bash
cd src/DistributedInventory.Api
dotnet run

# em outro terminal
cd src/DistributedInventory.Workers
dotnet run
```


Banco SQLite é criado em `./data/inventory.db` automaticamente no start do Api, por isso ele deve ser iniciado primeiro.


## Endpoints


- `GET /v1/stores/{storeId}/stock/{sku}`
- `POST /v1/stock/reserve` → body `{ sku, storeId, qty, clientId, ttlSeconds }`
- `POST /v1/stock/confirm/{reservationId}`
- `POST /v1/stock/cancel/{reservationId}`
- `POST /v1/stock/restock` → body `{ sku, storeId, qty, expectedVersion }`

## Testes


```bash
cd tests/DistributedInventory.Tests
dotnet test
```

## Arquivos
Link repo github com a implementação mais completa: https://github.com/henriquefoliveiradev/DistributedInventory.git

Link excalidraw com desenho do design do projeto: https://excalidraw.com/#json=jGUZLoW24YJIu3aq4c6dd,VRlXfdsE4FehgORMlaznuQ