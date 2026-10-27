FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

COPY ["meshbridge.csproj", "./"]
RUN dotnet restore "meshbridge.csproj" -r linux-x64

COPY . .
RUN dotnet publish "meshbridge.csproj" \
    -c Release \
    -r linux-x64 \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./meshbridge"]