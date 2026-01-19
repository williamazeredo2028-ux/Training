# This phase is used during execution in VS in fast mode (Default for Debug configuration).
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# This phase is used to compile the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Training/TrainingApi.csproj", "TrainingApi/"]
RUN dotnet restore "TrainingApi/TrainingApi.csproj"
COPY . .
WORKDIR "/src/TrainingApi"
RUN dotnet build "TrainingApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This phase is used to publish the service project to be copied to the final phase
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TrainingApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This phase is used in production or when run in VS in normal mode (default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrainingApi.dll"]