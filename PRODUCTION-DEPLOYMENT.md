# BeanShare Production Deployment Guide

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         PRODUCTION                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │   Keycloak   │    │  PostgreSQL  │    │  BeanShare   │       │
│  │   (Auth)     │    │  (Database)  │    │  Blazor Web  │       │
│  │   Port 8080  │    │  Port 5432   │    │  Port 443    │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│         │                   │                   │                │
│         └───────────────────┴───────────────────┘                │
│                         │                                        │
│                  ┌──────────────┐                                │
│                  │   NGINX /    │                                │
│                  │   Traefik    │                                │
│                  │   (Reverse   │                                │
│                  │    Proxy)    │                                │
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
- PostgreSQL 15+ (or Docker container)
- Keycloak 23+ (or Docker container)
- Reverse proxy (NGINX, Traefik, or cloud load balancer)
- SSL certificates (Let's Encrypt or commercial)

---

## Step-by-Step Deployment

### 1. Database Setup

Use a managed PostgreSQL service (e.g., AWS RDS, Azure Database) or run it in Docker.

```sql
CREATE DATABASE beanshare;
CREATE USER beanshare WITH PASSWORD '<STRONG_PASSWORD>';
GRANT ALL PRIVILEGES ON DATABASE beanshare TO beanshare;
```

The application automatically creates the database schema on first startup via `EnsureCreatedAsync()`. Ensure the database user has CREATE TABLE permissions.

### 2. Keycloak Setup

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

**Important**: Production Keycloak uses `start` (not `start-dev`), requires HTTPS, and uses a real database.

After Keycloak starts, configure the realm:

1. Open the Keycloak Admin Console at `https://auth.yourdomain.com/admin`
2. Create a new realm named `beanshare`
3. Import `scripts/keycloak-realm.json` via Realm Settings > Partial Import
4. Update the client redirect URIs to match your production domain:
   - `beanshare-web`: `https://yourdomain.com/*`
   - `beanshare-api`: `https://api.yourdomain.com/*`
   - `beanshare-mobile`: `beanshare://callback`
5. Change client secrets for `beanshare-web` and `beanshare-api`
6. Delete the demo users imported from the realm JSON (they are for development only)
7. Create a real admin user and assign the `admin` realm role
8. Enable user self-registration if desired (Realm Settings > Login > User registration)

### 3. Build and Deploy the API

```bash
cd src/Presentation/BeanShare.Api
dotnet publish -c Release -o ./publish
```

Configure via environment variables:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=DB_HOST;Port=5432;Database=beanshare;Username=beanshare;Password=<DB_PASSWORD>"
export UseOidc=true
export Oidc__Authority="https://auth.yourdomain.com/realms/beanshare"
export Oidc__ClientSecret="<API_CLIENT_SECRET>"
export Jwt__Secret="<RANDOM_SECRET_MIN_32_CHARS>"

# Optional — without this, constant fallback exchange rates are used
export OpenExchangeRates__AppId="<API_KEY>"

# Optional — email notifications
export Email__Enabled=true
export Email__SmtpUsername="<SMTP_USER>"
export Email__SmtpPassword="<SMTP_PASSWORD>"
```

Run:

```bash
cd publish && dotnet BeanShare.Api.dll
```

The API starts on port 5247 by default. Use a reverse proxy for HTTPS.

### 4. Build and Deploy the Blazor Web App

```bash
cd src/Presentation/BeanShare.BlazorWeb
dotnet publish -c Release -o ./publish
```

Configure via environment variables:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=DB_HOST;Port=5432;Database=beanshare;Username=beanshare;Password=<DB_PASSWORD>"
export UseOidc=true
export Oidc__Authority="https://auth.yourdomain.com/realms/beanshare"
export Oidc__ClientId="beanshare-web"
export Oidc__ClientSecret="<WEB_CLIENT_SECRET>"
```

Run:

```bash
cd publish && dotnet BeanShare.BlazorWeb.dll
```

The web app starts on port 5126. Use a reverse proxy for HTTPS.

### 5. Reverse Proxy (Nginx Example)

Blazor Server uses WebSockets — the `Upgrade` and `Connection` headers are required.

```nginx
server {
    listen 443 ssl;
    server_name yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5126;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 443 ssl;
    server_name api.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/api.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5247;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Database Seeding

On first startup, the application seeds **only essential data** in production:

- **12 global coffee presets** (Espresso, Cappuccino, Pour Over, AeroPress, etc.)

No demo users, spaces, or fake consumption data are created. Demo seeders only run in the Development environment (see [TEST.md](BeanShare/TEST.md) for local testing).

The admin user is managed through Keycloak. When a user with the `admin` realm role logs in for the first time, `UserSynchronizationService` automatically creates their profile in the database.

---

## External SSO Integration

Keycloak supports adding external identity providers (IdPs) for single sign-on. This is useful for integrating with university or corporate authentication systems.

### Supported Protocols

- **SAML 2.0** — common in academic environments (e.g., Shibboleth federations)
- **OpenID Connect** — modern OAuth 2.0-based alternative

### General Setup Steps

1. In the Keycloak Admin Console, navigate to `Identity Providers > Add provider`
2. Select the protocol used by the external IdP (SAML v2.0 or OpenID Connect v1.0)
3. Configure the IdP's metadata URL or endpoint URLs (SSO URL, token URL, etc.)
4. Set up attribute mapping so that external user attributes (email, name) map to Keycloak user fields
5. Provide your Service Provider (SP) metadata to the external IdP administrator for registration:
   - SAML SP descriptor: `https://auth.yourdomain.com/realms/beanshare/protocol/saml/descriptor`
   - OIDC redirect URI: `https://auth.yourdomain.com/realms/beanshare/broker/{alias}/endpoint`
6. Test the integration, then enable user self-registration or first-broker-login flows as needed

Refer to the [Keycloak Identity Broker documentation](https://www.keycloak.org/docs/latest/server_admin/#_identity_broker) for detailed configuration instructions.

---

## Security Checklist

- [ ] Change all default passwords (Keycloak admin, database, client secrets)
- [ ] Generate a cryptographically random JWT secret (min 32 characters)
- [ ] Enable HTTPS on all services
- [ ] Restrict `AllowedHosts` in appsettings to your actual domain
- [ ] Update Keycloak client redirect URIs to production URLs
- [ ] Delete demo users from Keycloak
- [ ] Store all secrets in environment variables, not config files
- [ ] Set up database backups
- [ ] Configure firewall rules (only expose ports 443 publicly)

---

## Maintenance

### Health Checks

```bash
# Check PostgreSQL
docker exec postgres pg_isready -U beanshare

# Check Keycloak
curl -s https://auth.yourdomain.com/health | jq

# Check BeanShare
curl -s https://yourdomain.com/health
```

### Backups

```bash
# Database backup
docker exec postgres pg_dump -U beanshare beanshare > backup_$(date +%Y%m%d).sql

# Keycloak realm export
docker exec keycloak /opt/keycloak/bin/kc.sh export --dir /tmp/export --realm beanshare
```

### Logs

```bash
docker logs -f beanshare-web
docker logs -f keycloak
docker logs -f postgres
```

---

## Troubleshooting

1. **"OIDC callback error"** — Check redirect URIs in Keycloak client configuration
2. **"Database connection failed"** — Verify connection string and PostgreSQL is running
3. **"SSL certificate error"** — Ensure certificates are valid and properly mounted
4. **"User not synced"** — Check `OnTokenValidated` event in Program.cs

---

*Last Updated: February 25, 2026*
*Author: Oliver Golec*
