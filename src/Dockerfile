# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install EF tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy everything and restore
COPY . .
RUN dotnet restore "./hamalba.csproj"
RUN dotnet build "./hamalba.csproj" -c Release

# Publish app
RUN dotnet publish "./hamalba.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish . 

EXPOSE 80

ENTRYPOINT ["dotnet", "hamalba.dll"]
