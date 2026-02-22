{ ... }:
{
  perSystem = { pkgs, ... }: {
    devShells.default = pkgs.mkShell {
      packages = [
        pkgs.git
        pkgs.dotnetCorePackages.sdk_10_0
        pkgs.dotnetCorePackages.aspnetcore_10_0
      ];

      shellHook = ''
        echo "BeanShare dev shell — .NET $(dotnet --version)"
      '';
    };
  };
}
