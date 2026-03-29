FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Shared/TaskManager.Shared.Domain/TaskManager.Shared.Domain.csproj src/Shared/TaskManager.Shared.Domain/
COPY src/Shared/TaskManager.Shared.Application/TaskManager.Shared.Application.csproj src/Shared/TaskManager.Shared.Application/
COPY src/Shared/TaskManager.Shared.Infrastructure/TaskManager.Shared.Infrastructure.csproj src/Shared/TaskManager.Shared.Infrastructure/

COPY src/Services/Project/TaskManager.Project.Domain/TaskManager.Project.Domain.csproj src/Services/Project/TaskManager.Project.Domain/
COPY src/Services/Project/TaskManager.Project.Application/TaskManager.Project.Application.csproj src/Services/Project/TaskManager.Project.Application/
COPY src/Services/Project/TaskManager.Project.Infrastructure/TaskManager.Project.Infrastructure.csproj src/Services/Project/TaskManager.Project.Infrastructure/
COPY src/Services/Project/TaskManager.Project.API/TaskManager.Project.API.csproj src/Services/Project/TaskManager.Project.API/

RUN dotnet restore src/Services/Project/TaskManager.Project.API/TaskManager.Project.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Project/ src/Services/Project/

RUN dotnet publish src/Services/Project/TaskManager.Project.API/TaskManager.Project.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskManager.Project.API.dll"]
