# NixOS module for deploying BeanShare Blazor web app.
# Use: self.nixosModules.blazorweb and set services.beanshare-blazorweb.package (e.g. self.packages.${system}.blazorwebapp).

{ moduleWithSystem, ... }:
{
  flake.nixosModules.blazorweb = moduleWithSystem (
    perSystem @ { inputs', ... }: nixos @ { pkgs, config, lib, system, ... }:
    let
      cfg = config.services.beanshare-blazorweb;
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
            };

            environment = {
              ASPNETCORE_ENVIRONMENT = cfg.environment;
              ASPNETCORE_URLS = "http://${cfg.listenAddress}:${toString cfg.port}";
            };

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
