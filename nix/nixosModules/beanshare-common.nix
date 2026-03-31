{ ... }:
{
  flake.nixosModules.beanshareCommon =
    { config, lib, options, ... }:
    let
      postgresqlSchema =
        if builtins.hasAttr "psqlSchema" config.services.postgresql.package
        then config.services.postgresql.package.psqlSchema
        else lib.versions.major config.services.postgresql.package.version;
      defaultPostgresqlDataDir = "/mnt/db/data/postgresql/${postgresqlSchema}";
      defaultUploadsRootPath = "/mnt/db/data/beanshare/uploads";
      sharedCfg = config.services.beanshare;
      blazorEnabled = lib.attrByPath [ "services" "beanshare-blazorweb" "enable" ] false config;
      apiEnabled = lib.attrByPath [ "services" "beanshare-api" "enable" ] false config;
      anyBeanShareServiceEnabled = blazorEnabled || apiEnabled;
      uploadsRootPath = sharedCfg.storage.uploadsRootPath;
      postgresqlEnabled = config.services.postgresql.enable;
      postgresqlDataDir = config.services.postgresql.dataDir;
      postgresqlParentDir = builtins.dirOf postgresqlDataDir;
      uploadsTmpfilesRules = [
        "d ${uploadsRootPath} 2775 root beanshare - -"
        "d ${uploadsRootPath}/avatars 2775 root beanshare - -"
      ];
      postgresqlTmpfilesRules = [
        "d ${postgresqlParentDir} 0755 root root - -"
        "d ${postgresqlDataDir} 0700 postgres postgres - -"
      ];

      applySharedDefaults =
        serviceName:
        lib.mkIf (lib.hasAttrByPath [ "services" serviceName ] options) (
          lib.mkMerge [
            (lib.setAttrByPath [ "services" serviceName "database" "enable" ] (lib.mkDefault true))
            (lib.setAttrByPath [ "services" serviceName "database" "host" ] (lib.mkDefault sharedCfg.database.host))
            (lib.setAttrByPath [ "services" serviceName "database" "port" ] (lib.mkDefault sharedCfg.database.port))
            (lib.setAttrByPath [ "services" serviceName "database" "name" ] (lib.mkDefault sharedCfg.database.name))
            (lib.setAttrByPath [ "services" serviceName "database" "user" ] (lib.mkDefault sharedCfg.database.user))
            (lib.setAttrByPath [ "services" serviceName "database" "password" ] (lib.mkDefault sharedCfg.database.password))
            (lib.setAttrByPath [ "services" serviceName "database" "connectionString" ] (lib.mkDefault sharedCfg.database.connectionString))
            (lib.setAttrByPath [ "services" serviceName "database" "postgresql" "enable" ] (lib.mkDefault sharedCfg.database.postgresql.enable))
            (lib.setAttrByPath [ "services" serviceName "database" "postgresql" "dataDir" ] (lib.mkDefault sharedCfg.database.postgresql.dataDir))
            (lib.setAttrByPath [ "systemd" "services" serviceName "environment" "UploadsRootPath" ] (lib.mkDefault uploadsRootPath))
            (lib.setAttrByPath [ "systemd" "services" serviceName "serviceConfig" "SupplementaryGroups" ] (lib.mkDefault [ "beanshare" ]))
            (lib.setAttrByPath [ "systemd" "services" serviceName "serviceConfig" "UMask" ] (lib.mkDefault "0002"))
          ]
        );
    in
    {
      options.services.beanshare = {
        database = {
          connectionString = lib.mkOption {
            type = lib.types.nullOr lib.types.str;
            default = null;
            description = "Shared BeanShare database connection string. When set, it becomes the default for both the web and API modules.";
            example = "Host=localhost;Port=5432;Database=beanshare;Username=beanshare;Password=secret";
          };

          host = lib.mkOption {
            type = lib.types.str;
            default = "localhost";
            description = "Shared default database host for BeanShare services.";
          };

          port = lib.mkOption {
            type = lib.types.port;
            default = 5432;
            description = "Shared default database port for BeanShare services.";
          };

          name = lib.mkOption {
            type = lib.types.str;
            default = "beanshare";
            description = "Shared default database name for BeanShare services.";
          };

          user = lib.mkOption {
            type = lib.types.str;
            default = "beanshare";
            description = "Shared default database user for BeanShare services.";
          };

          password = lib.mkOption {
            type = lib.types.str;
            default = "";
            description = "Shared default database password for BeanShare services. Prefer environmentFile for secrets in production.";
          };

          postgresql = {
            enable = lib.mkOption {
              type = lib.types.bool;
              default = true;
              description = "Shared default for using the local NixOS PostgreSQL service with BeanShare.";
            };

            dataDir = lib.mkOption {
              type = lib.types.str;
              default = defaultPostgresqlDataDir;
              description = "Shared default PostgreSQL data directory. The default keeps data under /mnt/db/data and includes the PostgreSQL major version in the path.";
              example = "/mnt/db/data/postgresql/18";
            };
          };
        };

        storage = {
          uploadsRootPath = lib.mkOption {
            type = lib.types.str;
            default = defaultUploadsRootPath;
            description = "Shared filesystem path for BeanShare uploads. The path is prepared with group-write permissions so both the web app and API can serve and update avatars.";
            example = "/mnt/db/data/beanshare/uploads";
          };
        };
      };

      config = lib.mkMerge [
        (applySharedDefaults "beanshare-blazorweb")
        (applySharedDefaults "beanshare-api")
        (lib.mkIf anyBeanShareServiceEnabled {
          users.groups.beanshare = { };
          systemd.tmpfiles.rules = uploadsTmpfilesRules;
        })
        (lib.mkIf (anyBeanShareServiceEnabled && postgresqlEnabled) {
          systemd.tmpfiles.rules = postgresqlTmpfilesRules;
        })
      ];
    };
}
