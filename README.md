# Vinh Khanh Guide

## Build and run

Prerequisites:
- .NET 8 SDK
- .NET MAUI workload (`dotnet workload install maui`)

Commands:

```bash
dotnet restore VinhKhanhGuide.sln
dotnet build VinhKhanhGuide.sln
dotnet run --project src/backend/VinhKhanhGuide.Api/VinhKhanhGuide.Api.csproj
dotnet test tests/VinhKhanhGuide.Application.Tests/VinhKhanhGuide.Application.Tests.csproj
dotnet test tests/VinhKhanhGuide.Api.Tests/VinhKhanhGuide.Api.Tests.csproj
dotnet build src/mobile/VinhKhanhGuide.Mobile/VinhKhanhGuide.Mobile.csproj -t:Run -f net8.0-ios
```
## API build
```bash
dotnet build src/backend/VinhKhanhGuide.Api/VinhKhanhGuide.Api.csproj -p:NuGetAudit=false -v minimal
dotnet run --project src/backend/VinhKhanhGuide.Api/VinhKhanhGuide.Api.csproj```