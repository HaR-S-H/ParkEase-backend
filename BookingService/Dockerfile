FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BookingService/BookingService.csproj", "BookingService/"]
RUN dotnet restore "BookingService/BookingService.csproj"

COPY . .
WORKDIR /src/BookingService
RUN dotnet publish "BookingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet BookingService.dll --urls http://0.0.0.0:${PORT:-10000}"]