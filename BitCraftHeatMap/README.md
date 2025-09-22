# BitCraft HeatMap

## Settings

Settings can be changed in `appsettings.json` or via environment variables.

| Name                | Environment Variable | Default Value                                 | Description |
|---------------------|----------------------|-----------------------------------------------|-------------|
| `BitCraft:Host`     | `BitCraft__Host`     | `wss://bitcraft-early-access.spacetimedb.com` |             |
| `BitCraft:Region`   | `BitCraft__Region`   | `bitcraft-8`                                  |             |
| `BitCraft:Token`    | `BitCraft__Token`    | `INSERT_TOKEN_HERE`                           |             |
| `Database:Host`     | `Database__Host`     | `localhost`                                   |             |
| `Database:Port`     | `Database__Port`     | `5432`                                        |             |
| `Database:Database` | `Database__Database` | `postgres`                                    |             |
| `Database:Username` | `Database__Username` | `postgres`                                    |             |
| `Database:Password` | `Database__Password` | `CHANGE_ME`                                   |             |


## Exit Codes

| Exit Code | Cause                                           |
|-----------|-------------------------------------------------|
| 10        | Error in Database connection                    |
| 20        | Error while establishing connection to BitCraft |
| 30        | Error in LocationHandler Task                   |


## Database

> [TimescaleDB](https://github.com/timescale/timescaledb)

```postgresql
create table player_locations_v2
(
    entity_id   bigint  not null,
    timestamp   TIMESTAMPTZ not null,
    location_x  integer not null,
    location_z  integer not null,
    constraint player_locations_v2_pk primary key (entity_id, timestamp)
) WITH (
    tsdb.hypertable,
    tsdb.partition_column='timestamp',
    tsdb.segmentby = 'entity_id',
    tsdb.orderby = 'timestamp DESC',
    tsdb.chunk_interval='1 day'
);
SELECT add_retention_policy('player_locations_v2', INTERVAL '7 days');
```


## Docker Compose

```yaml
services:
  postgres:
    container_name: timescaledb
    image: timescale/timescaledb-ha:pg17
    networks:
      backend:
        aliases:
          - timescaledb.local
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: CHANGE_ME
      PGDATA: /pgdata
    volumes:
      - ./timescaledb:/pgdata
    ports:
      - "0.0.0.0:5432:5432"
    restart: unless-stopped

  heatmap:
    container_name: bitcraft-heatmap
    image: ghcr.io/bluepanda222/bitcraft-heatmap:latest
    networks:
      - backend
    environment:
      BitCraft__Host: wss://bitcraft-early-access.spacetimedb.com
      BitCraft__Region: bitcraft-8
      BitCraft__Token: INSERT_TOKEN_HERE
      Database__Host: timescaledb.local
      Database__Port: 5432
      Database__Database: postgres
      Database__Username: postgres
      Database__Password: CHANGE_ME
    restart: unless-stopped

networks:
  backend:
```
