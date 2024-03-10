FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ef-core-query-internal-server-error/ef-core-query-internal-server-error.csproj", "ef-core-query-internal-server-error/"]
RUN dotnet restore "ef-core-query-internal-server-error/ef-core-query-internal-server-error.csproj"
COPY . .
WORKDIR "/src/ef-core-query-internal-server-error"
RUN dotnet build "ef-core-query-internal-server-error.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ef-core-query-internal-server-error.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ef-core-query-internal-server-error.dll"]
