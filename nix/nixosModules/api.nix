# NixOS module for deploying BeanShare API.

{ moduleWithSystem, ... }:
{
  flake.nixosModules.api = moduleWithSystem (
    perSystem @ { inputs', ... }: nixos @ { pkgs, config, lib, system, ... }:
      let
        cfg = config.services.beanshare-api;
        packageContentRoot = "${cfg.package}/lib/beanshare-apiapp";
        dbHost = if cfg.database.postgresql.enable then "localhost" else cfg.database.host;
        dbPort = if cfg.database.postgresql.enable then config.services.postgresql.settings.port else cfg.database.port;
        dbConnectionString =
          if cfg.database.connectionString != null
          then cfg.database.connectionString
          else "Host=${dbHost};Port=${toString dbPort};Database=${cfg.database.name};Username=${cfg.database.user};Password=${cfg.database.password}";
        useDatabase = cfg.database.enable || cfg.database.postgresql.enable;
        baseEnv = {
          ASPNETCORE_ENVIRONMENT = cfg.environment;
          ASPNETCORE_URLS = "http://${cfg.listenAddress}:${toString cfg.port}";
          UseOidc = if cfg.oidc.enable then "true" else "false";
        };
        dbEnv = lib.optionalAttrs useDatabase {
          ConnectionStrings__DefaultConnection = dbConnectionString;
        };
        oidcEnv = lib.optionalAttrs (cfg.oidc.enable && cfg.oidc.authority != "") {
          UseOidc = "true";
          Oidc__Authority = cfg.oidc.authority;
          Oidc__Audience = cfg.oidc.audience;
        };
        jwtEnv = lib.optionalAttrs (cfg.jwt.secret != "") {
          Jwt__Secret = cfg.jwt.secret;
          Jwt__Issuer = cfg.jwt.issuer;
          Jwt__Audience = cfg.jwt.audience;
          Jwt__ExpirationMinutes = toString cfg.jwt.expirationMinutes;
        };
        serviceEnvironment = baseEnv // dbEnv // oidcEnv // jwtEnv;
        proxyHeaderConfig = ''
          proxy_set_header Host $host;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Host $http_host;
          proxy_set_header X-Forwarded-Proto $scheme;
        '';
      in
      {
        options.services.beanshare-api = {
          enable = lib.mkEnableOption "BeanShare API";

          package = lib.mkOption {
            type = lib.types.package;
            description = "BeanShare API package.";
          };

          port = lib.mkOption {
            type = lib.types.port;
            default = 5247;
            description = "Port on which Kestrel will listen.";
          };

          listenAddress = lib.mkOption {
            type = lib.types.str;
            default = "127.0.0.1";
          };

          openFirewall = lib.mkOption {
            type = lib.types.bool;
            default = false;
          };

          environment = lib.mkOption {
            type = lib.types.str;
            default = "Production";
          };

          database = {
            enable = lib.mkOption { type = lib.types.bool; default = false; };
            postgresql = {
              enable = lib.mkOption {
                type = lib.types.bool;
                default = true;
                description = "Enable and use the NixOS PostgreSQL service for the API.";
              };

              dataDir = lib.mkOption {
                type = lib.types.str;
                default = "/var/lib/postgresql";
                description = "Persistent PostgreSQL data directory. Overridden by beanshare-common when both modules are used.";
              };
            };
            connectionString = lib.mkOption { type = lib.types.nullOr lib.types.str; default = null; };
            host = lib.mkOption { type = lib.types.str; default = "localhost"; };
            port = lib.mkOption { type = lib.types.port; default = 5432; };
            name = lib.mkOption { type = lib.types.str; default = "beanshare"; };
            user = lib.mkOption { type = lib.types.str; default = "beanshare"; };
            password = lib.mkOption {
              type = lib.types.str;
              default = "";
              description = ''
                Database password. Empty is valid for local/dev setups.
                WARNING: values set here end up in the world-readable Nix store.
                Use environmentFile for secrets in production.
              '';
            };
          };

          oidc = {
            enable = lib.mkOption { type = lib.types.bool; default = false; };
            authority = lib.mkOption { type = lib.types.str; default = ""; };
            audience = lib.mkOption { type = lib.types.str; default = "beanshare-api"; };
          };

          jwt = {
            secret = lib.mkOption {
              type = lib.types.str;
              default = "";
              description = ''
                JWT signing secret.
                WARNING: values set here end up in the world-readable Nix store.
                Use environmentFile for secrets in production.
              '';
            };
            issuer = lib.mkOption { type = lib.types.str; default = "BeanShare"; description = "JWT issuer claim."; };
            audience = lib.mkOption { type = lib.types.str; default = "BeanShare"; description = "JWT audience claim."; };
            expirationMinutes = lib.mkOption { type = lib.types.int; default = 1440; description = "JWT token expiration in minutes."; };
          };

          environmentFile = lib.mkOption {
            type = lib.types.nullOr lib.types.str;
            default = null;
          };

          nginx = {
            enable = lib.mkOption { type = lib.types.bool; default = false; };
            domain = lib.mkOption { type = lib.types.nullOr lib.types.str; default = null; };
            enableACME = lib.mkOption { type = lib.types.bool; default = false; };
          };
        };

        config = lib.mkIf cfg.enable (lib.mkMerge [
          {
            services.beanshare-api.package = lib.mkDefault perSystem.config.packages.apiapp;

            systemd.services.beanshare-api = {
              description = "BeanShare API";
              after = [ "network-online.target" ] ++ (lib.optionals cfg.database.postgresql.enable [ "postgresql.service" ]);
              wants = [ "network-online.target" ] ++ (lib.optionals cfg.database.postgresql.enable [ "postgresql.service" ]);
              wantedBy = [ "multi-user.target" ];

              serviceConfig = {
                DynamicUser = true;
                RuntimeDirectory = "beanshare-api";
                WorkingDirectory = packageContentRoot;
                Restart = "on-failure";
                RestartSec = "10s";
              } // lib.optionalAttrs (cfg.environmentFile != null) {
                EnvironmentFile = cfg.environmentFile;
              };

              environment = serviceEnvironment;

              script = ''
                exec ${cfg.package}/bin/BeanShare.Api
              '';
            };
          }

          (lib.mkIf cfg.openFirewall {
            networking.firewall.allowedTCPPorts = [ cfg.port ];
          })

          (lib.mkIf cfg.nginx.enable (lib.mkIf (cfg.nginx.domain != null) {
            services.nginx.enable = true;
            services.nginx.virtualHosts.${cfg.nginx.domain} = {
              serverName = cfg.nginx.domain;
              locations."/" = {
                proxyPass = "http://${cfg.listenAddress}:${toString cfg.port}";
                proxyWebsockets = true;
                extraConfig = proxyHeaderConfig;
              };
              forceSSL = cfg.nginx.enableACME;
              enableACME = cfg.nginx.enableACME;
            };
          }))
        ]);
      }
  );
}
