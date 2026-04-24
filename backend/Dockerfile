FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/backend.csproj backend/
RUN dotnet restore backend/backend.csproj

COPY backend/. backend/
WORKDIR /src/backend
RUN dotnet publish backend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/images

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "backend.dll"]
