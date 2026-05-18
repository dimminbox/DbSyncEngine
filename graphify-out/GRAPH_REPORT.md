# Graph Report - .  (2026-05-15)

## Corpus Check
- Corpus is ~9,995 words - fits in a single context window. You may not need a graph.

## Summary
- 434 nodes · 444 edges · 86 communities (49 shown, 37 thin omitted)
- Extraction: 93% EXTRACTED · 7% INFERRED · 0% AMBIGUOUS · INFERRED: 30 edges (avg confidence: 0.89)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Application Wiring|Application Wiring]]
- [[_COMMUNITY_Sync Pipeline Steps|Sync Pipeline Steps]]
- [[_COMMUNITY_Worker Background Service|Worker Background Service]]
- [[_COMMUNITY_Infrastructure Persistence|Infrastructure Persistence]]
- [[_COMMUNITY_Table Data Repositories|Table Data Repositories]]
- [[_COMMUNITY_Schema Normalization|Schema Normalization]]
- [[_COMMUNITY_Postgres DDL Generation|Postgres DDL Generation]]
- [[_COMMUNITY_MySQL DDL Generation|MySQL DDL Generation]]
- [[_COMMUNITY_DDL Factory & Schema Bootstrap|DDL Factory & Schema Bootstrap]]
- [[_COMMUNITY_Dapper Base Repository|Dapper Base Repository]]
- [[_COMMUNITY_Schema Bootstrapping Pipeline|Schema Bootstrapping Pipeline]]
- [[_COMMUNITY_Schema Abstractions|Schema Abstractions]]
- [[_COMMUNITY_Community 12|Community 12]]
- [[_COMMUNITY_Community 13|Community 13]]
- [[_COMMUNITY_Community 14|Community 14]]
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 28|Community 28]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 31|Community 31]]
- [[_COMMUNITY_Community 32|Community 32]]
- [[_COMMUNITY_Community 33|Community 33]]
- [[_COMMUNITY_Community 34|Community 34]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]
- [[_COMMUNITY_Community 37|Community 37]]
- [[_COMMUNITY_Community 38|Community 38]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Community 40|Community 40]]
- [[_COMMUNITY_Community 41|Community 41]]
- [[_COMMUNITY_Community 42|Community 42]]
- [[_COMMUNITY_Community 43|Community 43]]
- [[_COMMUNITY_Community 44|Community 44]]
- [[_COMMUNITY_Community 45|Community 45]]
- [[_COMMUNITY_Community 46|Community 46]]
- [[_COMMUNITY_Community 47|Community 47]]
- [[_COMMUNITY_Community 48|Community 48]]
- [[_COMMUNITY_Community 82|Community 82]]
- [[_COMMUNITY_Community 83|Community 83]]
- [[_COMMUNITY_Community 84|Community 84]]
- [[_COMMUNITY_Community 85|Community 85]]

## God Nodes (most connected - your core abstractions)
1. `PostgresDdlGenerator` - 16 edges
2. `MySqlDdlGenerator` - 16 edges
3. `SchemaBootstrapper` - 13 edges
4. `SyncProcess` - 10 edges
5. `EnsureTargetSchemaStep` - 9 edges
6. `ITargetDdlGenerator` - 9 edges
7. `ILogger` - 8 edges
8. `ISchemaBootstrapper` - 8 edges
9. `PostgresSchemaNormalizer` - 8 edges
10. `PostgresTableDataRepository` - 8 edges

## Surprising Connections (you probably didn't know these)
- `Full Sync Pipeline` --rationale_for--> `FullSyncStrategy`  [INFERRED]
  README.md → src/DbSyncEngine.Application/Strategies/Implementations/FullSyncStrategy.cs
- `Chunked Processing Strategy` --rationale_for--> `ReadDataStep`  [INFERRED]
  README.md → src/DbSyncEngine.Application/Pipelines/Steps/FullSyncSteps/ReadDataStep.cs
- `Chunked Processing Strategy` --rationale_for--> `WriteDataStep`  [INFERRED]
  README.md → src/DbSyncEngine.Application/Pipelines/Steps/FullSyncSteps/WriteDataStep.cs
- `SQLite State Tracking` --rationale_for--> `SyncProcess`  [INFERRED]
  README.md → src/DbSyncEngine.Domain/SyncProcessAggregate/SyncProcess.cs
- `Schema Bootstrap Process` --rationale_for--> `SchemaBootstrapper`  [INFERRED]
  README.md → src/DbSyncEngine.Infrastructure/Persistence/Schema/SchemaBootstrapper.cs

## Hyperedges (group relationships)
- **Full Sync Pipeline Steps** — getsyncstep_getsyncstep, ensuretargetschemastep_ensuretargetschemastep, readdatastep_readdatastep, mapchunkstep_mapchunkstep, writedatastep_writedatastep [EXTRACTED 1.00]
- **Schema Management Interfaces** — ischemareader_ischemareader, ischemanormalizer_ischemanormalizer, ischemabootstrapper_ischemabootstrapper [INFERRED 0.85]
- **Database Provider Normalization Pair** — mysqlvaluenormalizer_mysqlvaluenormalizer, postgresvaluenormalizer_postgresvaluenormalizer, valuenormalizerfactory_valuenormalizerfactory [EXTRACTED 0.95]
- **Schema Reader/Normalizer Provider Pair** — mysqlschemareader_mysqlschemareader, mysqlschemanormalizer_mysqlschemanormalizer, postgresschemareader_postgresschemareader, postgresschemanormalizer_postgresschemanormalizer [INFERRED 0.85]
- **DDL Generator Provider Pair** — postgresddlgenerator_postgresddlgenerator, mysqlddlgenerator_mysqlddlgenerator, targetddlgeneratorfactory_targetddlgeneratorfactory [EXTRACTED 0.95]
- **Worker Service Bootstrap** — program_program, dependencyinjection_workerdependencyinjection, syncbackgroundservice_syncbackgroundservice [EXTRACTED 1.00]

