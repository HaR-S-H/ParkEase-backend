FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["VehicleService/VehicleService.csproj", "VehicleService/"]
RUN dotnet restore "VehicleService/VehicleService.csproj"

COPY . .
WORKDIR /src/VehicleService
RUN dotnet publish "VehicleService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet VehicleService.dll --urls http://0.0.0.0:${PORT:-10000}"]