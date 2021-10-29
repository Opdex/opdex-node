FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Opdex.Cirrus.csproj", "Opdex.Cirrus/"]
WORKDIR /src/Opdex.Cirrus
RUN dotnet restore "Opdex.Cirrus.csproj"
COPY . .
RUN dotnet build "Opdex.Cirrus.csproj" -c Release --no-restore -o /app/build

FROM build AS publish
RUN dotnet publish "Opdex.Cirrus.csproj" -c Release --no-build -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Opdex.Cirrus.dll"]
