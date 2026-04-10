# ============================
# 1. Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./src/Directory.Packages.props ./

COPY ./src/DbSyncEngine.Worker/*.csproj ./DbSyncEngine.Worker/
COPY ./src/DbSyncEngine.Application/*.csproj ./DbSyncEngine.Application/
COPY ./src/DbSyncEngine.Infrastructure/*.csproj ./DbSyncEngine.Infrastructure/
COPY ./src/DbSyncEngine.Domain/*.csproj ./DbSyncEngine.Domain/

RUN dotnet restore ./DbSyncEngine.Worker/DbSyncEngine.Worker.csproj
COPY ./src .
RUN dotnet publish ./DbSyncEngine.Worker/DbSyncEngine.Worker.csproj -c Release -o /app/publish


# ============================
# 2. Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        iputils-ping \
        iproute2 \
        net-tools \
        dnsutils && \
    rm -rf /var/lib/apt/lists/*
    
COPY --from=build /app/publish .

ENV SYNC_CONFIG_PATH=""

ENTRYPOINT ["dotnet", "DbSyncEngine.Worker.dll"]