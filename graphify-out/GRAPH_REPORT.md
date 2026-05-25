# Graph Report - .  (2026-05-25)

## Corpus Check
- 38 files · ~0 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 202 nodes · 342 edges · 37 communities (34 shown, 3 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 28 edges (avg confidence: 0.9)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Dapper  DB Connectivity|Dapper / DB Connectivity]]
- [[_COMMUNITY_Schema Sync Pipeline Steps|Schema Sync Pipeline Steps]]
- [[_COMMUNITY_Schema Normalizer Abstractions|Schema Normalizer Abstractions]]
- [[_COMMUNITY_DDL Generator Logic|DDL Generator Logic]]
- [[_COMMUNITY_Schema Reader & Bootstrapper|Schema Reader & Bootstrapper]]
- [[_COMMUNITY_Table Data Repository|Table Data Repository]]
- [[_COMMUNITY_Sync Strategy & Pipeline|Sync Strategy & Pipeline]]
- [[_COMMUNITY_Background Worker Service|Background Worker Service]]
- [[_COMMUNITY_RowData & Row Access|RowData & Row Access]]
- [[_COMMUNITY_Graphify Knowledge Graph|Graphify Knowledge Graph]]
- [[_COMMUNITY_Domain Exceptions|Domain Exceptions]]
- [[_COMMUNITY_Application Entry Point|Application Entry Point]]
- [[_COMMUNITY_Pipeline Assembler|Pipeline Assembler]]

## God Nodes (most connected - your core abstractions)
1. `MySqlDdlGenerator` - 18 edges
2. `PostgresDdlGenerator` - 17 edges
3. `SyncProcess` - 14 edges
4. `ILogger` - 13 edges
5. `PostgresTableDataRepository` - 11 edges
6. `RowData` - 10 edges
7. `PostgresSchemaNormalizer` - 10 edges
8. `SyncContext` - 9 edges
9. `EnsureTargetSchemaStep` - 9 edges
10. `MySqlSchemaNormalizer` - 9 edges

## Surprising Connections (you probably didn't know these)
- `Full Sync Pipeline` --rationale_for--> `FullSyncStrategy`  [INFERRED]
  README.md → src/DbSyncEngine.Application/Strategies/Implementations/FullSyncStrategy.cs
- `SQLite State Tracking` --rationale_for--> `SyncProcess`  [INFERRED]
  README.md → DbSyncEngine.Application/Pipelines/Common/SyncContext.cs
- `Chunked Processing Strategy` --rationale_for--> `ReadDataStep`  [INFERRED]
  README.md → src/DbSyncEngine.Application/Pipelines/Steps/FullSyncSteps/ReadDataStep.cs
- `Chunked Processing Strategy` --rationale_for--> `WriteDataStep`  [INFERRED]
  README.md → src/DbSyncEngine.Application/Pipelines/Steps/FullSyncSteps/WriteDataStep.cs
- `SyncEntityConfig` --shares_data_with--> `DbEndpoint`  [EXTRACTED]
  DbSyncEngine.Application/Pipelines/Common/SyncPipeline.cs → src/DbSyncEngine.Application/Strategies/Options/DbEndpoint.cs

## Communities (37 total, 3 thin omitted)

### Community 0 - "Dapper / DB Connectivity"
Cohesion: 0.08
Nodes (11): DapperRepository, Docker Compose Config, IDbConnection, ISyncProcessRepository, db-sync-engine, Full Sync Pipeline, SQLite State Tracking, string (+3 more)

### Community 1 - "Schema Sync Pipeline Steps"
Cohesion: 0.22
Nodes (14): EnsureSchemaOptions, EnsureTargetSchemaStep, SyncSequencesStep, GetSyncStep, ILogger, ISequenceSynchronizer, ISyncStep, MapChunkStep (+6 more)

### Community 2 - "Schema Normalizer Abstractions"
Cohesion: 0.19
Nodes (12): Application DependencyInjection, Dictionary, IDictionary, ISchemaNormalizer, ISchemaNormalizerFactory, IValueNormalizer, MySqlSchemaNormalizer, MySqlValueNormalizer (+4 more)

### Community 3 - "DDL Generator Logic"
Cohesion: 0.35
Nodes (3): ITargetDdlGenerator, MySqlDdlGenerator, PostgresDdlGenerator

### Community 4 - "Schema Reader & Bootstrapper"
Cohesion: 0.18
Nodes (9): IDbConnectionFactory, ISchemaBootstrapper, ISchemaReader, ISchemaReaderFactory, ISyncProcessRepositoryFactory, MySqlSchemaReader, PostgresSchemaReader, ColumnDefinition (+1 more)

### Community 5 - "Table Data Repository"
Cohesion: 0.23
Nodes (8): ILoggerFactory, ITableDataRepository, ITableDataRepositoryFactory, MySqlConnection, MySqlTableDataRepository, NpgsqlConnection, PostgresTableDataRepository, Regex

### Community 6 - "Sync Strategy & Pipeline"
Cohesion: 0.26
Nodes (11): DbEndpoint, FullSyncStrategy, IReadOnlyList, IServiceProvider, IServiceScopeFactory, ISyncPipeline, ISyncStrategy, NormalizerOptions (+3 more)

### Community 8 - "Background Worker Service"
Cohesion: 0.28
Nodes (6): BackgroundService, Worker DependencyInjection, IOptionsMonitor, IReadOnlyDictionary, Worker Program Entry Point, SyncBackgroundService

### Community 10 - "Graphify Knowledge Graph"
Cohesion: 0.38
Nodes (6): graphify-out/GRAPH_REPORT.md, graphify explain, graphify query, graphify, graphify update, graphify-out/wiki/index.md

### Community 11 - "Domain Exceptions"
Cohesion: 0.5
Nodes (4): Exception, ConnectionException, DomainException, InvalidStrategyException

## Knowledge Gaps
- **27 isolated node(s):** `InvalidStrategyException`, `IReadOnlyList`, `EnsureSchemaOptions`, `NormalizerContext`, `DomainException` (+22 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ILogger` connect `Schema Sync Pipeline Steps` to `Background Worker Service`, `Schema Reader & Bootstrapper`, `Table Data Repository`?**
  _High betweenness centrality (0.099) - this node is a cross-community bridge._
- **Why does `SyncProcess` connect `Dapper / DB Connectivity` to `Schema Sync Pipeline Steps`?**
  _High betweenness centrality (0.088) - this node is a cross-community bridge._
- **Why does `SyncContext` connect `Schema Sync Pipeline Steps` to `Dapper / DB Connectivity`, `Sync Strategy & Pipeline`?**
  _High betweenness centrality (0.083) - this node is a cross-community bridge._
- **What connects `InvalidStrategyException`, `IReadOnlyList`, `EnsureSchemaOptions` to the rest of the system?**
  _27 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Dapper / DB Connectivity` be split into smaller, more focused modules?**
  _Cohesion score 0.08 - nodes in this community are weakly interconnected._