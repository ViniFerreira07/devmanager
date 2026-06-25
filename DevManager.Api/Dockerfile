FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DevManager.sln", "."]
COPY ["DevManager.Api/DevManager.Api.csproj", "DevManager.Api/"]
COPY ["DevManager.Application/DevManager.Application.csproj", "DevManager.Application/"]
COPY ["DevManager.Domain/DevManager.Domain.csproj", "DevManager.Domain/"]
COPY ["DevManager.Infrastructure/DevManager.Infrastructure.csproj", "DevManager.Infrastructure/"]
RUN dotnet restore "DevManager.Api/DevManager.Api.csproj"

COPY . .
WORKDIR "/src/DevManager.Api"
RUN dotnet build "DevManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DevManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevManager.Api.dll"]