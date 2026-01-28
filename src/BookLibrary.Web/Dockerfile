# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY src/BookLibrary.Web/BookLibrary.Web.csproj src/BookLibrary.Web/

# Restore dependencies
RUN dotnet restore src/BookLibrary.Web/BookLibrary.Web.csproj

# Copy everything else
COPY src/BookLibrary.Web/ src/BookLibrary.Web/

# Build and publish
WORKDIR /src/src/BookLibrary.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8081

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BookLibrary.Web.dll"]
