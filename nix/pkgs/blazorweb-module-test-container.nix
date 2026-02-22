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
              inputs.self.nixosModules.blazorweb
              ({ pkgs, ... }: {
                boot.isContainer = true;

                networking = {
                  firewall.enable = false;
                  hostName = "blazorweb-module-test";
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
                  listenAddress = "0.0.0.0";
                  port = 5000;
                  openFirewall = false;
                  database.postgresql.enable = true;
                };

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
          name = "blazorweb-module-test-container";
          tag = "latest";

          config = {
            Cmd = [ "/init" ];
            StopSignal = "SIGRTMIN+3";
            Hostname = "blazorweb-module-test";
            ExposedPorts = {
              "2222/tcp" = { };
              "5000/tcp" = { };
            };
          };

          copyToRoot = pkgs.symlinkJoin {
            name = "blazorweb-module-test-root";
            paths = [
              testNixos.config.system.build.toplevel
              initFix
            ];
          };
        };
    in
    lib.optionalAttrs (lib.elem system [ "x86_64-linux" "aarch64-linux" ]) {
      packages.blazorweb-module-test-container = mkModuleTestContainer system;
    };
}
