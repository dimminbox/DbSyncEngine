# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Orders.DatabaseExchanger is a background service that synchronizes data between MySQL and PostgreSQL databases. It's part of the larger Orders solution which includes API services, AMQP workers, and other components.

## Common Commands

### Build and Run
```bash
# Build the project
dotnet build Orders.DatabaseExchanger.csproj

# Run the service
dotnet run --project Orders.DatabaseExchanger.csproj

# Build entire solution (from parent directory)
cd ..
dotnet build Orders.slnx
```

### Testing
```bash
# Run all tests in solution (from parent directory)
cd ..
dotnet test

# Run tests with coverage report
./coverage.sh
```

## Configuration

The service is configured via `appsettings.json` or Consul KV store (when `ConsulConnectionString` environment variable is set).

Key configuration sections:
- **Sync.Direction**: Controls which sync strategy runs (`FullReloadMySqlToPostgres`, `IncrementalMySqlToPostgres`, `PostgresToMySql`)
- **Sync.Interval**: Time between sync cycles (e.g., `"00.00:00:02"` for 2 seconds)
- **Sync.MaxInsertRetries**: Number of retry attempts with exponential backoff
- **FullReload.ChunkSize**: Number of records to read per chunk
- **FullReload.InsertBatchSize**: Batch size for inserts

## Architecture

### Pipeline Pattern
The core architecture uses a pipeline of steps that process data sequentially:

1. **SyncPipeline** orchestrates execution of **ISyncStep** implementations
2. Each step receives a **SyncContext** (shared state) and a `next()` continuation
3. Steps can modify context and call `next()` to continue the pipeline
4. Context flows through: PrepareReloadStep → ReadDataStep → Transform/Map → Write → UpdateState

### Strategy Pattern
**ISyncStrategyFactory** creates different sync strategies based on **SyncDirection** enum:

- **FullReloadMySqlToPostgresSyncStrategy**: Reads MySQL in chunks, maps entities, writes to PostgreSQL
  - Pipeline: PrepareReloadStep → ReadDataStep<Product> → MapChunkStep → WriteChunkToPostgresStep → UpdateFullReloadStateStep

- **IncrimentalMySqlToPostgresSyncStrategy**: Compares aggregates and syncs differences
  - Pipeline: PrepareReloadStep → ReadDataStep<Order> → EnrichMySqlAggregatesStep → EnrichPostgresAggregatesStep → CompareAggregatesStep → TransformMySqlToPostgresStep → PersistToPostgresStep → UpdateIncrimentalReloadStateStep

### Provider Pattern
**IMySqlProvider** and **IPostgresProvider** wrap separate DI containers with database-specific configurations:
- Each provider has its own ServiceProvider with appropriate connection strings and repositories
- This isolation allows different Dapper/MicroOrm configurations per database engine

### State Management
**ReloadState** entity tracks sync progress in PostgreSQL:
- `LastProcessedKey`: Resume point for chunked reads (supports Id, DateUpdate, etc.)
- `IsCompleted`: Whether full reload finished
- `TotalProcessedRows`: Progress counter
- Allows restartable syncs that resume from last checkpoint

### Entity Configuration
**ReloadEntityConfig<T>** defines how to sync each entity type:
- `KeySelector`: Property to use for chunking (e.g., `o => o.DateUpdate`)
- `Comparison`: How to compare keys for incremental sync
- `ParseKey`: Convert string back to typed key for resume
- `InitialFilter`: Starting filter when no checkpoint exists

## Key Relationships

- **SyncBackgroundService** (hosted service) → **ISyncStrategyFactory** → **ISyncStrategy** → **SyncPipeline** → **ISyncStep[]**
- **Startup.ConfigureServices** registers two separate DI containers (MySQL and Postgres) wrapped by providers
- **ReloadEntityConfig** instances are registered per entity type (Order, Product) to configure sync behavior
- **IEntityMapper** implementations (OrderToOrderPgMapper, ProductToProductPgMapper) handle schema differences between databases

## Development Notes

- The namespace in code is `DbSyncEngine` but the project is named `Orders.DatabaseExchanger`
- Uses Polly for retry logic with exponential backoff
- MicroOrm.Dapper.Repositories is used for data access with custom SQL generators
- The service runs continuously in a loop with configurable intervals between sync cycles
