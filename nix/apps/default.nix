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

    apps."dev-container" = {
      type = "app";
      program = "${pkgs.writeShellApplication {
        name = "dev-container";
        runtimeInputs = [
          pkgs.nix
          pkgs.docker
          pkgs.openssh
          pkgs.coreutils
          pkgs.gnugrep
          pkgs.git
        ];
        text = ''
          set -euo pipefail

          image_name="dev-container"
          container_name="dev-container-instance"
          version_file="''${XDG_CACHE_HOME:-$HOME/.cache}/dev-container-version"
          target_system="''${TARGET_SYSTEM:-x86_64-linux}"
          package_attr="devContainer"
          flake_ref="path:$PWD"

          mkdir -p "$(dirname "$version_file")"

          usage() {
            cat <<EOF
          Usage: dev-container <command> [options]

          Commands:
            up      Build and start the dev container (detached)
            exec    Execute a command in the running container (default: bash)
            down    Stop and remove the dev container
            ps      List dev containers
            status  Show detailed container status
            ssh     Connect to container via SSH (legacy behavior)

          Examples:
            dev-container up
            dev-container exec
            dev-container exec ls -la
            dev-container down
            dev-container ps
            dev-container status
          EOF
          }

          wait_for_ssh() {
            echo ">>> Waiting for SSH to become available..."
            for attempt in $(seq 1 60); do
              if timeout 1 bash -c '</dev/tcp/127.0.0.1/2222' >/dev/null 2>&1; then
                return 0
              fi
              echo "Attempt $attempt: waiting for SSH..."
              sleep 1
            done

            echo ">>> SSH did not become available in time" >&2
            return 1
          }

          cmd_up() {
            local rebuild=false
            local no_cache=false

            while [[ $# -gt 0 ]]; do
              case $1 in
                --rebuild|-r) rebuild=true; shift ;;
                --no-cache) no_cache=true; shift ;;
                -h|--help)
                  echo "Usage: dev-container up [--rebuild|-r] [--no-cache]"
                  echo "  --rebuild, -r   Force rebuild of the image"
                  echo "  --no-cache      Disable Nix cache during build"
                  exit 0
                  ;;
                *) echo "Unknown option: $1"; exit 1 ;;
              esac
            done

            if docker ps --format '{{.Names}}' | grep -q "^$container_name$"; then
              echo ">>> Container '$container_name' is already running"
              echo ">>> Use 'dev-container exec' to enter the container"
              return 0
            fi

            if docker ps -a --format '{{.Names}}' | grep -q "^$container_name$"; then
              echo ">>> Removing stopped container..."
              docker rm -f "$container_name" 2>/dev/null || true
            fi

            local current_version
            current_version=$(git -C "$(git rev-parse --show-toplevel)" describe --always --dirty 2>/dev/null || date +%s)
            local last_version=""
            [ -f "$version_file" ] && last_version=$(cat "$version_file")

            if ! docker image inspect "$image_name":latest >/dev/null 2>&1 || \
               [ "$rebuild" = true ] || \
               [ "$current_version" != "$last_version" ]; then
              echo ">>> Building container image: $image_name"
              if [ "$no_cache" = true ]; then
                nix build "$flake_ref#packages.\"$target_system\".$package_attr" --option substitute false
              else
                nix build "$flake_ref#packages.\"$target_system\".$package_attr"
              fi
              echo ">>> Loading image into Docker..."
              docker load < result
              echo "$current_version" > "$version_file"
            else
              echo ">>> Using existing image: $image_name:latest"
            fi

            echo ">>> Starting container..."
            docker run -d --rm --privileged --cgroupns=host \
              -v /sys/fs/cgroup:/sys/fs/cgroup:rw \
              -p 127.0.0.1:2222:2222 \
              -p 0.0.0.0:5000:80 \
              --name "$container_name" \
              "$image_name":latest >/dev/null

            echo ">>> Container '$container_name' started successfully"
            echo ">>> Use 'dev-container exec' to enter the container"
            echo ">>> Use 'dev-container status' to check status"
          }

          cmd_exec() {
            local tty_flag=""
            local shell_path="/run/current-system/sw/bin/bash"
            [ -t 0 ] && tty_flag="-t"

            if ! docker ps --format '{{.Names}}' | grep -q "^$container_name$"; then
              echo ">>> Container '$container_name' is not running"
              echo ">>> Run 'dev-container up' first"
              exit 1
            fi

            if [ $# -gt 0 ]; then
              docker exec -i $tty_flag "$container_name" "$shell_path" -lc "$*"
            else
              docker exec -i $tty_flag "$container_name" "$shell_path"
            fi
          }

          cmd_down() {
            local force=false

            while [[ $# -gt 0 ]]; do
              case $1 in
                --force|-f) force=true; shift ;;
                -h|--help)
                  echo "Usage: dev-container down [--force|-f]"
                  echo "  --force, -f  Force remove without confirmation"
                  exit 0
                  ;;
                *) echo "Unknown option: $1"; exit 1 ;;
              esac
            done

            if ! docker ps -a --format '{{.Names}}' | grep -q "^$container_name$"; then
              echo ">>> Container '$container_name' is not running or does not exist"

              if docker image inspect "$image_name":latest >/dev/null 2>&1; then
                if [ "$force" = true ]; then
                  docker image rm -f "$image_name":latest
                else
                  read -r -p ">>> Remove image '$image_name:latest'? [y/N]: " confirm
                  if [[ "$confirm" == [yY] || "$confirm" == [yY][eE][sS] ]]; then
                    docker image rm -f "$image_name":latest
                  fi
                fi
              fi
              return 0
            fi

            if [ "$force" = true ]; then
              echo ">>> Stopping container..."
              docker stop "$container_name" 2>/dev/null || docker kill "$container_name" 2>/dev/null || true
            else
              echo ">>> Stopping container..."
              docker stop "$container_name" || true
            fi

            echo ">>> Container stopped"

            if docker image inspect "$image_name":latest >/dev/null 2>&1; then
              if [ "$force" = true ]; then
                docker image rm -f "$image_name":latest
              else
                read -r -p ">>> Remove image '$image_name:latest'? [y/N]: " confirm
                if [[ "$confirm" == [yY] || "$confirm" == [yY][eE][sS] ]]; then
                  docker image rm -f "$image_name":latest
                fi
              fi
            fi
          }

          cmd_ps() {
            local all=false
            local quiet=false

            while [[ $# -gt 0 ]]; do
              case $1 in
                --all|-a) all=true; shift ;;
                --quiet|-q) quiet=true; shift ;;
                -h|--help)
                  echo "Usage: dev-container ps [--all|-a] [--quiet|-q]"
                  echo "  --all, -a      Show all containers (including stopped)"
                  echo "  --quiet, -q    Only show container names/IDs"
                  exit 0
                  ;;
                *) echo "Unknown option: $1"; exit 1 ;;
              esac
            done

            local ps_format="table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}\t{{.RunningFor}}"
            local containers
            if [ "$all" = false ]; then
              containers=$(docker ps --filter "status=running" --format "{{.Names}}" | grep "^$container_name" || true)
            else
              containers=$(docker ps --format "{{.Names}}" | grep "^$container_name" || true)
            fi

            if [ -z "$containers" ]; then
              if [ "$quiet" = true ]; then
                :
              else
                echo ">>> No dev containers found"
              fi
              return 0
            fi

            if [ "$quiet" = true ]; then
              echo "$containers"
            else
              echo ">>> Dev Containers"
              echo "=================="
              if [ "$all" = false ]; then
                docker ps --filter "status=running" --filter "name=$container_name" --format "$ps_format"
              else
                docker ps --filter "name=$container_name" --format "$ps_format"
              fi
            fi
          }

          cmd_status() {
            echo ">>> Container Status"
            echo "===================="

            if docker ps --format '{{.Names}}' | grep -q "^$container_name$"; then
              echo "Status:    Running"
              docker inspect --format='Started:   {{.State.StartedAt}}' "$container_name"
              docker inspect --format='Health:    {{.State.Health.Status}}' "$container_name" 2>/dev/null || echo "Health:    N/A"
            elif docker ps -a --format '{{.Names}}' | grep -q "^$container_name$"; then
              echo "Status:    Stopped"
            else
              echo "Status:    Not created"
            fi

            if docker image inspect "$image_name":latest >/dev/null 2>&1; then
              echo "Image:     Available"
              echo "Created:   $(docker inspect --format='{{.Created}}' "$image_name":latest | cut -dT -f1)"
            else
              echo "Image:     Not found"
            fi
          }

          cmd_ssh() {
            if ! docker ps --format '{{.Names}}' | grep -q "^$container_name$"; then
              echo ">>> Container not running, starting..."
              cmd_up
            fi

            wait_for_ssh

            echo ">>> Connecting via SSH..."
            ssh nixos@localhost -p 2222 \
              -o StrictHostKeyChecking=no \
              -o UserKnownHostsFile=/dev/null
          }

          if [ $# -eq 0 ]; then
            usage
            exit 1
          fi

          COMMAND="$1"
          shift

          case "$COMMAND" in
            up)     cmd_up "$@" ;;
            exec)   cmd_exec "$@" ;;
            down)   cmd_down "$@" ;;
            ps)     cmd_ps "$@" ;;
            status) cmd_status "$@" ;;
            ssh)    cmd_ssh "$@" ;;
            -h|--help)
              usage
              exit 0
              ;;
            *)
              echo "Unknown command: $COMMAND"
              usage
              exit 1
              ;;
          esac
        '';
      }}/bin/dev-container";
    };
  };
}
