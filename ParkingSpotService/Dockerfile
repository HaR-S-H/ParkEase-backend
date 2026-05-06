FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ParkingSpotService/ParkingSpotService.csproj", "ParkingSpotService/"]
RUN dotnet restore "ParkingSpotService/ParkingSpotService.csproj"

COPY . .
WORKDIR /src/ParkingSpotService
RUN dotnet publish "ParkingSpotService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet ParkingSpotService.dll --urls http://0.0.0.0:${PORT:-10000}"]