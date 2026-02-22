{ ... }:
{
  perSystem =
    { pkgs
    , ...
    }:
    let
      blazorwebapp = pkgs.buildDotnetModule {
        pname = "beanshare-blazorwebapp";
        version = "0.1.0";
        src = ../..;

        projectFile = "src/Presentation/BeanShare.BlazorWeb/BeanShare.BlazorWeb.csproj";
        executables = [ "BeanShare.BlazorWeb" ];

        dotnet-sdk = pkgs.dotnetCorePackages.sdk_10_0;
        dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_10_0;

        nugetDeps = ../deps.json;

        meta.mainProgram = "BeanShare.BlazorWeb";
      };
    in
    {
      packages.blazorwebapp = blazorwebapp;
    };
}
