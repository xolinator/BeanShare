{
  description = "BeanShare development flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-25.11";
    nixpkgs-unstable.url = "github:nixos/nixpkgs/nixos-unstable";

    haumea.url = "github:nix-community/haumea";
    flake-parts.url = "github:hercules-ci/flake-parts";
    treefmt-nix.url = "github:numtide/treefmt-nix";
  };

  outputs = inputs:
    let
      inherit (inputs.nixpkgs) lib;
      h = inputs.haumea.lib;
      flake = inputs.flake-parts.lib.mkFlake { inherit inputs; } {
        imports =
          [
            inputs.treefmt-nix.flakeModule
          ]
          ++ (lib.collect builtins.isPath (h.load {
            src = ./nix;
            loader = h.loaders.path;
          }));

        systems = [ "x86_64-linux" "aarch64-linux" "aarch64-darwin" ];

        # Example nixosConfiguration:
        #   nixosConfigurations.myhost = nixosSystem {
        #     system = "x86_64-linux";
        #     modules = [
        #       self.nixosModules.blazorweb
        #       ({ config, ... }: {
        #         services.beanshare-blazorweb = {
        #           enable = true;
        #           package = self.packages.${config.nixpkgs.system}.blazorwebapp;
        #           nginx.enable = true;
        #           nginx.domain = "beanshare.example.com";
        #           nginx.enableACME = true;
        #         };
        #       })
        #     ];
        #   };

        perSystem =
          { system, ... }:
          {
            _module.args.pkgs = import inputs.nixpkgs {
              inherit system;
              config.allowUnfree = true;
            };

            treefmt = {
              projectRootFile = "flake.nix";
              programs.nixpkgs-fmt.enable = true;
            };
          };
      };
    in
    flake;
}
