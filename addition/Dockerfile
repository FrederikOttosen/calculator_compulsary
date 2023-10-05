FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["addition/addition.csproj", "addition/"]
RUN dotnet restore "addition/addition.csproj"
COPY . .
WORKDIR "/src/addition"
RUN dotnet build "addition.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "addition.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "addition.dll"]
