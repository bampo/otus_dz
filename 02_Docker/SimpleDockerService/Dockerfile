FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

USER root
EXPOSE 8000

WORKDIR /app
COPY ./bin/publish .

ENTRYPOINT ["dotnet", "SimpleDockerService.dll"]