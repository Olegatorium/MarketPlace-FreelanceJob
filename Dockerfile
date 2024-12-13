FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release 
WORKDIR /src

COPY ["BestShop.csproj", "./"]
RUN dotnet restore "BestShop.csproj"

COPY . . 
RUN dotnet build "BestShop.csproj" -c $BUILD_CONFIGURATION -o /app/build 

FROM build AS publish
ARG BUILD_CONFIGURATION=Release  
RUN dotnet publish "BestShop.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 

ENTRYPOINT ["dotnet", "BestShop.dll"]
