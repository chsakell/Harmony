# Docker & Kubernetes

## Development certificate
```powershell
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\harmony.pfx -p HarmonyTeamsSecretKey
dotnet dev-certs https --trust
```

## docker compose
```powershell
copy .env.example .env          # optional - every setting has a development default
docker compose build
docker compose up -d                                                         # SQL Server
docker compose -f docker-compose.yml -f docker-compose.postgres.yml up -d    # PostgreSQL instead of SQL Server
```

| | |
|---|---|
| Harmony | http://localhost:7096 |
| Aspire dashboard (OpenTelemetry traces, metrics, logs) | http://localhost:18888 |
| Kibana (Serilog -> Elasticsearch) | http://localhost:5601 |
| RabbitMQ management | http://localhost:15672 |

Databases (SQL Server / PostgreSQL, MongoDB, RabbitMQ, Redis, Elasticsearch) use named volumes (`harmony_*-data`):
`docker compose down` keeps the data, `docker compose down -v` deletes it.
Passwords are written into the volumes on first start - change them in `.env` before the first `up`.

## Kubernetes (Docker Desktop)
```powershell
docker compose build
kubectl create namespace harmony
kubectl create configmap cert-config-map -n harmony --from-file $env:USERPROFILE\.aspnet\https\harmony.pfx
kubectl apply -k .k8s/overlays/sqlserver     # or .k8s/overlays/postgres
```
See [.k8s/README.md](../.k8s/README.md).

## .NET Aspire
For local development prefer the Aspire AppHost, which needs no certificate export or compose files:
```powershell
aspire run      # or: dotnet run --project src/Aspire/Harmony.AppHost
```
