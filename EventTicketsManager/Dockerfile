FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update -y
RUN apt-get install wget libgdiplus -y
RUN wget -P /app https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.dll
RUN wget -P /app https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.dylib
RUN wget -P /app https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.so
USER $APP_UID
WORKDIR /app
EXPOSE 8080

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
