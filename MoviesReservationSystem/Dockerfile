FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.1 as runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 7035
ENTRYPOINT ["dotnet", "MoviesReservationSystem.dll"]
 