FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish "Live_Cryptocurrency_Watchlist.csproj" -c Release -o /app/publish

RUN dotnet publish "Live_Cryptocurrency_Watchlist.csproj" -c Release -o /app/publish
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Live_Cryptocurrency_Watchlist.dll"]