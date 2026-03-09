{ ... }:
{
  perSystem =
    { pkgs
    , ...
    }:
    let
      apiapp = pkgs.buildDotnetModule {
        pname = "beanshare-apiapp";
        version = "0.1.0";
        src = ../..;

        projectFile = "src/Presentation/BeanShare.Api/BeanShare.Api.csproj";
        executables = [ "BeanShare.Api" ];

        dotnet-sdk = pkgs.dotnetCorePackages.sdk_10_0;
        dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_10_0;

        nugetDeps = ../deps.json;

        meta.mainProgram = "BeanShare.Api";
      };
    in
    {
      packages.apiapp = apiapp;
    };
}
