# db-sync-engine

`db-sync-engine` is a .NET worker service for ETL-style synchronization between databases. It reads data from a source table, normalizes values and schema for the target provider, creates or updates the target schema, and writes data in chunks.

The project is designed for recurring background sync jobs and stores sync progress separately in SQLite so runs can resume from the last processed key.

## What it does

- syncs data between supported databases
- processes records in chunks
- tracks sync state in SQLite
- recreates or prepares target schema from source schema
- normalizes schema/types for the target database
- normalizes row values before writing
- removes duplicate rows inside a chunk by key before insert
- retries failed sync runs with exponential backoff
- supports running as a local .NET worker or in Docker

## Supported databases

### Source/target providers
- MySQL
- PostgreSQL

### Internal state storage
- SQLite for sync process tracking

## How it works

For each configured entity, the worker runs a full sync pipeline:

1. load or create sync state
2. ensure target schema exists and matches normalized source schema
3. read a chunk from the source table ordered by key
4. normalize row values
5. deduplicate rows inside the chunk by key
6. write the chunk to the target table
7. update sync progress in SQLite

The worker repeats this in a background loop and waits `IntervalSeconds` between entity runs.

## Main features

- **Full sync strategy**: current implemented sync mode is `Full`
- **Chunked reads/writes**: reads by key boundary and processes data in batches
- **Resume support**: stores `LastProcessedKey` and completion state in SQLite
- **Schema bootstrap**: reads source schema, normalizes it for target DB, then applies it to the target
- **Schema normalization options**:
  - preserve or lowercase names
  - rename columns
  - preserve default values
  - set default varchar length
  - optionally copy data during table replacement
- **Write optimization**:
  - PostgreSQL writes use binary `COPY`
  - MySQL writes use batched `INSERT`
- **Retry policy**: failed entity runs are retried using exponential backoff

## Project structure

- `src/DbSyncEngine.Worker` — host process and background loop
- `src/DbSyncEngine.Application` — sync strategies, pipeline, steps, options
- `src/DbSyncEngine.Infrastructure` — DB connections, repositories, schema handling, normalizers
- `src/DbSyncEngine.Domain` — sync process state model
- `config/config.json` — example runtime configuration
- `data/` — SQLite state database when running locally or in Docker

## Configuration

The worker reads configuration from the path stored in the `SYNC_CONFIG_PATH` environment variable.

Example:

```bash
SYNC_CONFIG_PATH=/absolute/path/to/config/config.json
```

### Configuration shape

```json
{
  "Sync": {
    "Entities": [
      {
        "Name": "buffers",
        "Target": {
          "Provider": "MySQL",
          "ConnectionString": "server=...;port=3306;database=...;User ID=...;password=...",
          "Schema": "orders2",
          "Table": "buffers"
        },
        "Source": {
          "Provider": "PostgreSQL",
          "ConnectionString": "Host=...;Database=...;Username=...;Password=...;",
          "Schema": "orders",
          "Table": "buffers",
          "Columns": [],
          "Key": "id",
          "KeyType": "long"
        },
        "ChunkSize": 10000,
        "InsertBatchSize": 1000,
        "MaxInsertRetries": 3,
        "IntervalSeconds": 2,
        "Direction": "Full"
      }
    ],
    "SyncProcessDb": "Data Source=/app/data/sync_engine.db"
  }
}
```

### Root settings

- `Sync.Entities` — list of sync jobs
- `Sync.SyncProcessDb` — SQLite connection string used to store sync progress

### Entity settings

- `Name` — logical entity/job name
- `Source` — source database connection and table settings
- `Target` — target database connection and table settings
- `ChunkSize` — number of rows to read/write per chunk
- `InsertBatchSize` — intended batch size setting in config
- `MaxInsertRetries` — retry count for failed entity runs
- `IntervalSeconds` — delay before the next run for this entity
- `Direction` — sync direction, currently `Full`
- `NormalizerOptions` — schema normalization options for target generation

### Source/Target endpoint settings

- `Provider` — `MySQL` or `PostgreSQL`
- `ConnectionString` — database connection string
- `Schema` — schema name when supported by the provider
- `Table` — table name
- `Columns` — optional column whitelist; empty means all columns
- `Key` — ordered key column used for incremental chunk boundaries
- `KeyType` — key type, supported values in code: `long`, `int`, `datetime`

## Running locally

### Requirements

- .NET SDK
- reachable source and target databases
- writable location for the SQLite sync state DB

### Build

```bash
dotnet restore src/db-sync-engine.sln
dotnet build src/db-sync-engine.sln
```

### Run

```bash
SYNC_CONFIG_PATH=$(pwd)/config/config.json dotnet run --project src/DbSyncEngine.Worker/DbSyncEngine.Worker.csproj
```

### Optional

```bash
dotnet watch --project src/DbSyncEngine.Worker/DbSyncEngine.Worker.csproj run
```

## Running with Docker

The repository already includes `Dockerfile` and `docker-compose.yml`.

### Docker Compose

```bash
docker compose up --build
```

Current compose setup:
- mounts `./config` to `/app/config` as read-only
- mounts `./data` to `/app/data`
- sets `SYNC_CONFIG_PATH=/app/config/config.json`

## Notes and behavior

- sync progress is stored independently from source and target DBs
- the worker processes entities sequentially inside the background loop
- when no more rows are returned, the sync process is marked as completed

## Known implementation details

- PostgreSQL target writes use binary `COPY` for speed
- MySQL target writes use multi-row `INSERT`
- duplicate rows inside a single chunk are reduced to the first row per key before writing
- unsupported providers throw `NotSupportedException`
- current implemented strategy is only `Full`

## Development

Useful commands:

```bash
dotnet restore src/db-sync-engine.sln
dotnet build src/db-sync-engine.sln
```
