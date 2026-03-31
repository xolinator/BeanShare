# NixOS module for deploying BeanShare API.

{ moduleWithSystem, ... }:
{
  flake.nixosModules.api = moduleWithSystem (
    perSystem @ { inputs', ... }: nixos @ { pkgs, config, lib, system, ... }:
      let
        cfg = config.services.beanshare-api;
        packageContentRoot = "${cfg.package}/lib/beanshare-apiapp";
        postgresqlSchema =
          if builtins.hasAttr "psqlSchema" config.services.postgresql.package
          then config.services.postgresql.package.psqlSchema
          else lib.versions.major config.services.postgresql.package.version;
        defaultPostgresqlDataDir = "/mnt/db/data/postgresql/${postgresqlSchema}";
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
        serviceEnvironment = baseEnv // dbEnv // oidcEnv;
        proxyHeaderConfig = ''
          proxy_set_header Host $host;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Host $http_host;
          proxy_set_header X-Forwarded-Proto $scheme;
        '';
      in
      with lib;
      {
        options.services.beanshare-api = {
          enable = mkEnableOption "BeanShare API";

          package = mkOption {
            type = types.package;
            description = "BeanShare API package.";
          };

          port = mkOption {
            type = types.port;
            default = 5000;
          };

          listenAddress = mkOption {
            type = types.str;
            default = "127.0.0.1";
          };

          openFirewall = mkOption {
            type = types.bool;
            default = false;
          };

          environment = mkOption {
            type = types.str;
            default = "Production";
          };

          database = {
            enable = mkOption { type = types.bool; default = false; };
            postgresql = {
              enable = mkOption {
                type = types.bool;
                default = true;
                description = "Enable and use the NixOS PostgreSQL service for the API.";
              };

              dataDir = mkOption {
                type = types.str;
                default = defaultPostgresqlDataDir;
                description = "Persistent PostgreSQL data directory for the local API database. The default keeps data under /mnt/db/data and includes the PostgreSQL major version in the path.";
                example = "/mnt/db/data/postgresql/18";
              };
            };
            connectionString = mkOption { type = types.nullOr types.str; default = null; };
            host = mkOption { type = types.str; default = "localhost"; };
            port = mkOption { type = types.port; default = 5432; };
            name = mkOption { type = types.str; default = "beanshare"; };
            user = mkOption { type = types.str; default = "beanshare"; };
            password = mkOption { type = types.str; default = ""; };
          };

          oidc = {
            enable = mkOption { type = types.bool; default = false; };
            authority = mkOption { type = types.str; default = ""; };
            audience = mkOption { type = types.str; default = "beanshare-api"; };
          };

          environmentFile = mkOption {
            type = types.nullOr types.str;
            default = null;
          };

          nginx = {
            enable = mkOption { type = types.bool; default = false; };
            domain = mkOption { type = types.nullOr types.str; default = null; };
            enableACME = mkOption { type = types.bool; default = false; };
          };
        };

        config = mkIf cfg.enable (mkMerge [
          (mkIf cfg.database.postgresql.enable (
            let
              sqlEsc = s: builtins.replaceStrings [ "'" ] [ "''" ] s;
            in
            {
              services.postgresql.enable = mkDefault true;
              services.postgresql.dataDir = mkOverride 900 cfg.database.postgresql.dataDir;
              services.postgresql.initialScript = mkDefault (pkgs.writeText "beanshare-pg-init.sql" ''
                CREATE USER "${cfg.database.user}" WITH PASSWORD '${sqlEsc cfg.database.password}';
                CREATE DATABASE "${cfg.database.name}" OWNER "${cfg.database.user}";
              '');
            }
          ))
          {
            services.beanshare-api.package = mkDefault config.packages.apiapp;

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
