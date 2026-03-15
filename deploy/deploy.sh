#!/usr/bin/env bash
set -euo pipefail

# BeanShare Fly.io Deployment Script (with Keycloak)
# Usage: ./deploy/deploy.sh <command>
#
# Commands:
#   setup            - Create Fly apps and Postgres cluster
#   deploy-keycloak  - Build and deploy Keycloak
#   setup-keycloak   - Configure Keycloak realm, clients, users
#   deploy-api       - Build and deploy the API
#   deploy-web       - Build and deploy BlazorWeb
#   deploy-all       - Deploy everything (keycloak + api + web)
#   full             - Full setup + deploy (first time)
#   status           - Show deployment status
#   secrets          - Show generated secrets file path

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

FLY_REGION="${FLY_REGION:-fra}"
DB_NAME="beanshare-db"
WEB_APP="beanshare-web"
API_APP="beanshare-api"
KC_APP="beanshare-keycloak"
SECRETS_FILE="$SCRIPT_DIR/.secrets.env"

# ── Helpers ──────────────────────────────────────────────

command_exists() { command -v "$1" &>/dev/null; }

find_flyctl() {
  if command_exists flyctl; then
    FLYCTL="flyctl"
  elif command_exists fly; then
    FLYCTL="fly"
  elif [ -f "$HOME/.fly/bin/flyctl.exe" ]; then
    FLYCTL="$HOME/.fly/bin/flyctl.exe"
  elif [ -f "$HOME/.fly/bin/flyctl" ]; then
    FLYCTL="$HOME/.fly/bin/flyctl"
  else
    echo "ERROR: flyctl not found."
    echo ""
    echo "Install flyctl:"
    echo "  Windows (PowerShell): irm https://fly.io/install.ps1 | iex"
    echo "  macOS/Linux:          curl -L https://fly.io/install.sh | sh"
    echo ""
    echo "Then sign up:  flyctl auth signup"
    echo "Or log in:     flyctl auth login"
    exit 1
  fi
}

check_flyctl() {
  find_flyctl
  if ! $FLYCTL auth whoami &>/dev/null 2>&1; then
    echo "Not logged in to Fly.io."
    echo ""
    echo "Run one of:"
    echo "  $FLYCTL auth signup   # Create new account"
    echo "  $FLYCTL auth login    # Log in to existing account"
    echo ""
    echo "Then re-run this script."
    exit 1
  fi
  echo "Logged in as: $($FLYCTL auth whoami 2>/dev/null)"
}

generate_secret() {
  # Generate a random alphanumeric string
  openssl rand -base64 "$1" 2>/dev/null | tr -d '=+/' | head -c "$1" \
    || cat /dev/urandom | LC_ALL=C tr -dc 'a-zA-Z0-9' | head -c "$1"
}

# Parse postgres:// URL into PG_USER, PG_PASS, PG_HOST, PG_PORT, PG_DB
parse_postgres_url() {
  local url="$1"
  local rest="${url#postgres://}"
  local userpass="${rest%%@*}"
  local hostportdb="${rest#*@}"
  PG_USER="${userpass%%:*}"
  PG_PASS="${userpass#*:}"
  local hostport="${hostportdb%%/*}"
  local dbparams="${hostportdb#*/}"
  PG_HOST="${hostport%%:*}"
  PG_PORT="${hostport#*:}"
  PG_DB="${dbparams%%\?*}"
}

save_secret() {
  local key="$1"
  local value="$2"
  # Create file if it doesn't exist
  touch "$SECRETS_FILE"
  # Remove existing key if present
  if grep -q "^${key}=" "$SECRETS_FILE" 2>/dev/null; then
    sed -i "s|^${key}=.*|${key}=${value}|" "$SECRETS_FILE"
  else
    echo "${key}=${value}" >> "$SECRETS_FILE"
  fi
}

load_secret() {
  local key="$1"
  if [ -f "$SECRETS_FILE" ]; then
    grep "^${key}=" "$SECRETS_FILE" 2>/dev/null | cut -d= -f2- || echo ""
  else
    echo ""
  fi
}

# ── Setup ────────────────────────────────────────────────

