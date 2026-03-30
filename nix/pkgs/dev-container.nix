{ inputs, lib, ... }:
{
  perSystem =
    { config
    , pkgs
    , system
    , ...
    }:
    let
      mkModuleTestContainer =
        targetSystem:
        let
          testNixos = inputs.nixpkgs.lib.nixosSystem {
            system = targetSystem;
            modules = [
              inputs.self.nixosModules.api
              inputs.self.nixosModules.beanshare
              ({ pkgs, ... }: {
                boot.isContainer = true;

                networking = {
                  firewall.enable = false;
                  hostName = "beanshare-module-test";
                  useDHCP = false;
                  interfaces = { };
                  nameservers = [ "1.1.1.1" "8.8.8.8" ];
                };

                users.users.nixos = {
                  isNormalUser = true;
                  initialPassword = "nixos";
                  extraGroups = [ "wheel" ];
                  shell = pkgs.bashInteractive;
                };

                services.openssh = {
                  enable = true;
                  settings.PasswordAuthentication = true;
                  ports = [ 2222 ];
                };

                environment.systemPackages = with pkgs; [
                  bash
                  coreutils
                  curl
                  git
                  iproute2
                  inetutils
                  systemd
                ];

                services.beanshare-blazorweb = {
                  enable = true;
                  package = config.packages.blazorwebapp;
                  listenAddress = "127.0.0.1";
                  port = 5001;
                  openFirewall = false;
                  environmentFile = "-/etc/beanshare-oidc.env";
                  qrCodeBaseUrl = "http://localhost:5000";
                  nginx = {
                    enable = true;
                    domain = "localhost";
                    proxyApi.enable = true;
                  };
                  database.password = "beanshare";
                  database.postgresql.enable = true;
                };

                services.beanshare-api = {
                  enable = true;
                  package = config.packages.apiapp;
                  listenAddress = "127.0.0.1";
                  port = 5247;
                  openFirewall = false;
                  environmentFile = "-/etc/beanshare-oidc.env";
                  database.password = "beanshare";
                  database.postgresql.enable = true;
                };

                systemd.services.beanshare-api.environment.Jwt__Secret =
                  "BeanShareSecretKeyForJwtTokenGeneration2024SuperSecure!";

                system.stateVersion = "25.11";
              })
            ];
          };

          initFix = pkgs.runCommand "add-init" { preferLocalBuild = true; } ''
            mkdir -p $out
            ln -s ${pkgs.systemd}/lib/systemd/systemd $out/init
          '';
        in
        pkgs.dockerTools.buildImage {
          name = "beanshare-dev-container";
          tag = "latest";

          config = {
            Cmd = [ "/init" ];
            StopSignal = "SIGRTMIN+3";
            Hostname = "beanshare-dev-container";
            ExposedPorts = {
              "2222/tcp" = { };
              "80/tcp" = { };
            };
          };

          copyToRoot = pkgs.symlinkJoin {
            name = "beanshare-module-test-root";
            paths = [
              testNixos.config.system.build.toplevel
              initFix
            ];
          };
        };
    in
    lib.optionalAttrs (lib.elem system [ "x86_64-linux" "aarch64-linux" ]) {
      packages.devContainer = mkModuleTestContainer system;
    };
}
