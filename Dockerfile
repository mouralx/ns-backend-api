# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/MassageBooking.Api/MassageBooking.Api.csproj", "src/MassageBooking.Api/"]
COPY ["src/MassageBooking.Application/MassageBooking.Application.csproj", "src/MassageBooking.Application/"]
COPY ["src/MassageBooking.Domain/MassageBooking.Domain.csproj", "src/MassageBooking.Domain/"]
COPY ["src/MassageBooking.Infrastructure/MassageBooking.Infrastructure.csproj", "src/MassageBooking.Infrastructure/"]
RUN dotnet restore "src/MassageBooking.Api/MassageBooking.Api.csproj"

# Copy source and build
COPY src/ src/
RUN dotnet publish "src/MassageBooking.Api/MassageBooking.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install ICU for globalization
RUN apt-get update && apt-get install -y --no-install-recommends libicu-dev && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN useradd -m appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "MassageBooking.Api.dll"]
