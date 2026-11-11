FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ShoeProductionApp.csproj", "./"]
RUN dotnet restore "ShoeProductionApp.csproj"

COPY . .
RUN dotnet publish "ShoeProductionApp.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShoeProductionApp.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrator
WORKDIR /src

COPY ["ShoeProductionApp.csproj", "./"]
RUN dotnet restore "ShoeProductionApp.csproj"

COPY . .
RUN dotnet build "ShoeProductionApp.csproj" --configuration Release
RUN dotnet tool install --global dotnet-ef --version 10.0.13

ENV PATH="${PATH}:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ef", "database", "update", "--no-build", "--configuration", "Release"]
