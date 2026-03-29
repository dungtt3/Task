FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Gateway/TaskManager.Gateway/TaskManager.Gateway.csproj src/Gateway/TaskManager.Gateway/

RUN dotnet restore src/Gateway/TaskManager.Gateway/TaskManager.Gateway.csproj

COPY src/Gateway/ src/Gateway/

# Replace ocelot.json with Docker version
COPY src/Gateway/TaskManager.Gateway/ocelot.docker.json src/Gateway/TaskManager.Gateway/ocelot.json

RUN dotnet publish src/Gateway/TaskManager.Gateway/TaskManager.Gateway.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskManager.Gateway.dll"]
