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

    apps.beanshareModuleTestContainer = {
      type = "app";
      program = "${pkgs.writeShellApplication {
        name = "beanshareModuleTestContainer";
        runtimeInputs = [
          pkgs.nix
          pkgs.docker
          pkgs.openssh
          pkgs.coreutils
        ];
        text = ''
          set -euo pipefail

          image_name="''${IMAGE_NAME:-beanshare-module-test-container}"
          container_name="''${CONTAINER_NAME:-beanshare-module-test-instance}"
          target_system="''${TARGET_SYSTEM:-x86_64-linux}"
          package_attr="''${PACKAGE_ATTR:-beanshare-module-test-container}"
          flake_ref="path:$PWD"

          echo ">>> Building NixOS module test image for $target_system ($package_attr)"
          nix build "$flake_ref#packages.\"$target_system\".\"$package_attr\""

          echo ">>> Loading image into Docker"
          docker load < result

          cleanup() {
            echo ">>> Stopping test container"
            docker stop "$container_name" >/dev/null 2>&1 || true

            echo ">>> Removing docker image"
            docker image rm -f "$image_name:latest" >/dev/null 2>&1 || true
          }

          trap cleanup EXIT

          echo ">>> Starting container (systemd-in-container mode)"
          docker run -d --rm --privileged --cgroupns=host \
            -v /sys/fs/cgroup:/sys/fs/cgroup:rw \
            --network host \
            --name "$container_name" \
            "$image_name:latest" >/dev/null

          echo ">>> Waiting for SSH on localhost:2222"
          for attempt in $(seq 1 60); do
            echo ">>> SSH probe attempt $attempt/60"
            if ssh -o ConnectTimeout=1 -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null nixos@localhost -p 2222 'echo ok' >/dev/null 2>&1; then
              break
            fi
            sleep 1
          done

          echo ">>> Opening SSH session (password: nixos)"
          ssh nixos@localhost -p 2222 \
            -o StrictHostKeyChecking=no \
            -o UserKnownHostsFile=/dev/null
        '';
      }}/bin/beanshareModuleTestContainer";
    };
  };
}