cmd_setup() {
  check_flyctl

  echo ""
  echo "╔══════════════════════════════════════════════╗"
  echo "║  BeanShare Fly.io Setup                     ║"
  echo "║  Creating apps and Postgres cluster          ║"
  echo "╚══════════════════════════════════════════════╝"
  echo ""

  # Generate secrets
  local kc_admin_pass
  kc_admin_pass=$(load_secret "KC_ADMIN_PASSWORD")
  if [ -z "$kc_admin_pass" ]; then
    kc_admin_pass=$(generate_secret 20)
    save_secret "KC_ADMIN_PASSWORD" "$kc_admin_pass"
  fi

  local web_client_secret
  web_client_secret=$(load_secret "WEB_CLIENT_SECRET")
  if [ -z "$web_client_secret" ]; then
    web_client_secret=$(generate_secret 24)
    save_secret "WEB_CLIENT_SECRET" "$web_client_secret"
  fi

  local api_client_secret
  api_client_secret=$(load_secret "API_CLIENT_SECRET")
  if [ -z "$api_client_secret" ]; then
    api_client_secret=$(generate_secret 24)
    save_secret "API_CLIENT_SECRET" "$api_client_secret"
  fi

  echo "Generated secrets saved to: $SECRETS_FILE"
  echo "(Keep this file safe - it contains your deployment passwords)"
  echo ""

  # Create apps
  for app in "$KC_APP" "$API_APP" "$WEB_APP"; do
    echo "==> Creating app: $app"
    if $FLYCTL apps list 2>/dev/null | grep -q "$app"; then
      echo "    Already exists, skipping."
    else
      $FLYCTL apps create "$app" --org personal || {
        echo "    FAILED: App name '$app' may already be taken globally."
        echo "    Try a different name by editing this script."
        exit 1
      }
    fi
  done

  # Create Postgres cluster
  echo ""
  echo "==> Creating Postgres cluster: $DB_NAME"
  if $FLYCTL postgres list 2>/dev/null | grep -q "$DB_NAME"; then
    echo "    Already exists, skipping."
  else
    $FLYCTL postgres create \
      --name "$DB_NAME" \
      --region "$FLY_REGION" \
      --vm-size shared-cpu-1x \
      --initial-cluster-size 1 \
      --volume-size 1
  fi

  # Attach Postgres to web app (creates a database)
  echo ""
  echo "==> Attaching Postgres to $WEB_APP..."
  local attach_output
  attach_output=$($FLYCTL postgres attach "$DB_NAME" --app "$WEB_APP" 2>&1) || {
    if echo "$attach_output" | grep -qi "already"; then
      echo "    Already attached."
    else
      echo "$attach_output"
      echo "    WARNING: Attach may have failed. You may need to set ConnectionStrings__DefaultConnection manually."
    fi
  }

  # Extract DATABASE_URL from attach output
  local db_url
  db_url=$(echo "$attach_output" | grep -o 'postgres://[^ ]*' | head -1) || true
  if [ -n "$db_url" ]; then
    parse_postgres_url "$db_url"
    local conn_str="Host=$PG_HOST;Port=$PG_PORT;Database=$PG_DB;Username=$PG_USER;Password=$PG_PASS;SSL Mode=Disable"
    save_secret "DATABASE_URL" "$db_url"
    save_secret "DOTNET_CONN_STR" "$conn_str"
    echo "    Connection string captured."

    # Set for both web and API (they share the same database)
    echo "==> Setting database connection for $WEB_APP and $API_APP..."
    $FLYCTL secrets set --app "$WEB_APP" "ConnectionStrings__DefaultConnection=$conn_str" --stage
    $FLYCTL secrets set --app "$API_APP" "ConnectionStrings__DefaultConnection=$conn_str" --stage
  else
    echo "    WARNING: Could not parse DATABASE_URL from attach output."
    echo "    You may need to set ConnectionStrings__DefaultConnection manually for web and api apps."
    echo "    Attach output was: $attach_output"
  fi

  # Attach Postgres to Keycloak
  echo ""
  echo "==> Attaching Postgres to $KC_APP..."
  local kc_attach_output
  kc_attach_output=$($FLYCTL postgres attach "$DB_NAME" --app "$KC_APP" 2>&1) || {
    if echo "$kc_attach_output" | grep -qi "already"; then
      echo "    Already attached."
    else
      echo "$kc_attach_output"
    fi
  }

  local kc_db_url
  kc_db_url=$(echo "$kc_attach_output" | grep -o 'postgres://[^ ]*' | head -1) || true
  if [ -n "$kc_db_url" ]; then
    parse_postgres_url "$kc_db_url"
    save_secret "KC_DATABASE_URL" "$kc_db_url"

    echo "==> Setting Keycloak database secrets..."
    $FLYCTL secrets set --app "$KC_APP" \
      "KC_DB_URL=jdbc:postgresql://$PG_HOST:$PG_PORT/$PG_DB" \
      "KC_DB_USERNAME=$PG_USER" \
      "KC_DB_PASSWORD=$PG_PASS" \
      "KC_HOSTNAME=$KC_APP.fly.dev" \
      "KEYCLOAK_ADMIN=admin" \
      "KEYCLOAK_ADMIN_PASSWORD=$kc_admin_pass" \
      --stage
  else
    echo "    WARNING: Could not parse Keycloak DATABASE_URL."
    echo "    Attach output was: $kc_attach_output"
  fi

  # Set OIDC secrets for web and API
  local kc_url="https://$KC_APP.fly.dev"

  echo ""
  echo "==> Setting OIDC secrets for $WEB_APP..."
  $FLYCTL secrets set --app "$WEB_APP" \
    "Oidc__Authority=$kc_url/realms/beanshare" \
    "Oidc__ClientId=beanshare-web" \
    "Oidc__ClientSecret=$web_client_secret" \
    "ApiBaseUrl=https://$API_APP.fly.dev" \
    --stage

  echo "==> Setting OIDC secrets for $API_APP..."
  $FLYCTL secrets set --app "$API_APP" \
    "Oidc__Authority=$kc_url/realms/beanshare" \
    "Oidc__Audience=beanshare-api" \
    "Oidc__ClientId=beanshare-api" \
    "Oidc__ClientSecret=$api_client_secret" \
    --stage

  echo ""
  echo "=== Setup complete! ==="
  echo ""
  echo "Secrets file: $SECRETS_FILE"
  echo "  Keycloak admin password: $kc_admin_pass"
  echo "  Web client secret:       $web_client_secret"
  echo "  API client secret:       $api_client_secret"
  echo ""
  echo "Next step: ./deploy/deploy.sh deploy-all"
}

