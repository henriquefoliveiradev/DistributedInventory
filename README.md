# DistributedInventory – Teste mercado livre

## 📖 Resumo do Projeto

O **DistributedInventory** é um protótipo de **sistema distribuído de gerenciamento de estoque**, desenvolvido em **.NET 8** com **SQLite** como persistência.  
Ele foi projetado para atacar problemas de **latência e inconsistência** comuns em sistemas de estoque quando há múltiplas operações concorrentes e integração com serviços externos.

### 🔑 Objetivos
- Garantir **consistência forte** nas operações críticas (reserva, confirmação, cancelamento, reposição).  
- Reduzir **latência** de sincronização entre múltiplas lojas e serviços consumidores.  
- Evitar problemas de **concorrência** como estoque negativo ou sobrescrita silenciosa.  
- Permitir **propagação assíncrona e confiável** de eventos para outros sistemas (ex.: billing, logística).

### 🛠️ Principais decisões técnicas
- **Minimal APIs (.NET 8)** → endpoints leves e objetivos.  
- **Controle otimista de concorrência** → campo `Version` marca estado e impede overwrite.  
- **Outbox Pattern** → eventos de integração só são publicados após persistência bem-sucedida.  
- **Worker dedicado** → consome a `Outbox` e publica os eventos, desacoplando escrita de publicação.  

### 🚀 Fluxo principal
1. **Consulta de estoque** → leitura direta (`GET /stock`).  
2. **Reserva** → reduz disponibilidade e insere evento `ReservationCreated` na outbox.  
3. **Confirmação/Cancelamento** → atualiza estoque e gera novos eventos.  
4. **Reposição** → incrementa estoque com verificação de versão (`expectedVersion`) para evitar concorrência perdida.  
5. **Worker** → lê eventos pendentes na `Outbox`, publica (por enquanto estou logando) e marca como processados.  

### 📦 Benefícios
- **Menor latência** → propagação assíncrona quase em tempo real, sem batch.  
- **Consistência garantida** → nenhum update silencioso, sempre há detecção de concorrência.  
- **Escalabilidade** → fácil adicionar novos consumidores de eventos (ex.: projeções, notificações).  
- **Rastreabilidade** → cada mudança no estoque gera um evento armazenado na outbox.  


Banco SQLite é criado em `./data/inventory.db` automaticamente no start do Api, por isso ele deve ser iniciado primeiro.

## Endpoints


- `GET /v1/stores/{storeId}/stock/{sku}`
- `POST /v1/stock/reserve` → body `{ sku, storeId, qty, clientId, ttlSeconds }`
- `POST /v1/stock/confirm/{reservationId}`
- `POST /v1/stock/cancel/{reservationId}`
- `POST /v1/stock/restock` → body `{ sku, storeId, qty, expectedVersion }`


## Subir API + Worker

Você precisa estar na **pasta raiz do projeto** (onde está o arquivo
`docker-compose.yml`).

``` bash
# 1) build das imagens + subir containers
docker compose up --build

# 2) logs em tempo real (se estiver rodando em background)
# docker compose up -d
# docker compose logs -f
```

A **API** ficará disponível em:\
**http://localhost:5000**

Você pode ver o swagger em:\
**http://localhost:5000/swagger**


### Endpoints (com exemplos rápidos)

``` bash
# estoque
curl http://localhost:5000/v1/stores/S1/stock/SKU-001

# reservar
curl -X POST http://localhost:5000/v1/stock/reserve   -H "Content-Type: application/json"   -d '{ "sku":"SKU-001", "storeId":"S1", "qty":2, "clientId":"C1", "ttlSeconds":60 }'

# confirmar reserva
curl -X POST http://localhost:5000/v1/stock/confirm   -H "Content-Type: application/json"   -d '{ "reservationId": "UTILIZAR GUID RETORNADO NA RESERVE" }'

# cancelar reserva
curl -X POST http://localhost:5000/v1/stock/cancel   -H "Content-Type: application/json"   -d '{ "reservationId": "UTILIZAR GUID RETORNADO NA RESERVE" }'

# repor estoque
curl -X POST http://localhost:5000/v1/stock/restock   -H "Content-Type: application/json"   -d '{ "sku":"SKU-001", "storeId":"S1", "qty":5, "expectedVersion":1 }'
```

------------------------------------------------------------------------

## Como rodar utilizando o .net sdk, caso queira debugar pela ide e etc..
```bash
cd src/DistributedInventory.Api
dotnet run

# em outro terminal
cd src/DistributedInventory.Workers
dotnet run
```
## Testes


```bash
cd tests/DistributedInventory.Tests
dotnet test
```

## Arquivos
Link repo github com a implementação mais completa: https://github.com/henriquefoliveiradev/DistributedInventory.git

Link excalidraw com desenho do design do projeto: [https://excalidraw.com/#json=jGUZLoW24YJIu3aq4c6dd,VRlXfdsE4FehgORMlaznuQ](https://excalidraw.com/#json=73p-LKitSGYSMJ5j_v55d,tZEjvwYu1_7BQaIT7eF8kA)
