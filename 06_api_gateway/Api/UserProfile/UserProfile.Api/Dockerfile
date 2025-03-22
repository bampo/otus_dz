
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base

WORKDIR /app
EXPOSE 8080

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN apk add --no-cache icu-libs

USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CrudApi/CrudApi.csproj", "CrudApi/"]
COPY ["EfDal/EfDal.csproj", "EfDal/"]
RUN dotnet restore "./CrudApi/CrudApi.csproj"
COPY . .
WORKDIR "/src/CrudApi"
RUN dotnet build "./CrudApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CrudApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrudApi.dll"]