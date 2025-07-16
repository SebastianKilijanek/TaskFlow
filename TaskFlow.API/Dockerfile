FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=app # predefined non-root user
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskFlow.API/TaskFlow.API.csproj", "TaskFlow.API/"]
COPY ["TaskFlow.Application/TaskFlow.Application.csproj", "TaskFlow.Application/"]
COPY ["TaskFlow.Domain/TaskFlow.Domain.csproj", "TaskFlow.Domain/"]
COPY ["TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "TaskFlow.Infrastructure/"]
RUN dotnet restore "TaskFlow.API/TaskFlow.API.csproj"
COPY . .
WORKDIR "/src/TaskFlow.API"
RUN dotnet build "./TaskFlow.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskFlow.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final production stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]

# Development stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
ARG APP_UID=app # predefined non-root user
USER $APP_UID
WORKDIR /app
COPY --from=build /src/TaskFlow.API .
ENV DOTNET_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]