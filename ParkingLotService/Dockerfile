FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ParkingLotService/ParkingLotService.csproj", "ParkingLotService/"]
RUN dotnet restore "ParkingLotService/ParkingLotService.csproj"

COPY . .
WORKDIR /src/ParkingLotService
RUN dotnet publish "ParkingLotService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet ParkingLotService.dll --urls http://0.0.0.0:${PORT:-10000}"]