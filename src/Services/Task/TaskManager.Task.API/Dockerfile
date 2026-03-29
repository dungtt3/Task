FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Shared/TaskManager.Shared.Domain/TaskManager.Shared.Domain.csproj src/Shared/TaskManager.Shared.Domain/
COPY src/Shared/TaskManager.Shared.Application/TaskManager.Shared.Application.csproj src/Shared/TaskManager.Shared.Application/
COPY src/Shared/TaskManager.Shared.Infrastructure/TaskManager.Shared.Infrastructure.csproj src/Shared/TaskManager.Shared.Infrastructure/

COPY src/Services/Task/TaskManager.Task.Domain/TaskManager.Task.Domain.csproj src/Services/Task/TaskManager.Task.Domain/
COPY src/Services/Task/TaskManager.Task.Application/TaskManager.Task.Application.csproj src/Services/Task/TaskManager.Task.Application/
COPY src/Services/Task/TaskManager.Task.Infrastructure/TaskManager.Task.Infrastructure.csproj src/Services/Task/TaskManager.Task.Infrastructure/
COPY src/Services/Task/TaskManager.Task.API/TaskManager.Task.API.csproj src/Services/Task/TaskManager.Task.API/

RUN dotnet restore src/Services/Task/TaskManager.Task.API/TaskManager.Task.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Task/ src/Services/Task/

RUN dotnet publish src/Services/Task/TaskManager.Task.API/TaskManager.Task.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskManager.Task.API.dll"]
