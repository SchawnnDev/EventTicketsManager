FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EventTicketsManager/EventTicketsManager.csproj", "EventTicketsManager/"]
COPY ["Library/Library.csproj", "Library/"]
COPY ["Server/Server.csproj", "Server/"]
RUN dotnet restore "EventTicketsManager/EventTicketsManager.csproj"
COPY . .
WORKDIR "/src/EventTicketsManager"
RUN dotnet build "EventTicketsManager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventTicketsManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventTicketsManager.dll"]