## Communities (86 total, 37 thin omitted)

### Community 0 - "Application Wiring"
Cohesion: 0.07
Nodes (35): DbEndpoint, Worker DependencyInjection, Docker Compose Config, EnsureTargetSchemaStep, FullSyncStrategy, GetSyncStep, ISchemaBootstrapper, ISyncStrategy (+27 more)

### Community 1 - "Sync Pipeline Steps"
Cohesion: 0.1
Nodes (12): TableDataRepositoryFactory, GetSyncStep, MapChunkStep, PrepareToWriteDataStep, ReadDataStep, UpdateSyncStep, WriteDataStep, ILogger (+4 more)

### Community 2 - "Worker Background Service"
Cohesion: 0.09
Nodes (12): BackgroundService, RowData, SyncBackgroundService, Dictionary, ValueNormalizerFactory, SyncStrategyFactory, IOptionsMonitor, IRowAccessor (+4 more)

### Community 3 - "Infrastructure Persistence"
Cohesion: 0.1
Nodes (9): DbConnectionFactory, SyncProcessRepositoryFactory, IDbConnectionFactory, ISchemaReader, MySqlSchemaReader, PostgresSchemaReader, SyncProcessSql, string (+1 more)

### Community 4 - "Table Data Repositories"
Cohesion: 0.14
Nodes (8): ITableDataRepository, MySqlConnection, NpgsqlConnection, Regex, MySqlTableDataRepository, PostgresTableDataRepository, TableDataRepositoryBase, TableDataRepositoryBase

### Community 5 - "Schema Normalization"
Cohesion: 0.17
Nodes (5): ISchemaNormalizer, DbSyncEngine.Infrastructure.Persistence.Schema.Normalization, MySqlSchemaNormalizer, DbSyncEngine.Infrastructure.Persistence.Schema.Normalization, PostgresSchemaNormalizer

### Community 8 - "DDL Factory & Schema Bootstrap"
Cohesion: 0.16
Nodes (4): TargetDdlGeneratorFactory, IReadOnlyDictionary, ITargetDdlGeneratorFactory, SchemaBootstrapper

### Community 9 - "Dapper Base Repository"
Cohesion: 0.15
Nodes (5): DapperRepository, DapperRepository, IDbConnection, ISyncProcessRepository, SyncProcessRepository

### Community 10 - "Schema Bootstrapping Pipeline"
Cohesion: 0.18
Nodes (6): EnsureSchemaOptions, EnsureTargetSchemaStep, IDictionary, ISchemaBootstrapper, ISchemaNormalizerFactory, SchemaNormalizerFactory

### Community 11 - "Schema Abstractions"
Cohesion: 0.24
Nodes (12): ISchemaNormalizer, ISchemaNormalizerFactory, ISchemaReader, ISchemaReaderFactory, MySqlSchemaNormalizer, MySqlSchemaReader, PostgresSchemaNormalizer, PostgresSchemaReader (+4 more)

### Community 12 - "Community 12"
Cohesion: 0.2
Nodes (6): SyncPipeline, FullSyncStrategy, IReadOnlyList, ISyncPipeline, ISyncStrategy, SyncEntityConfig

### Community 16 - "Community 16"
Cohesion: 0.25
Nodes (9): DapperRepository, ISyncProcessRepository, ISyncProcessRepositoryFactory, ISyncStep, SyncProcessRepository, SyncProcessRepositoryFactory, SyncProcessSchemaInitializer, SyncProcessSql (+1 more)

### Community 18 - "Community 18"
Cohesion: 0.29
Nodes (3): MySqlValueNormalizer, PostgresValueNormalizer, IValueNormalizer

### Community 19 - "Community 19"
Cohesion: 0.53
Nodes (6): Application DependencyInjection, IValueNormalizer, IValueNormalizerFactory, MySqlValueNormalizer, PostgresValueNormalizer, ValueNormalizerFactory

### Community 22 - "Community 22"
Cohesion: 0.4
Nodes (3): Exception, DomainException, InvalidStrategyException

### Community 26 - "Community 26"
Cohesion: 0.6
Nodes (5): ITargetDdlGenerator, ITargetDdlGeneratorFactory, MySqlDdlGenerator, PostgresDdlGenerator, TargetDdlGeneratorFactory

## Knowledge Gaps
- **43 isolated node(s):** `SyncConfig`, `SyncEntityConfig`, `NormalizerOptions`, `DbEndpoint`, `SyncContext` (+38 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **37 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SchemaBootstrapper` connect `DDL Factory & Schema Bootstrap` to `Infrastructure Persistence`, `Sync Pipeline Steps`, `Schema Bootstrapping Pipeline`, `Worker Background Service`?**
  _High betweenness centrality (0.030) - this node is a cross-community bridge._
- **Why does `IServiceProvider` connect `Worker Background Service` to `Schema Bootstrapping Pipeline`, `Community 12`?**
  _High betweenness centrality (0.022) - this node is a cross-community bridge._
- **What connects `SyncConfig`, `SyncEntityConfig`, `NormalizerOptions` to the rest of the system?**
  _43 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Application Wiring` be split into smaller, more focused modules?**
  _Cohesion score 0.07 - nodes in this community are weakly interconnected._
- **Should `Sync Pipeline Steps` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._
- **Should `Worker Background Service` be split into smaller, more focused modules?**
  _Cohesion score 0.09 - nodes in this community are weakly interconnected._
- **Should `Infrastructure Persistence` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._