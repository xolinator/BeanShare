# BeanShare Production Deployment Guide

## Architecture Overview

BeanShare consists of three services:

| Service | Description | Port |
|---------|-------------|------|
| **BeanShare.BlazorWeb** | Blazor Server web application | 5126 (dev) / 8080 (internal prod) |
| **BeanShare.Api** | REST API (FastEndpoints) | 5247 (dev and internal prod) |
| **PostgreSQL** | Database (v15+) | 5432 |

An external **OpenID Connect provider** is required for authentication. BeanShare works with any standards-compliant OIDC provider — Keycloak, Auth0, Azure AD/Entra ID, Google Identity Platform, Okta, and others.

```
┌─────────────────────────────────────────────────────────────────┐
│                         PRODUCTION                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │  OIDC Provider│    │  PostgreSQL  │    │  BeanShare   │       │
│  │  (Keycloak,  │    │  (Database)  │    │  Blazor Web  │       │
│  │  Auth0, etc.)│    │  Port 5432   │    │  Port 443    │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│         │                   │                   │                │
│         └───────────────────┴───────────────────┘                │
│                         │                                        │
│                  ┌──────────────┐                                │
│                  │   NGINX /    │                                │
│                  │   Traefik    │                                │
│                  └──────────────┘                                │
│                         │                                        │
└─────────────────────────┴────────────────────────────────────────┘
                          │
                    HTTPS (443)
                          │
                      Internet
```

---

## Prerequisites

