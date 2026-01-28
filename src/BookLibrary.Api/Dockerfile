# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BookLibrary.sln .
COPY src/BookLibrary.Domain/BookLibrary.Domain.csproj src/BookLibrary.Domain/
COPY src/BookLibrary.Application/BookLibrary.Application.csproj src/BookLibrary.Application/
COPY src/BookLibrary.Infrastructure/BookLibrary.Infrastructure.csproj src/BookLibrary.Infrastructure/
COPY src/BookLibrary.Api/BookLibrary.Api.csproj src/BookLibrary.Api/

# Restore dependencies
RUN dotnet restore src/BookLibrary.Api/BookLibrary.Api.csproj

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/src/BookLibrary.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BookLibrary.Api.dll"]
