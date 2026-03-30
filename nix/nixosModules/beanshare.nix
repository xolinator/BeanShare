# NixOS module for deploying BeanShare Blazor web app.
# Use: self.nixosModules.beanshare (services.beanshare-blazorweb.package defaults to config.packages.blazorwebapp from this flake).
# Database and OIDC (Keycloak-compatible) config are passed as environment variables matching appsettings.

{ moduleWithSystem, ... }:
{
  flake.nixosModules.beanshare = moduleWithSystem (
    perSystem @ { inputs', ... }: nixos @ { pkgs, config, lib, system, ... }:
      let
        cfg = config.services.beanshare-blazorweb;
        packageContentRoot = "${cfg.package}/lib/beanshare-blazorwebapp";
        apiCfg =
          if builtins.hasAttr "beanshare-api" config.services
          then config.services."beanshare-api"
          else null;
        # When using local PostgreSQL, use its port and localhost; otherwise use database.* options.
        dbHost = if cfg.database.postgresql.enable then "localhost" else cfg.database.host;
        dbPort = if cfg.database.postgresql.enable then config.services.postgresql.settings.port else cfg.database.port;
        # Build connection string from structured options (PostgreSQL-style; app expects this format).
        dbConnectionString =
          if cfg.database.connectionString != null
          then cfg.database.connectionString
          else "Host=${dbHost};Port=${toString dbPort};Database=${cfg.database.name};Username=${cfg.database.user};Password=${cfg.database.password}";
        useDatabase = cfg.database.enable || cfg.database.postgresql.enable;
        baseEnv = {
          ASPNETCORE_ENVIRONMENT = cfg.environment;
          ASPNETCORE_URLS = "http://${cfg.listenAddress}:${toString cfg.port}";
          UseOidc = if cfg.oidc.enable then "true" else "false";
        } // lib.optionalAttrs (cfg.apiBaseUrl != null) {
          ApiBaseUrl = cfg.apiBaseUrl;
        } // lib.optionalAttrs (cfg.qrCodeBaseUrl != null) {
          QrCodeBaseUrl = cfg.qrCodeBaseUrl;
        };
        dbEnv = lib.optionalAttrs useDatabase {
          ConnectionStrings__DefaultConnection = dbConnectionString;
        };
        oidcEnv = lib.optionalAttrs (cfg.oidc.enable && cfg.oidc.authority != "") {
          Oidc__Authority = cfg.oidc.authority;
          Oidc__ClientId = cfg.oidc.clientId;
          Oidc__ClientSecret = cfg.oidc.clientSecret;
        } // lib.optionalAttrs (cfg.oidc.enable && cfg.oidc.registrationEndpoint != null) {
          Oidc__RegistrationEndpoint = cfg.oidc.registrationEndpoint;
        } // lib.optionalAttrs (cfg.oidc.enable && cfg.oidc.identityProviderHintParam != null) {
          Oidc__IdentityProviderHintParam = cfg.oidc.identityProviderHintParam;
        };
        serviceEnvironment = baseEnv // dbEnv // oidcEnv;
        apiProxyEnabled = cfg.nginx.proxyApi.enable;
        apiProxyUpstream =
          if apiCfg == null
          then null
          else "http://${apiCfg.listenAddress}:${toString apiCfg.port}";
        proxyHeaderConfig = ''
          proxy_set_header Host $host;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Host $http_host;
          proxy_set_header X-Forwarded-Proto $scheme;
        '';
      in
      with lib;
      {
        options.services.beanshare-blazorweb = {
          enable = mkEnableOption "BeanShare Blazor web application";

          package = mkOption {
            type = types.package;
            description = "BeanShare Blazor package (defaults to the flake's per-system blazorwebapp package; can be overridden, e.g. with pkgs.blazorwebapp).";
            example = "pkgs.blazorwebapp";
          };

          port = mkOption {
            type = types.port;
            default = 5000;
            description = "Port on which Kestrel will listen (when not using nginx, bind to 0.0.0.0).";
          };

          listenAddress = mkOption {
            type = types.str;
            default = "127.0.0.1";
            description = "Address Kestrel binds to. Use 0.0.0.0 to allow direct access; use 127.0.0.1 when behind nginx.";
            example = "127.0.0.1";
          };

          openFirewall = mkOption {
            type = types.bool;
            default = false;
            description = "Open the configured port in the firewall (only relevant when listenAddress is 0.0.0.0).";
          };

          environment = mkOption {
            type = types.str;
            default = "Production";
            description = "ASPNETCORE_ENVIRONMENT value.";
          };

          apiBaseUrl = mkOption {
            type = types.nullOr types.str;
            default = null;
            description = "Base URL of the BeanShare API consumed by the web app.";
            example = "http://127.0.0.1:5247";
          };

          qrCodeBaseUrl = mkOption {
            type = types.nullOr types.str;
            default = null;
            description = "Public base URL used when generating QR code links.";
            example = "http://localhost:5000";
          };

          database = {
            enable = mkOption {
              type = types.bool;
              default = false;
              description = "Pass database connection to the app (ConnectionStrings:DefaultConnection). Set to true when using an external DB; when database.postgresql.enable is true this is implied.";
            };

            postgresql = {
              enable = mkOption {
                type = types.bool;
                default = true;
                description = "Enable and use the NixOS PostgreSQL service. Ensures database and user exist; app uses this instance by default.";
              };
            };

            connectionString = mkOption {
              type = types.nullOr types.str;
              default = null;
              description = "Full connection string. If set, overrides host/port/name/user/password.";
              example = "Host=localhost;Database=beanshare;Username=beanshare;Password=secret";
            };

            host = mkOption {
              type = types.str;
              default = "localhost";
              description = "Database host.";
            };

            port = mkOption {
              type = types.port;
              default = 5432;
              description = "Database port (e.g. 5432 for PostgreSQL).";
            };

            name = mkOption {
              type = types.str;
              default = "beanshare";
              description = "Database name.";
            };

            user = mkOption {
              type = types.str;
              default = "beanshare";
              description = "Database user.";
            };

            password = mkOption {
              type = types.str;
              default = "";
              description = "Database password. Empty is valid (e.g. for local/dev). Prefer environmentFile for secrets to avoid storing in Nix.";
            };
          };

          oidc = {
            enable = mkOption {
              type = types.bool;
              default = false;
              description = "Enable OIDC authentication. Sets UseOidc and Oidc:* config.";
            };

            authority = mkOption {
              type = types.str;
              default = "";
              description = "OIDC authority URL (e.g. https://auth.example.com/realms/beanshare).";
              example = "https://keycloak.example.com/realms/beanshare";
            };

            clientId = mkOption {
              type = types.str;
              default = "beanshare-web";
              description = "OIDC client ID.";
            };

            clientSecret = mkOption {
              type = types.str;
              default = "";
              description = "OIDC client secret. Prefer environmentFile for secrets to avoid storing in Nix.";
            };

            registrationEndpoint = mkOption {
              type = types.nullOr types.str;
              default = null;
              description = "Optional OIDC registration endpoint used by the web registration page. When unset, BeanShare derives a Keycloak-compatible endpoint from authority.";
              example = "https://auth.example.com/realms/beanshare/protocol/openid-connect/registrations";
            };

            identityProviderHintParam = mkOption {
              type = types.nullOr types.str;
              default = null;
              description = "Optional authorization request parameter used to force a specific upstream identity provider, for example kc_idp_hint for Keycloak or login_hint for some brokers.";
              example = "kc_idp_hint";
            };

          };

          environmentFile = mkOption {
            type = types.nullOr types.str;
            default = null;
            description = "Path to a file loaded as systemd EnvironmentFile (e.g. for ConnectionStrings and OIDC client secret). Overrides env vars set from database/oidc options.";
            example = "/run/secrets/beanshare-blazorweb.env";
          };

          nginx = {
            enable = mkOption {
              type = types.bool;
              default = false;
              description = "Configure nginx as reverse proxy in front of the Blazor app.";
            };

            domain = mkOption {
              type = types.nullOr types.str;
              default = null;
              description = "Server name for the nginx virtualHost (required when nginx.enable is true).";
              example = "beanshare.example.com";
            };

            enableACME = mkOption {
              type = types.bool;
              default = false;
              description = "Enable TLS via Let's Encrypt (requires nginx.enable and domain).";
            };

            extraConfig = mkOption {
              type = types.lines;
              default = "";
              description = "Extra nginx configuration for the location block.";
            };

            proxyApi.enable = mkOption {
              type = types.bool;
              default = false;
              description = "Expose BeanShare API under the same nginx virtualHost on /api/ (and /swagger). Requires services.beanshare-api.enable.";
            };
          };
        };

        config = mkIf cfg.enable (mkMerge [
          {
            assertions = [
              {
                assertion = !apiProxyEnabled || (apiCfg != null && apiCfg.enable);
                message = "services.beanshare-blazorweb.nginx.proxyApi.enable requires services.beanshare-api.enable.";
              }
            ];
          }
          (mkIf cfg.database.postgresql.enable (
            let
              # Escape single quotes for PostgreSQL string literal.
              sqlEsc = s: builtins.replaceStrings [ "'" ] [ "''" ] s;
            in
            {
              services.postgresql.enable = true;
              # Create DB and user with password on first PostgreSQL init. If PostgreSQL was already enabled elsewhere, create database/user manually and set database.password or use environmentFile.
              services.postgresql.initialScript = pkgs.writeText "beanshare-pg-init.sql" ''
                CREATE USER "${cfg.database.user}" WITH PASSWORD '${sqlEsc cfg.database.password}';
                CREATE DATABASE "${cfg.database.name}" OWNER "${cfg.database.user}";
              '';
            }
          ))
          {
            services.beanshare-blazorweb.package = mkDefault config.packages.blazorwebapp;

            systemd.services.beanshare-blazorweb = {
              description = "BeanShare Blazor web application";
              after = [ "network-online.target" ] ++ (lib.optionals cfg.database.postgresql.enable [ "postgresql.service" ]);
              wants = [ "network-online.target" ] ++ (lib.optionals cfg.database.postgresql.enable [ "postgresql.service" ]);
              wantedBy = [ "multi-user.target" ];

              serviceConfig = {
                DynamicUser = true;
                RuntimeDirectory = "beanshare-blazorweb";
                WorkingDirectory = packageContentRoot;
                Restart = "on-failure";
                RestartSec = "10s";
              } // lib.optionalAttrs (cfg.environmentFile != null) {
                EnvironmentFile = cfg.environmentFile;
              };

              environment = serviceEnvironment;

              script = ''
                exec ${cfg.package}/bin/BeanShare.BlazorWeb
              '';
            };
          }

          (mkIf cfg.openFirewall {
            networking.firewall.allowedTCPPorts = [ cfg.port ];
          })

          (mkIf cfg.nginx.enable (mkIf (cfg.nginx.domain != null) {
            services.nginx.enable = true;
            services.nginx.virtualHosts.${cfg.nginx.domain} = {
              serverName = cfg.nginx.domain;
              locations =
                {
                  "/" = {
                    proxyPass = "http://${cfg.listenAddress}:${toString cfg.port}";
                    proxyWebsockets = true;
                    extraConfig = proxyHeaderConfig + cfg.nginx.extraConfig;
                  };
                }
                // lib.optionalAttrs apiProxyEnabled {
                  "/api/" = {
                    proxyPass = apiProxyUpstream;
                    proxyWebsockets = true;
                    extraConfig = proxyHeaderConfig;
                  };
                  "/swagger" = {
                    proxyPass = apiProxyUpstream;
                    proxyWebsockets = true;
                    extraConfig = proxyHeaderConfig;
                  };
                };
              forceSSL = cfg.nginx.enableACME;
              enableACME = cfg.nginx.enableACME;
            };
          }))
        ]);
      }
  );
}
