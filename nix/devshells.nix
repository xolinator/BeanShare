{ ... }:
{
  perSystem =
    { pkgs, ... }:
    {
      devShells.default = pkgs.mkShell {
        packages = [
          pkgs.dotnetCorePackages.sdk_10_0
          pkgs.dotnetCorePackages.aspnetcore_10_0
        ];
      };
    };
}
