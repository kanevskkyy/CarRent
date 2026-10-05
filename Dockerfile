# Build and publish the API with the .NET 10 SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CarRent.Api/CarRent.Api.csproj src/CarRent.Api/
COPY src/CarRent.Application/CarRent.Application.csproj src/CarRent.Application/
COPY src/CarRent.Domain/CarRent.Domain.csproj src/CarRent.Domain/
COPY src/CarRent.Infrastructure/CarRent.Infrastructure.csproj src/CarRent.Infrastructure/
RUN dotnet restore src/CarRent.Api/CarRent.Api.csproj

COPY src/ src/
RUN dotnet publish src/CarRent.Api/CarRent.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Run on the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CarRent.Api.dll"]

