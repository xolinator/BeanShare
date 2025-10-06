FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Core/BeanShare.Domain/BeanShare.Domain.csproj", "src/Core/BeanShare.Domain/"]
COPY ["src/Core/BeanShare.Application/BeanShare.Application.csproj", "src/Core/BeanShare.Application/"]
COPY ["src/Infrastructure/BeanShare.Infrastructure/BeanShare.Infrastructure.csproj", "src/Infrastructure/BeanShare.Infrastructure/"]
COPY ["src/Infrastructure/BeanShare.Infrastructure.Persistence/BeanShare.Infrastructure.Persistence.csproj", "src/Infrastructure/BeanShare.Infrastructure.Persistence/"]
COPY ["src/Infrastructure/BeanShare.Infrastructure.Identity/BeanShare.Infrastructure.Identity.csproj", "src/Infrastructure/BeanShare.Infrastructure.Identity/"]
COPY ["src/Presentation/BeanShare.Contracts/BeanShare.Contracts.csproj", "src/Presentation/BeanShare.Contracts/"]
COPY ["src/Presentation/BeanShare.Api/BeanShare.Api.csproj", "src/Presentation/BeanShare.Api/"]
RUN dotnet restore "src/Presentation/BeanShare.Api/BeanShare.Api.csproj"

COPY . .
WORKDIR "/src/src/Presentation/BeanShare.Api"
RUN dotnet build "BeanShare.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BeanShare.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BeanShare.Api.dll"]
