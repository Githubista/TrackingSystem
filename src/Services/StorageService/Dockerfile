FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/StorageService/StorageService.csproj", "src/Services/StorageService/"]
COPY ["src/Common/RabbitMq/RabbitMq.csproj", "src/Common/RabbitMq/"]
RUN dotnet restore "src/Services/StorageService/StorageService.csproj"
COPY . .
WORKDIR "/src/src/Services/StorageService"
RUN dotnet build "StorageService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StorageService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StorageService.dll"]