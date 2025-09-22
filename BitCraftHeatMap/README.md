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