# ── Deploy Keycloak ──────────────────────────────────────

deploy_keycloak() {
  echo "==> Deploying Keycloak ($KC_APP)"

  $FLYCTL deploy \
    --app "$KC_APP" \
    --config deploy/fly-keycloak.toml \
    --dockerfile deploy/Dockerfile.keycloak \
    --remote-only

  echo "==> Keycloak deployed: https://$KC_APP.fly.dev"
  echo "    Admin console: https://$KC_APP.fly.dev/admin"
}

# ── Setup Keycloak realm ─────────────────────────────────

cmd_setup_keycloak() {
  check_flyctl

  local kc_admin_pass
  kc_admin_pass=$(load_secret "KC_ADMIN_PASSWORD")
  if [ -z "$kc_admin_pass" ]; then
    echo "ERROR: KC_ADMIN_PASSWORD not found in $SECRETS_FILE"
    echo "Run './deploy/deploy.sh setup' first."
    exit 1
  fi

  local web_client_secret
  web_client_secret=$(load_secret "WEB_CLIENT_SECRET")
  local api_client_secret
  api_client_secret=$(load_secret "API_CLIENT_SECRET")

  echo "==> Configuring Keycloak realm and clients..."
  bash "$SCRIPT_DIR/setup-keycloak-fly.sh" \
    "https://$KC_APP.fly.dev" \
    "$kc_admin_pass" \
    "$web_client_secret" \
    "$api_client_secret"
}

# ── Deploy API ───────────────────────────────────────────

deploy_api() {
  echo "==> Deploying API ($API_APP)"

  $FLYCTL deploy \
    --app "$API_APP" \
    --config deploy/fly-api.toml \
    --dockerfile deploy/Dockerfile.api \
    --remote-only

  echo "==> API deployed: https://$API_APP.fly.dev"
}

