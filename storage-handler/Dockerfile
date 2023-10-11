FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["storage-handler/storage-handler.csproj", "storage-handler/"]
RUN dotnet restore "storage-handler/storage-handler.csproj"
COPY . .
WORKDIR "/src/storage-handler"
RUN dotnet build "storage-handler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "storage-handler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "storage-handler.dll"]
