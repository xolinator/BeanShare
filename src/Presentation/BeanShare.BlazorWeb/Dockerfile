FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files
COPY ["src/Core/BeanShare.Domain/BeanShare.Domain.csproj", "src/Core/BeanShare.Domain/"]
COPY ["src/Core/BeanShare.Application/BeanShare.Application.csproj", "src/Core/BeanShare.Application/"]
COPY ["src/Infrastructure/BeanShare.Infrastructure/BeanShare.Infrastructure.csproj", "src/Infrastructure/BeanShare.Infrastructure/"]
COPY ["src/Infrastructure/BeanShare.Infrastructure.Persistence/BeanShare.Infrastructure.Persistence.csproj", "src/Infrastructure/BeanShare.Infrastructure.Persistence/"]
COPY ["src/Infrastructure/BeanShare.Infrastructure.Identity/BeanShare.Infrastructure.Identity.csproj", "src/Infrastructure/BeanShare.Infrastructure.Identity/"]
COPY ["src/Infrastructure/BeanShare.Infrastructure.Communication/BeanShare.Infrastructure.Communication.csproj", "src/Infrastructure/BeanShare.Infrastructure.Communication/"]
COPY ["src/Presentation/BeanShare.Contracts/BeanShare.Contracts.csproj", "src/Presentation/BeanShare.Contracts/"]
COPY ["src/Presentation/BeanShare.SharedUi/BeanShare.SharedUi.csproj", "src/Presentation/BeanShare.SharedUi/"]
COPY ["src/Presentation/BeanShare.BlazorWeb/BeanShare.BlazorWeb.csproj", "src/Presentation/BeanShare.BlazorWeb/"]

RUN dotnet restore "src/Presentation/BeanShare.BlazorWeb/BeanShare.BlazorWeb.csproj"

# Copy all source files
COPY . .

WORKDIR "/src/src/Presentation/BeanShare.BlazorWeb"
RUN dotnet build "BeanShare.BlazorWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BeanShare.BlazorWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BeanShare.BlazorWeb.dll"]
