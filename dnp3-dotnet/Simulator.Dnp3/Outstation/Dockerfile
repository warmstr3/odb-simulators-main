FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Outstation/Outstation.csproj", "Outstation/"]
RUN dotnet restore "Outstation/Outstation.csproj"
COPY . .
WORKDIR "/src/Outstation"
RUN dotnet build "./Outstation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Outstation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 20000
ENTRYPOINT ["dotnet", "Simulator.Dnp3.Outstation.dll"]