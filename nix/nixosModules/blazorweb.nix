# NixOS module for deploying BeanShare Blazor web app.
# Use: self.nixosModules.blazorweb and set services.beanshare-blazorweb.package (e.g. self.packages.${system}.blazorwebapp).
# Database and OIDC (Keycloak-compatible) config are passed as environment variables matching appsettings.

{ moduleWithSystem, ... }:
{
  flake.nixosModules.blazorweb = moduleWithSystem (
    perSystem @ { inputs', ... }: nixos @ { pkgs, config, lib, system, ... }:
    let
      cfg = config.services.beanshare-blazorweb;
      # Build connection string from structured options (PostgreSQL-style; app expects this format).
      dbConnectionString = if cfg.database.connectionString != null
        then cfg.database.connectionString
        else "Host=${cfg.database.host};Port=${toString cfg.database.port};Database=${cfg.database.name};Username=${cfg.database.user};Password=${cfg.database.password}";
      baseEnv = {
        ASPNETCORE_ENVIRONMENT = cfg.environment;
        ASPNETCORE_URLS = "http://${cfg.listenAddress}:${toString cfg.port}";
      };
      dbEnv = lib.optionalAttrs cfg.database.enable {
        ConnectionStrings__DefaultConnection = dbConnectionString;
      };
      oidcEnv = lib.optionalAttrs (cfg.oidc.enable && cfg.oidc.authority != "") {
        UseKeycloak = "true";
        Keycloak__Authority = cfg.oidc.authority;
        Keycloak__ClientId = cfg.oidc.clientId;
        Keycloak__ClientSecret = cfg.oidc.clientSecret;
      };
      serviceEnvironment = baseEnv // dbEnv // oidcEnv;
    in
    with lib;
    {
      options.services.beanshare-blazorweb = {
        enable = mkEnableOption "BeanShare Blazor web application";

        package = mkOption {
          type = types.package;
          description = "BeanShare Blazor package (e.g. pkgs.blazorwebapp or self.packages.${system}.blazorwebapp from the flake).";
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

        database = {
          enable = mkOption {
            type = types.bool;
            default = false;
            description = "Pass database connection to the app (ConnectionStrings:DefaultConnection).";
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
            description = "Database password. Prefer environmentFile for secrets to avoid storing in Nix.";
          };
        };

        oidc = {
          enable = mkOption {
            type = types.bool;
            default = false;
            description = "Enable OIDC authentication (Keycloak-compatible). Sets UseKeycloak and Keycloak:* config.";
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
        };
      };

      config = mkIf cfg.enable (mkMerge [
        {
          systemd.services.beanshare-blazorweb = {
            description = "BeanShare Blazor web application";
            after = [ "network-online.target" ];
            wants = [ "network-online.target" ];

            serviceConfig = {
              DynamicUser = true;
              RuntimeDirectory = "beanshare-blazorweb";
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
            locations."/" = {
              proxyPass = "http://${cfg.listenAddress}:${toString cfg.port}";
              proxyWebsockets = true;
              proxyHeaders = true;
              extraConfig = cfg.nginx.extraConfig;
            };
            forceSSL = cfg.nginx.enableACME;
            enableACME = cfg.nginx.enableACME;
          };
        }))
      ]);
    }
  );
}
