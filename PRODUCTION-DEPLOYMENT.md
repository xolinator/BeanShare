# BeanShare Production Deployment Guide

## Deployment Architecture

### Required Services

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

## Step-by-Step Deployment

### 1. Prerequisites

```bash
# Required software
- Docker & Docker Compose
- .NET 10 SDK (or use containerized build)
- PostgreSQL 15+ (or Docker container)
- Keycloak 23+ (or Docker container)
- Reverse proxy (NGINX, Traefik, or cloud load balancer)
- SSL certificates (Let's Encrypt or commercial)
```

### 2. Environment Configuration

Create production environment files:

```bash
# keycloak/.env.production
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=<STRONG_PASSWORD>
KC_DB=postgres
KC_DB_URL=jdbc:postgresql://postgres:5432/keycloak
KC_DB_USERNAME=keycloak
KC_DB_PASSWORD=<STRONG_PASSWORD>
KC_HOSTNAME=auth.yourdomain.com
KC_PROXY=edge

# BeanShare appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=beanshare;Username=beanshare;Password=<STRONG_PASSWORD>"
  },
  "Keycloak": {
    "Authority": "https://auth.yourdomain.com/realms/beanshare",
    "ClientId": "beanshare-web",
    "ClientSecret": "<PRODUCTION_SECRET>"
  }
}
```

### 3. Database Setup

```bash
# Create production database
docker exec -it postgres psql -U postgres -c "CREATE DATABASE beanshare;"
docker exec -it postgres psql -U postgres -c "CREATE USER beanshare WITH PASSWORD '<PASSWORD>';"
docker exec -it postgres psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE beanshare TO beanshare;"
```

The application automatically creates the database schema on first startup via `EnsureCreatedAsync()`.

### Database Seeding

The application uses an environment-aware seeding system. Each seeder implements `IDataSeeder` with an `IsEssential` flag that controls whether it runs in production or only in development.

| Seeder | Essential | Description |
|--------|-----------|-------------|
| `GlobalPresetSeeder` | Yes | 12 coffee recipe presets (Espresso, Cappuccino, Pour Over, etc.) |
| `UserSeeder` | No | 8 demo users with hardcoded passwords |
| `SpaceSeeder` | No | 5 demo spaces (Engineering Team, Marketing Office, etc.) |
| `CoffeeStockSeeder` | No | Demo coffee stock purchases |
| `ConsumptionSeeder` | No | Demo consumption entries |
| `BillingPeriodSeeder` | No | Demo billing periods |

**Production** — Only essential seeders run. The 12 global coffee presets are seeded automatically on first startup. No demo users or fake data are created.

**Development** — All seeders run, populating the database with demo users, spaces, stock, consumption entries, and billing periods for local testing.

The admin user in production is managed through Keycloak. When a user with the `admin` realm role logs in for the first time, their profile is automatically synchronized to the database by `UserSynchronizationService`.

### 4. Keycloak Configuration

```bash
# Import the realm configuration
# 1. Access Keycloak admin console: https://auth.yourdomain.com/admin
# 2. Create realm "beanshare" or import keycloak/realm-export.json
# 3. Update client secrets for production
# 4. Configure SSL/HTTPS redirect URIs
# 5. Optionally add external identity providers (see SSO section below)
```

### 5. Build and Deploy Blazor App

```bash
# Build for production
cd BeanShare/src/Presentation/BeanShare.BlazorWeb
dotnet publish -c Release -o ./publish

# Or build Docker image
docker build -t beanshare-web:latest .
docker run -d -p 5000:80 -e ASPNETCORE_ENVIRONMENT=Production beanshare-web:latest
```

### 6. Docker Compose Production

```yaml
# docker-compose.production.yml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: beanshare
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: beanshare
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: always

  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: keycloak
      KC_DB_PASSWORD: ${KC_DB_PASSWORD}
      KC_HOSTNAME: auth.yourdomain.com
      KC_PROXY: edge
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: ${KC_ADMIN_PASSWORD}
    command: start
    depends_on:
      - postgres
    restart: always

  beanshare-web:
    image: beanshare-web:latest
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: Host=postgres;Database=beanshare;Username=beanshare;Password=${DB_PASSWORD}
    depends_on:
      - postgres
      - keycloak
    restart: always

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./certs:/etc/nginx/certs
    depends_on:
      - beanshare-web
      - keycloak
    restart: always

volumes:
  postgres_data:
```

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

- [ ] Change all default passwords
- [ ] Enable HTTPS everywhere
- [ ] Configure proper CORS origins
- [ ] Enable Keycloak brute force protection (already configured)
- [ ] Set up database backups
- [ ] Configure rate limiting on reverse proxy
- [ ] Review and restrict redirect URIs
- [ ] Enable audit logging
- [ ] Set up monitoring and alerting

---

## Production URLs

After deployment, your services will be available at:

| Service | URL |
|---------|-----|
| BeanShare Web | https://yourdomain.com |
| Keycloak Admin | https://auth.yourdomain.com/admin |
| Keycloak Auth | https://auth.yourdomain.com/realms/beanshare |

---

## Support and Maintenance

### Health Checks

```bash
# Check PostgreSQL
docker exec postgres pg_isready -U beanshare

# Check Keycloak
curl -s https://auth.yourdomain.com/health | jq

# Check BeanShare
curl -s https://yourdomain.com/health
```

### Backup Commands

```bash
# Database backup
docker exec postgres pg_dump -U beanshare beanshare > backup_$(date +%Y%m%d).sql

# Keycloak realm export
docker exec keycloak /opt/keycloak/bin/kc.sh export --dir /tmp/export --realm beanshare
```

---

## Troubleshooting

### Common Issues

1. **"OIDC callback error"**: Check redirect URIs in Keycloak client configuration
2. **"Database connection failed"**: Verify connection string and PostgreSQL is running
3. **"SSL certificate error"**: Ensure certificates are valid and properly mounted
4. **"User not synced"**: Check `OnTokenValidated` event in Program.cs

### Logs

```bash
# View BeanShare logs
docker logs -f beanshare-web

# View Keycloak logs
docker logs -f keycloak

# View PostgreSQL logs
docker logs -f postgres
```

---

## Version Information

- **BeanShare**: v1.0.0 (Diploma Thesis)
- **.NET**: 10.0 Preview
- **Keycloak**: 23.0
- **PostgreSQL**: 15
- **Blazor**: Server-side rendering

---

*Last Updated: February 24, 2026*
*Author: Oliver Golec*
