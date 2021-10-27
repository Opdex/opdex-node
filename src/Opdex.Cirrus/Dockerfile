FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/Opdex.Cirrus/Opdex.Cirrus.csproj", "Opdex.Cirrus/"]
RUN dotnet restore "Opdex.Cirrus\Opdex.Cirrus.csproj"
COPY . .
WORKDIR "/src/src/Opdex.Cirrus"
RUN dotnet build "Opdex.Cirrus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Opdex.Cirrus.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Opdex.Cirrus.dll"]
