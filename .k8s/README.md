# Harmony on Kubernetes (kustomize)

```
.k8s/
  base/                     namespace "harmony", common ConfigMap, Secret, infrastructure and Harmony services
    infra/                  MongoDB, RabbitMQ, Redis, Elasticsearch (StatefulSets + PVCs), Kibana, Aspire dashboard
    apps/                   api, automations, notifications, signalr, sourcecontrol, api-gateway, client
    secrets.env             development credentials -> Secret "harmony-secrets"
  overlays/
    sqlserver/              SQL Server StatefulSet + database settings for api / automations / notifications
    postgres/               PostgreSQL StatefulSet + database settings for api / automations / notifications
```

## Run on Docker Desktop

```powershell
# 1. images (docker compose tags them harmony-<service>:latest)
docker compose build

# 2. namespace + development certificate
kubectl create namespace harmony
kubectl create configmap cert-config-map -n harmony --from-file $env:USERPROFILE\.aspnet\https\harmony.pfx

# 3. everything (pick one database)
kubectl apply -k .k8s/overlays/sqlserver
# kubectl apply -k .k8s/overlays/postgres

kubectl get pods -n harmony -w
```

| | |
|---|---|
| Harmony | http://localhost:7096 |
| Aspire dashboard (traces, metrics, logs) | http://localhost:18888 |
| Kibana | http://localhost:5601 |

## Persistence

SQL Server, PostgreSQL, MongoDB, RabbitMQ, Redis and Elasticsearch are StatefulSets with `volumeClaimTemplates`
(`data-<statefulset>-0` PVCs on the default `hostpath` StorageClass). Deleting or restarting pods keeps the data.
`kubectl delete -k ...` keeps the PVCs as well; remove them explicitly to start from scratch:

```powershell
kubectl delete pvc -n harmony --all
```

## Secrets

`base/secrets.env` contains **development** credentials only. For any shared environment, replace its values or create
the `harmony-secrets` Secret yourself (same keys) and remove the `secretGenerator` from `base/kustomization.yaml`.
Database passwords are written into the volumes on first start - changing them later requires changing them in the database too.
