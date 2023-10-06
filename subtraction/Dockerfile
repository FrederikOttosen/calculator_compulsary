FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["subtraction/subtraction.csproj", "subtraction/"]
RUN dotnet restore "subtraction/subtraction.csproj"
COPY . .
WORKDIR "/src/subtraction"
RUN dotnet build "subtraction.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "subtraction.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "subtraction.dll"]
