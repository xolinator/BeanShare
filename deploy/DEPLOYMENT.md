# BeanShare Deployment Guide

## Architecture Overview

BeanShare is deployed on [Fly.io](https://fly.io) as three separate services sharing a PostgreSQL database cluster:

| Service | App Name | URL | Memory |
|---------|----------|-----|--------|
| Blazor Web | `beanshare-web` | https://beanshare-web.fly.dev | 512 MB |
| REST API | `beanshare-api` | https://beanshare-api.fly.dev | 256 MB |
| Keycloak (IdP) | `beanshare-keycloak` | https://beanshare-keycloak.fly.dev | 1024 MB |
| PostgreSQL | `beanshare-db` | Internal (Flycast) | 256 MB / 1 GB volume |

All services run in the `fra` (Frankfurt) region, use shared-cpu-1x VMs, and auto-suspend when idle to minimize costs.

```
┌──────────────┐     OIDC      ┌──────────────────┐
│   Browser    │◄─────────────►│    Keycloak       │
│   / MAUI     │               │  (beanshare-kc)   │
└──────┬───────┘               └────────┬──────────┘
       │                                │
       │  HTTPS                         │ PostgreSQL
       ▼                                ▼
┌──────────────┐   HTTP API    ┌──────────────────┐
│  BlazorWeb   │──────────────►│    REST API       │
│  (SSR+Server)│               │  (FastEndpoints)  │
└──────┬───────┘               └────────┬──────────┘
       │                                │
       │ PostgreSQL                     │ PostgreSQL
       ▼                                ▼
    ┌─────────────────────────────────────┐
    │         Fly Postgres Cluster        │
    │           (beanshare-db)            │
    └─────────────────────────────────────┘
```

## Prerequisites

- [Fly.io CLI](https://fly.io/docs/flyctl/install/) (`flyctl` or `fly`)
- Fly.io account (`flyctl auth login`)
- Docker (used by Fly's remote builder)
- Bash shell (Git Bash on Windows works)

## Deploy Scripts

All deployment scripts live in the `deploy/` directory:

| File | Purpose |
|------|---------|
| `deploy.sh` | Main orchestrator - handles all deploy commands |
| `Dockerfile.web` | Multi-stage build for BlazorWeb (.NET 10 preview) |
| `Dockerfile.api` | Multi-stage build for REST API (.NET 10 preview) |
| `Dockerfile.keycloak` | Keycloak 26.0 with PostgreSQL |
| `fly-web.toml` | Fly.io config for BlazorWeb |
| `fly-api.toml` | Fly.io config for REST API |
| `fly-keycloak.toml` | Fly.io config for Keycloak |
| `setup-keycloak-fly.sh` | Configures Keycloak realm, clients, users via REST API |
| `.secrets.env` | Generated secrets (gitignored, local only) |
| `blazor-framework/blazor.web.js` | Vendored Blazor JS (workaround for .NET 10 preview) |

## Commands

```bash
# From the BeanShare repo root:

# First-time full setup (creates apps, DB, deploys everything)
bash deploy/deploy.sh full

# Individual deployments
bash deploy/deploy.sh deploy-keycloak    # Rebuild & deploy Keycloak
bash deploy/deploy.sh deploy-api         # Rebuild & deploy API
bash deploy/deploy.sh deploy-web         # Rebuild & deploy BlazorWeb
bash deploy/deploy.sh deploy-all         # Deploy all three services

# Other commands
bash deploy/deploy.sh setup              # Create Fly apps + Postgres (no deploy)
bash deploy/deploy.sh setup-keycloak     # Configure Keycloak realm/users only
bash deploy/deploy.sh status             # Show deployment status
bash deploy/deploy.sh secrets            # Show secrets file path
```

## First-Time Setup (`full`)

The `full` command performs these steps in order:

1. **Generate secrets** - Creates random passwords/secrets, saved to `deploy/.secrets.env`
   - `KC_ADMIN_PASSWORD` (20 chars) - Keycloak admin console password
   - `WEB_CLIENT_SECRET` (24 chars) - OIDC client secret for BlazorWeb
   - `API_CLIENT_SECRET` (24 chars) - OIDC client secret for API
2. **Create Fly apps** - Three apps: `beanshare-keycloak`, `beanshare-api`, `beanshare-web`
3. **Create Postgres cluster** - Shared `beanshare-db` with 1 GB volume
4. **Attach databases** - Creates per-app databases and connection strings
5. **Set Fly secrets** - Pushes secrets to each app as environment variables
6. **Deploy Keycloak** - Builds Docker image, deploys, waits for health check
7. **Configure Keycloak** - Creates realm, clients, roles, and demo users
8. **Deploy API** - Builds and deploys the REST API
9. **Deploy BlazorWeb** - Builds and deploys the Blazor frontend

## Secrets & Environment Variables

### Keycloak (`beanshare-keycloak`)
| Variable | Description |
|----------|-------------|
| `KC_DB_URL` | PostgreSQL JDBC URL |
| `KC_DB_USERNAME` | Database username |
| `KC_DB_PASSWORD` | Database password |
| `KC_HOSTNAME` | `beanshare-keycloak.fly.dev` |
| `KEYCLOAK_ADMIN` | Admin username (`admin`) |
| `KEYCLOAK_ADMIN_PASSWORD` | Admin password (from `.secrets.env`) |

### BlazorWeb (`beanshare-web`)
| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `UseOidc` | `true` - enables OIDC authentication |
| `Oidc__Authority` | `https://beanshare-keycloak.fly.dev/realms/beanshare` |
| `Oidc__ClientId` | `beanshare-web` |
| `Oidc__ClientSecret` | Web client secret |
| `ApiBaseUrl` | `https://beanshare-api.fly.dev` |

### REST API (`beanshare-api`)
| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `UseOidc` | `true` - enables JWT Bearer authentication |
| `Oidc__Authority` | `https://beanshare-keycloak.fly.dev/realms/beanshare` |
| `Oidc__ClientId` | `beanshare-api` |
| `Oidc__ClientSecret` | API client secret |

## Authentication Flow

1. User clicks "Sign In" on BlazorWeb
2. Browser redirects to Keycloak login page (`beanshare-keycloak.fly.dev`)
3. User enters credentials → Keycloak validates against its PostgreSQL user store
4. Keycloak redirects back to BlazorWeb with authorization code
5. BlazorWeb exchanges code for tokens (server-side, confidential client)
6. BlazorWeb extracts `realm_access.roles` from token and maps to ASP.NET role claims
7. User is synced to BeanShare's own database via `IUserSynchronizationService`
8. Cookie-based session is established for subsequent requests

### Keycloak Clients

| Client | Type | Purpose |
|--------|------|---------|
| `beanshare-web` | Confidential | Blazor Server OIDC login |
| `beanshare-api` | Confidential | API service-to-service |
| `beanshare-mobile` | Public | MAUI app (PKCE, `beanshare://callback`) |

## Demo Users

All demo users are created in Keycloak by `setup-keycloak-fly.sh`. Password for all: `password123`

| Username | Email | Role |
|----------|-------|------|
| john.smith | john.smith@beanshare.dev | Admin |
| sarah.johnson | sarah.johnson@beanshare.com | User |
| mike.wilson | mike.wilson@beanshare.com | User |
| emma.davis | emma.davis@beanshare.com | User |
| alex.brown | alex.brown@beanshare.com | User |
| lisa.martinez | lisa.martinez@beanshare.com | User |
| david.garcia | david.garcia@beanshare.com | User |
| test.user | test@beanshare.com | User |

## Database

- **Engine:** PostgreSQL (Fly Postgres, single-node)
- **Databases:** Separate databases per service, all on the same cluster
- **Schema creation:** `EnsureCreated()` on startup (no EF migrations)
- **DataProtection keys:** Persisted to the `DataProtectionKeys` table (BlazorWeb)
- **Seed data:** Both API and BlazorWeb run `DatabaseSeeder` on startup with demo data

## Known Issues

### Keycloak Stale Connections After Suspension
When the Fly machine auto-suspends and wakes up, Keycloak's PostgreSQL connection pool may contain stale connections. The first login attempt after wake-up may fail with "Internal Server Error". The second attempt succeeds as Keycloak refreshes its connection pool.

### blazor.web.js Not in Publish Output (.NET 10 Preview)
The .NET 10 preview SDK does not include `blazor.web.js` in the publish output. A vendored copy is stored at `deploy/blazor-framework/blazor.web.js` and copied into the Docker image as a fallback. `Program.cs` includes both `UseStaticFiles()` (for the fallback) and `MapStaticAssets()` (for fingerprinted assets).

### BeanShare.BlazorWeb.styles.css 404
Component-scoped CSS (`*.razor.css`) files were removed during development, so the bundled `BeanShare.BlazorWeb.styles.css` doesn't exist. This causes a harmless 404 in the browser console but has no visual impact since all styling uses standalone CSS files.

## Monitoring & Logs

```bash
# View live logs
flyctl logs -a beanshare-web
flyctl logs -a beanshare-api
flyctl logs -a beanshare-keycloak

# Check app status
flyctl status -a beanshare-web

# SSH into a running machine
flyctl ssh console -a beanshare-web

# Open the Fly.io dashboard
flyctl dashboard -a beanshare-web

# Keycloak admin console
# URL: https://beanshare-keycloak.fly.dev/admin
# Username: admin
# Password: (see deploy/.secrets.env → KC_ADMIN_PASSWORD)
```

## Cost Optimization

All machines use `auto_stop_machines = "suspend"` and `min_machines_running = 0`, which means:
- Machines suspend after ~5 minutes of inactivity
- First request after suspension takes 3-8 seconds (cold start)
- No charges while suspended (only storage costs remain)
- Keycloak has the longest cold start (~5-8s) due to JVM startup
