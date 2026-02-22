{ ... }:
{
  perSystem = { pkgs, ... }: {
    apps.updateDeps = {
      type = "app";
      program = "${pkgs.writeShellApplication {
        name = "updateDeps";
        text = ''
          set -euo pipefail

          if [ ! -f flake.nix ]; then
            echo "Run from the repository root so ./nix/deps.json is writable." >&2
            exit 1
          fi

          $(nix build --no-link --print-out-paths .#default-fetch-deps) ./nix/deps.json
          echo "Updated nix/deps.json"
        '';
      }}/bin/updateDeps";
    };
  };
}
