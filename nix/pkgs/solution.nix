{ ... }:
{
  perSystem =
    { pkgs, ... }:
    let
      solution = pkgs.buildDotnetModule {
        pname = "beanshare-solution";
        version = "0.1.0";
        src = ../..;

        # Nix-compatible "solution" set for dependency locking (excludes MAUI workloads).
        projectFile = [
          "src/Presentation/BeanShare.BlazorWeb/BeanShare.BlazorWeb.csproj"
          "src/Presentation/BeanShare.Api/BeanShare.Api.csproj"
        ];
        dotnet-sdk = pkgs.dotnetCorePackages.sdk_10_0;
        dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_10_0;
        nugetDeps = ../deps.json;
      };
    in
    {
      packages.solution = solution;
      packages.default-fetch-deps = solution.passthru."fetch-deps";
    };
}