# ── Deploy BlazorWeb ─────────────────────────────────────

deploy_web() {
  echo "==> Deploying BlazorWeb ($WEB_APP)"

  $FLYCTL deploy \
    --app "$WEB_APP" \
    --config deploy/fly-web.toml \
    --dockerfile deploy/Dockerfile.web \
    --remote-only

  echo "==> BlazorWeb deployed: https://$WEB_APP.fly.dev"
}

# ── Deploy All ───────────────────────────────────────────

cmd_deploy_all() {
  check_flyctl

  echo ""
  echo "╔══════════════════════════════════════════════╗"
  echo "║  BeanShare Fly.io Deployment                ║"
  echo "║  Keycloak + API + BlazorWeb                 ║"
  echo "╚══════════════════════════════════════════════╝"
  echo ""

  # 1. Deploy Keycloak first
  deploy_keycloak

  # 2. Setup Keycloak realm (wait for health + configure)
  echo ""
  cmd_setup_keycloak

  # 3. Deploy API
  echo ""
  deploy_api

  # 4. Deploy BlazorWeb
  echo ""
  deploy_web

  echo ""
  echo "╔══════════════════════════════════════════════╗"
  echo "║  Deployment Complete!                       ║"
  echo "╚══════════════════════════════════════════════╝"
  echo ""
  echo "  BlazorWeb:      https://$WEB_APP.fly.dev"
  echo "  API:            https://$API_APP.fly.dev"
  echo "  Keycloak Admin: https://$KC_APP.fly.dev/admin"
  echo ""
  echo "  Keycloak admin password: $(load_secret KC_ADMIN_PASSWORD)"
  echo ""
  echo "  Demo logins (password: password123):"
  echo "    john.smith@beanshare.dev    - Admin"
  echo "    sarah.johnson@beanshare.com - Member"
  echo "    test@beanshare.com          - Test User"
}

# ── Full (first time) ───────────────────────────────────

cmd_full() {
  cmd_setup
  echo ""
  cmd_deploy_all
}

# ── Status ───────────────────────────────────────────────

cmd_status() {
  check_flyctl
  echo ""
  for app in "$KC_APP" "$API_APP" "$WEB_APP"; do
    echo "=== $app ==="
    $FLYCTL status --app "$app" 2>/dev/null || echo "(not deployed)"
    echo ""
  done
}

# ── Secrets ──────────────────────────────────────────────

cmd_secrets() {
  if [ -f "$SECRETS_FILE" ]; then
    echo "Secrets file: $SECRETS_FILE"
    echo ""
    cat "$SECRETS_FILE"
  else
    echo "No secrets file found. Run './deploy/deploy.sh setup' first."
  fi
}

# ── Main ─────────────────────────────────────────────────

case "${1:-help}" in
  setup)            cmd_setup ;;
  deploy-keycloak)  check_flyctl; deploy_keycloak ;;
  setup-keycloak)   cmd_setup_keycloak ;;
  deploy-api)       check_flyctl; deploy_api ;;
  deploy-web)       check_flyctl; deploy_web ;;
  deploy-all)       cmd_deploy_all ;;
  full)             cmd_full ;;
  status)           cmd_status ;;
  secrets)          cmd_secrets ;;
  *)
    echo "BeanShare Fly.io Deployment Script"
    echo ""
    echo "Usage: $0 <command>"
    echo ""
    echo "Commands:"
    echo "  full             First-time setup + deploy everything"
    echo "  setup            Create Fly apps, Postgres, and configure secrets"
    echo "  deploy-all       Deploy Keycloak + API + BlazorWeb"
    echo "  deploy-keycloak  Deploy only Keycloak"
    echo "  setup-keycloak   Configure Keycloak realm/clients/users"
    echo "  deploy-api       Deploy only API"
    echo "  deploy-web       Deploy only BlazorWeb"
    echo "  status           Show deployment status"
    echo "  secrets          Show generated secrets"
    echo ""
    echo "First-time deployment:"
    echo "  1. flyctl auth signup    # Create Fly.io account"
    echo "  2. flyctl auth login     # Authenticate"
    echo "  3. ./deploy/deploy.sh full  # Deploy everything"
    ;;
esac
