#!/usr/bin/env bash
set -euo pipefail

# Setup Keycloak realm, clients, and test users for BeanShare on Fly.io
# Usage: ./deploy/setup-keycloak-fly.sh <keycloak-url> <admin-password> <web-client-secret> <api-client-secret>
#
# Example:
#   ./deploy/setup-keycloak-fly.sh https://beanshare-keycloak.fly.dev myAdminPass webSecret123 apiSecret456

KC_URL="${1:?Usage: $0 <keycloak-url> <admin-password> <web-client-secret> <api-client-secret>}"
ADMIN_PASS="${2:?Missing admin password}"
WEB_CLIENT_SECRET="${3:?Missing web client secret}"
API_CLIENT_SECRET="${4:?Missing api client secret}"

ADMIN_USER="admin"
REALM="beanshare"
WEB_URL="https://beanshare-web.fly.dev"
API_URL="https://beanshare-api.fly.dev"

echo "=== Setting up Keycloak for BeanShare (Fly.io) ==="
echo "    Keycloak: $KC_URL"
echo "    Web:      $WEB_URL"
echo "    API:      $API_URL"
echo ""

# Wait for Keycloak to be ready
echo "0. Waiting for Keycloak to be ready..."
for i in $(seq 1 30); do
  if curl -sf "$KC_URL/realms/master" >/dev/null 2>&1; then
    echo "   Keycloak is ready!"
    break
  fi
  if [ "$i" -eq 30 ]; then
    echo "   TIMEOUT: Keycloak not ready after 5 minutes"
    exit 1
  fi
  echo "   Waiting... ($i/30)"
  sleep 10
done

# Get admin token
echo "1. Getting admin token..."
TOKEN=$(curl -sf -X POST "$KC_URL/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=$ADMIN_USER" \
  -d "password=$ADMIN_PASS" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" | sed 's/.*"access_token":"\([^"]*\)".*/\1/')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
  echo "   FAILED to get admin token. Is the admin password correct?"
  exit 1
fi
echo "   Token acquired."

# Create realm
echo "2. Creating realm '$REALM'..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$KC_URL/admin/realms" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"realm\": \"$REALM\",
    \"enabled\": true,
    \"registrationAllowed\": true,
    \"loginWithEmailAllowed\": true,
    \"duplicateEmailsAllowed\": false
  }")
echo "   HTTP $HTTP_CODE (201=created, 409=already exists)"

# Create API client
echo "3. Creating 'beanshare-api' client..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$KC_URL/admin/realms/$REALM/clients" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"clientId\": \"beanshare-api\",
    \"enabled\": true,
    \"protocol\": \"openid-connect\",
    \"publicClient\": false,
    \"secret\": \"$API_CLIENT_SECRET\",
    \"directAccessGrantsEnabled\": true,
    \"serviceAccountsEnabled\": true,
    \"standardFlowEnabled\": true,
    \"redirectUris\": [\"$API_URL/*\"],
    \"webOrigins\": [\"$API_URL\"]
  }")
echo "   HTTP $HTTP_CODE"

# Create Web client
echo "4. Creating 'beanshare-web' client..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$KC_URL/admin/realms/$REALM/clients" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"clientId\": \"beanshare-web\",
    \"enabled\": true,
    \"protocol\": \"openid-connect\",
    \"publicClient\": false,
    \"secret\": \"$WEB_CLIENT_SECRET\",
    \"directAccessGrantsEnabled\": true,
    \"standardFlowEnabled\": true,
    \"redirectUris\": [\"$WEB_URL/*\"],
    \"webOrigins\": [\"$WEB_URL\"]
  }")
echo "   HTTP $HTTP_CODE"

# Create Mobile client
echo "5. Creating 'beanshare-mobile' client..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$KC_URL/admin/realms/$REALM/clients" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"clientId\": \"beanshare-mobile\",
    \"enabled\": true,
    \"protocol\": \"openid-connect\",
    \"publicClient\": true,
    \"directAccessGrantsEnabled\": true,
    \"standardFlowEnabled\": true,
    \"redirectUris\": [\"beanshare://callback\", \"http://localhost/*\"],
    \"webOrigins\": [\"*\"]
  }")
echo "   HTTP $HTTP_CODE"

# Create seeded users with matching IDs
create_user() {
  local id=$1 username=$2 email=$3 first=$4 last=$5 password=$6
  echo -n "   Creating $username ($email)... "
  HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$KC_URL/admin/realms/$REALM/users" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
      \"id\": \"$id\",
      \"username\": \"$username\",
      \"email\": \"$email\",
      \"firstName\": \"$first\",
      \"lastName\": \"$last\",
      \"enabled\": true,
      \"emailVerified\": true,
      \"credentials\": [{
        \"type\": \"password\",
        \"value\": \"$password\",
        \"temporary\": false
      }]
    }")
  echo "HTTP $HTTP_CODE"
}

