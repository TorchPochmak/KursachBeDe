FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WatchStoreClient/WatchStoreClient.csproj", "WatchStoreClient/"]
RUN dotnet restore "WatchStoreClient/WatchStoreClient.csproj"
COPY . .
WORKDIR "/src/WatchStoreClient"
RUN dotnet build "WatchStoreClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WatchStoreClient.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install native dependencies
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libx11-6 \
    libxrender1 \
    libxext6 \
    libskia-dev \
    libc6-dev \
    libglib2.0-0 \
    libatk1.0-0 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WatchStoreClient.dll"] 