- Docker & Docker Compose
- .NET 10 SDK (or use containerized build)
- PostgreSQL 15+
- An OIDC provider with admin access to create clients
- Reverse proxy (Nginx, Traefik, or cloud load balancer)
- SSL certificates (Let's Encrypt or commercial)

---

## Step 1: Configure the OIDC Provider

BeanShare requires **three OIDC client registrations** in your identity provider. The exact steps vary by provider, but the requirements are the same.

### Client 1: Web Application (Confidential)

| Setting | Value |
|---------|-------|
| Client ID | `beanshare-web` (configurable) |
| Client Type | Confidential (server-side) |
| Grant Type | Authorization Code |
| Redirect URI | `https://YOUR_WEB_DOMAIN/signin-oidc` |
| Post-Logout URI | `https://YOUR_WEB_DOMAIN/signout-callback-oidc` |
| Scopes | `openid`, `profile`, `email` |

### Client 2: API (Resource Server)

| Setting | Value |
|---------|-------|
| Client ID | `beanshare-api` (configurable) |
| Client Type | Confidential or Bearer-only |
| Purpose | Validates JWT access tokens |

### Client 3: Mobile Application (Public)

| Setting | Value |
|---------|-------|
| Client ID | `beanshare-mobile` (configurable) |
| Client Type | Public (no client secret) |
| Grant Type | Authorization Code with PKCE |
| Redirect URI | `beanshare://callback` |
| Scopes | `openid`, `profile`, `email` |

### Required Claims

BeanShare reads the following claims from ID tokens and access tokens:

| Claim | Purpose |
|-------|---------|
| `sub` | User identifier (required) |
| `email` | User email |
| `name` | Display name (falls back to `preferred_username`) |
| `preferred_username` | Username |
| `picture` | Avatar URL (optional) |

### Role Claims

BeanShare supports optional role-based access (e.g., system admin). Roles can be provided via any of these formats:

- Standard `role` claim
- `realm_access.roles` JSON array (Keycloak convention)
- Standard `ClaimTypes.Role`

### Provider-Specific Setup

<details>
<summary><b>Keycloak</b></summary>

Deploy Keycloak with a persistent database backend (not the dev H2 mode):

```bash
docker run -d \
  --name keycloak \
  -e KC_DB=postgres \
  -e KC_DB_URL=jdbc:postgresql://DB_HOST:5432/keycloak \
  -e KC_DB_USERNAME=keycloak \
  -e KC_DB_PASSWORD=<KC_DB_PASSWORD> \
  -e KC_HOSTNAME=auth.yourdomain.com \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=<ADMIN_PASSWORD> \
  -p 8080:8080 \
  quay.io/keycloak/keycloak:23.0 start
```

After Keycloak starts:

1. Open the Admin Console at `https://auth.yourdomain.com/admin`
2. Create a new realm named `beanshare`
3. Import `scripts/keycloak-realm.json` via Realm Settings > Partial Import
4. Update client redirect URIs to match your production domain
5. Change client secrets for `beanshare-web` and `beanshare-api`
6. Delete demo users imported from the realm JSON (development only)
7. Create a real admin user and assign the `admin` realm role

Authority URL format: `https://your-keycloak.example.com/realms/beanshare`

Keycloak stores roles in `realm_access.roles` as a JSON object — BeanShare parses this automatically.

</details>

<details>
<summary><b>Auth0</b></summary>

1. Create a "Regular Web Application" for the web client
2. Create an "API" for the API audience
3. Create a "Native" application for mobile with PKCE
4. Authority URL: `https://YOUR_TENANT.auth0.com/`
5. Use Auth0 Rules/Actions to add roles to the `role` claim in tokens

</details>

<details>
<summary><b>Azure AD / Entra ID</b></summary>

1. Register three applications in Azure AD
2. Configure redirect URIs for each
3. Authority URL: `https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0`
4. Use Azure AD App Roles assigned to users
5. Set `MapInboundClaims = false` (already configured in BeanShare)

</details>

<details>
<summary><b>External SSO via Keycloak (SAML/OIDC brokering)</b></summary>

Keycloak can act as a broker for external identity providers — useful for integrating with university or corporate SSO systems.

1. In the Keycloak Admin Console, navigate to `Identity Providers > Add provider`
2. Select SAML v2.0 or OpenID Connect v1.0
3. Configure the IdP's metadata URL or endpoint URLs
4. Set up attribute mapping (email, name → Keycloak user fields)
5. Provide your SP metadata to the external IdP administrator:
   - SAML SP descriptor: `https://auth.yourdomain.com/realms/beanshare/protocol/saml/descriptor`
   - OIDC redirect URI: `https://auth.yourdomain.com/realms/beanshare/broker/{alias}/endpoint`

Refer to the [Keycloak Identity Broker documentation](https://www.keycloak.org/docs/latest/server_admin/#_identity_broker) for details.

</details>

---

## Step 2: Configure the Database

```sql
CREATE DATABASE beanshare;
CREATE USER beanshare WITH PASSWORD '<STRONG_PASSWORD>';
GRANT ALL PRIVILEGES ON DATABASE beanshare TO beanshare;
```

The application applies EF Core migrations automatically on first startup. The database user needs privileges to create and alter schema objects.

Connection string format:
```
Host=db.example.com;Port=5432;Database=beanshare;Username=beanshare;Password=your-secure-password;SSL Mode=Require
```

---

## Step 3: Set Environment Variables

Both applications read configuration from environment variables using the ASP.NET Core double-underscore convention (`Oidc__Authority` maps to `Oidc:Authority` in config).

### Required Variables

| Variable | Service | Description |
|----------|---------|-------------|
| `ConnectionStrings__DefaultConnection` | API, Web | PostgreSQL connection string |
| `UseOidc` | API, Web | Must be `true` |
| `Oidc__Authority` | API, Web | OIDC provider URL |
| `Oidc__ClientId` | Web | Web client ID (default: `beanshare-web`) |
| `Oidc__ClientSecret` | Web | Web client secret |
| `Oidc__Audience` | API | API audience (default: `beanshare-api`) |
| `QrCodeBaseUrl` | Web | Optional public URL for QR code deep links; defaults to the current web origin |

### Optional Variables

| Variable | Service | Description | Default |
|----------|---------|-------------|---------|
| `OpenExchangeRates__AppId` | API | API key for currency conversion | (disabled) |
| `Email__Enabled` | API | Enable email notifications | `false` |
| `Email__SmtpHost` | API | SMTP server | `smtp.gmail.com` |
| `Email__SmtpPort` | API | SMTP port | `587` |
| `Email__SmtpUsername` | API | SMTP username | - |
| `Email__SmtpPassword` | API | SMTP password | - |
| `IncludeDemoData` | API, Web | Seed demo spaces/users/coffee data on startup | `false` |
| `UploadsRootPath` | API, Web | Shared filesystem path for uploaded avatars | OS temp directory |

---

## Step 4: Deploy

### Option A: Docker Compose (Recommended for Self-Hosting)

1. Build the Docker images:

```bash
docker build -f deploy/Dockerfile.api -t beanshare-api .
docker build -f deploy/Dockerfile.web -t beanshare-web .
```

2. Create a `docker-compose.prod.yml`:

```yaml
services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: beanshare
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: beanshare
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U beanshare"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    image: beanshare-api
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=beanshare;Username=beanshare;Password=${POSTGRES_PASSWORD}
      UseOidc: "true"
      Oidc__Authority: ${OIDC_AUTHORITY}
      Oidc__Audience: beanshare-api
      ASPNETCORE_ENVIRONMENT: Production

  web:
    image: beanshare-web
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=beanshare;Username=beanshare;Password=${POSTGRES_PASSWORD}
      UseOidc: "true"
      Oidc__Authority: ${OIDC_AUTHORITY}
      Oidc__ClientId: beanshare-web
      Oidc__ClientSecret: ${OIDC_WEB_SECRET}
      ASPNETCORE_ENVIRONMENT: Production
    ports:
      - "8080:8080"

volumes:
  pgdata:
```

3. Start:

```bash
docker compose -f docker-compose.prod.yml up -d
```

### Option B: Direct Deployment (VPS / Bare Metal)

```bash
dotnet publish src/Presentation/BeanShare.Api -c Release -o /opt/beanshare/api
dotnet publish src/Presentation/BeanShare.BlazorWeb -c Release -o /opt/beanshare/web
```

Set environment variables in the service definition or via `/etc/environment`, then run:

```bash
cd /opt/beanshare/api && dotnet BeanShare.Api.dll
cd /opt/beanshare/web && dotnet BeanShare.BlazorWeb.dll
```

### Option C: Cloud Platforms (fly.io, Railway, Render, etc.)

The Dockerfiles work on any container platform. See `deploy/DEPLOYMENT.md` for a complete fly.io example.

---

## Step 5: Reverse Proxy (Nginx)

Blazor Server uses WebSockets, so the web upstream needs the `Upgrade` and `Connection` headers. A same-origin reverse proxy keeps the browser on one public host while routing `/api/` to the API service.

```nginx
server {
    listen 443 ssl;
    server_name yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api/ {
        proxy_pass http://localhost:5247;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /swagger {
        proxy_pass http://localhost:5247;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Equivalent NixOS module setup:

```nix
{
  services.beanshare-api = {
    enable = true;
    listenAddress = "127.0.0.1";
    port = 5247;
  };

  services.beanshare-blazorweb = {
    enable = true;
    listenAddress = "127.0.0.1";
    port = 8080;

    nginx = {
      enable = true;
      domain = "beanshare.example.com";
      proxyApi.enable = true;
    };
  };
}
```

---

## Database Seeding

On first startup, the application seeds **essential data only** in production:

- 12 global coffee presets (Espresso, Cappuccino, Pour Over, AeroPress, etc.)

No demo users, spaces, or fake consumption data are created unless `IncludeDemoData=true` is set. The first user to log in with the `admin` role in the OIDC provider is automatically synced to the database as an admin via `UserSynchronizationService`.

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| OIDC redirect loop | Incorrect redirect URI in provider | Verify redirect URI matches exactly, including trailing slash |
| 401 on all API calls | Token issuer mismatch | Check `Oidc__Authority` matches the `iss` claim in issued tokens |
| Database connection refused | Wrong connection string or firewall | Verify host, port, and credentials |
| "State mismatch" error | Clock skew between servers | Ensure NTP is configured; check `ClockSkew` setting |
| Roles not working | Claims not mapped | Check that your OIDC provider includes roles in the token |
| QR codes link to wrong URL | Wrong public origin detected | Set `QrCodeBaseUrl` explicitly to the public URL of your web application |

---

## Mobile Application (Android)

The MAUI Android app is a client-side application — it does not run on a server. Configuration is embedded in `appsettings.json` at build time and must be set before building the APK.

### Configuration

Edit `src/Presentation/BeanShare.Maui/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://api.yourdomain.com",
  "UseOidc": true,
  "Oidc": {
    "Authority": "https://auth.yourdomain.com/realms/beanshare",
    "ClientId": "beanshare-mobile",
    "RedirectUri": "beanshare://callback"
  }
}
```

`beanshare://callback` must match the redirect URI registered in your OIDC provider (Client 3 above).

### Build Requirements

- .NET 10 SDK with MAUI workload (`dotnet workload install maui-android`)
- Android SDK (API 35+)
- Java JDK 21

### Building the APK

```bash
dotnet build src/Presentation/BeanShare.Maui/BeanShare.Maui.csproj \
  -f net10.0-android \
  -c Release \
  -p:EmbedAssembliesIntoApk=true
```

The signed APK is output to `bin/Release/net10.0-android/com.companyname.beanshare.maui-Signed.apk`.

### Installation

Sideload via adb (USB debugging must be enabled on the device):

```bash
adb install -r com.companyname.beanshare.maui-Signed.apk
```

For distribution, the APK can be published to Google Play or distributed as a direct download. The app uses the `beanshare://` custom URL scheme for the OIDC callback, which is handled by `WebAuthenticatorCallbackActivity` and requires no additional server configuration.

---
*Author: Oliver Golec*