# Enable realm_access in ID token (Keycloak only puts it in access token by default)
echo "6a. Enabling realm roles in ID token..."
ROLES_SCOPE_ID=$(curl -sf "$KC_URL/admin/realms/$REALM/client-scopes" \
  -H "Authorization: Bearer $TOKEN" | grep -B1 '"name":"roles"' | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
if [ -n "$ROLES_SCOPE_ID" ]; then
  REALM_ROLES_MAPPER_ID=$(curl -sf "$KC_URL/admin/realms/$REALM/client-scopes/$ROLES_SCOPE_ID/protocol-mappers/models" \
    -H "Authorization: Bearer $TOKEN" | grep -B1 '"name":"realm roles"' | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
  if [ -n "$REALM_ROLES_MAPPER_ID" ]; then
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X PUT \
      "$KC_URL/admin/realms/$REALM/client-scopes/$ROLES_SCOPE_ID/protocol-mappers/models/$REALM_ROLES_MAPPER_ID" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"id\": \"$REALM_ROLES_MAPPER_ID\",
        \"name\": \"realm roles\",
        \"protocol\": \"openid-connect\",
        \"protocolMapper\": \"oidc-usermodel-realm-role-mapper\",
        \"consentRequired\": false,
        \"config\": {
          \"user.attribute\": \"foo\",
          \"introspection.token.claim\": \"true\",
          \"access.token.claim\": \"true\",
          \"id.token.claim\": \"true\",
          \"claim.name\": \"realm_access.roles\",
          \"jsonType.label\": \"String\",
          \"multivalued\": \"true\"
        }
      }")
    echo "   HTTP $HTTP_CODE (204=updated)"
  else
    echo "   WARNING: Could not find realm roles mapper"
  fi
else
  echo "   WARNING: Could not find roles client scope"
fi

# Create "admin" realm role
echo "6b. Creating 'admin' realm role..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$KC_URL/admin/realms/$REALM/roles" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "admin", "description": "System administrator role"}')
echo "   HTTP $HTTP_CODE (201=created, 409=already exists)"

echo "7. Creating demo users..."
create_user "11111111-1111-1111-1111-111111111111" "john.smith"      "john.smith@beanshare.dev"      "John"   "Smith"    "password123"
create_user "22222222-2222-2222-2222-222222222222" "sarah.johnson"   "sarah.johnson@beanshare.com"   "Sarah"  "Johnson"  "password123"
create_user "33333333-3333-3333-3333-333333333333" "mike.wilson"     "mike.wilson@beanshare.com"     "Mike"   "Wilson"   "password123"
create_user "44444444-4444-4444-4444-444444444444" "emma.davis"      "emma.davis@beanshare.com"      "Emma"   "Davis"    "password123"
create_user "55555555-5555-5555-5555-555555555555" "alex.brown"      "alex.brown@beanshare.com"      "Alex"   "Brown"    "password123"
create_user "66666666-6666-6666-6666-666666666666" "lisa.martinez"   "lisa.martinez@beanshare.com"   "Lisa"   "Martinez" "password123"
create_user "77777777-7777-7777-7777-777777777777" "david.garcia"    "david.garcia@beanshare.com"    "David"  "Garcia"   "password123"
create_user "88888888-8888-8888-8888-888888888888" "test.user"       "test@beanshare.com"            "Test"   "User"     "password123"

# Assign "admin" realm role to John Smith
echo "8. Assigning 'admin' realm role to John Smith..."
JOHN_ID=$(curl -sf "$KC_URL/admin/realms/$REALM/users?username=john.smith&exact=true" \
  -H "Authorization: Bearer $TOKEN" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

if [ -n "$JOHN_ID" ]; then
  ADMIN_ROLE_JSON=$(curl -sf "$KC_URL/admin/realms/$REALM/roles/admin" \
    -H "Authorization: Bearer $TOKEN")
  HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST \
    "$KC_URL/admin/realms/$REALM/users/$JOHN_ID/role-mappings/realm" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "[$ADMIN_ROLE_JSON]")
  echo "   HTTP $HTTP_CODE (204=assigned)"
else
  echo "   WARNING: Could not find john.smith user"
fi

echo ""
echo "=== Keycloak setup complete! ==="
echo ""
echo "Demo credentials (all passwords: password123):"
echo "  john.smith@beanshare.dev    - Admin (Engineering Team, Executive Lounge)"
echo "  sarah.johnson@beanshare.com - Member (Engineering Team, Marketing Office)"
echo "  test@beanshare.com          - Test User"
echo ""
echo "Keycloak Admin: $KC_URL/admin (admin / $ADMIN_PASS)"
echo "Realm: $REALM"
