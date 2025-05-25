FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FarmMetricsAPI/FarmMetricsAPI.csproj", "FarmMetricsAPI/"]
RUN dotnet restore "FarmMetricsAPI/FarmMetricsAPI.csproj"
COPY . .
WORKDIR "/src/FarmMetricsAPI"
RUN dotnet build "FarmMetricsAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FarmMetricsAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FarmMetricsAPI.dll"] 