FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Kjeholmen/Kjeholmen.csproj", "Kjeholmen/"]
RUN dotnet restore "Kjeholmen/Kjeholmen.csproj"
COPY . .
WORKDIR "/src/Kjeholmen"
RUN dotnet build "Kjeholmen.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kjeholmen.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kjeholmen.dll"]
