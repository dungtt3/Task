FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy shared projects
COPY src/Shared/TaskManager.Shared.Domain/TaskManager.Shared.Domain.csproj src/Shared/TaskManager.Shared.Domain/
COPY src/Shared/TaskManager.Shared.Application/TaskManager.Shared.Application.csproj src/Shared/TaskManager.Shared.Application/
COPY src/Shared/TaskManager.Shared.Infrastructure/TaskManager.Shared.Infrastructure.csproj src/Shared/TaskManager.Shared.Infrastructure/

# Copy service projects
COPY src/Services/Auth/TaskManager.Auth.Domain/TaskManager.Auth.Domain.csproj src/Services/Auth/TaskManager.Auth.Domain/
COPY src/Services/Auth/TaskManager.Auth.Application/TaskManager.Auth.Application.csproj src/Services/Auth/TaskManager.Auth.Application/
COPY src/Services/Auth/TaskManager.Auth.Infrastructure/TaskManager.Auth.Infrastructure.csproj src/Services/Auth/TaskManager.Auth.Infrastructure/
COPY src/Services/Auth/TaskManager.Auth.API/TaskManager.Auth.API.csproj src/Services/Auth/TaskManager.Auth.API/

RUN dotnet restore src/Services/Auth/TaskManager.Auth.API/TaskManager.Auth.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Auth/ src/Services/Auth/

RUN dotnet publish src/Services/Auth/TaskManager.Auth.API/TaskManager.Auth.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskManager.Auth.API.dll"]
