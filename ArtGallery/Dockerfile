

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ArtGallery/ArtGallery.csproj", "ArtGallery/"]
RUN dotnet restore "./ArtGallery/./ArtGallery.csproj"
COPY . .
WORKDIR "/src/ArtGallery"
RUN dotnet build "./ArtGallery.csproj" -c %BUILD_CONFIGURATION% -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ArtGallery.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArtGallery.dll"]