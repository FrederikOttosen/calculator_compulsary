FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["multiplication/multiplication.csproj", "multiplication/"]
RUN dotnet restore "multiplication/multiplication.csproj"
COPY . .
WORKDIR "/src/multiplication"
RUN dotnet build "multiplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "multiplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "multiplication.dll"]
