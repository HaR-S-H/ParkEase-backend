FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AnalyticsService/AnalyticsService.csproj", "AnalyticsService/"]
RUN dotnet restore "AnalyticsService/AnalyticsService.csproj"

COPY . .
WORKDIR /src/AnalyticsService
RUN dotnet publish "AnalyticsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet AnalyticsService.dll --urls http://0.0.0.0:${PORT:-10000}"]