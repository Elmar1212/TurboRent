FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/backend/TurboRent.Api/TurboRent.Api.csproj --locked-mode
RUN dotnet publish src/backend/TurboRent.Api/TurboRent.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY TurboRent.slnx .
RUN mkdir -p /app/artifacts/identity-local && chown -R $APP_UID:$APP_UID /app/artifacts
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "TurboRent.Api.dll"]
