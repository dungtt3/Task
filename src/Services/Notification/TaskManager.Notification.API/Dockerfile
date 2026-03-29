FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Shared/TaskManager.Shared.Domain/TaskManager.Shared.Domain.csproj src/Shared/TaskManager.Shared.Domain/
COPY src/Shared/TaskManager.Shared.Application/TaskManager.Shared.Application.csproj src/Shared/TaskManager.Shared.Application/
COPY src/Shared/TaskManager.Shared.Infrastructure/TaskManager.Shared.Infrastructure.csproj src/Shared/TaskManager.Shared.Infrastructure/

COPY src/Services/Notification/TaskManager.Notification.Domain/TaskManager.Notification.Domain.csproj src/Services/Notification/TaskManager.Notification.Domain/
COPY src/Services/Notification/TaskManager.Notification.Application/TaskManager.Notification.Application.csproj src/Services/Notification/TaskManager.Notification.Application/
COPY src/Services/Notification/TaskManager.Notification.Infrastructure/TaskManager.Notification.Infrastructure.csproj src/Services/Notification/TaskManager.Notification.Infrastructure/
COPY src/Services/Notification/TaskManager.Notification.API/TaskManager.Notification.API.csproj src/Services/Notification/TaskManager.Notification.API/

RUN dotnet restore src/Services/Notification/TaskManager.Notification.API/TaskManager.Notification.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Notification/ src/Services/Notification/

RUN dotnet publish src/Services/Notification/TaskManager.Notification.API/TaskManager.Notification.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskManager.Notification.API.dll"]
