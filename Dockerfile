FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/OptimusFrame.Core.API/OptimusFrame.Core.API.csproj", "src/OptimusFrame.Core.API/"]
COPY ["src/OptimusFrame.Core.Application/OptimusFrame.Core.Application.csproj", "src/OptimusFrame.Core.Application/"]
COPY ["src/OptimusFrame.Core.Domain/OptimusFrame.Core.Domain.csproj", "src/OptimusFrame.Core.Domain/"]
COPY ["src/OptimusFrame.Core.Infrastructure/OptimusFrame.Core.Infrastructure.csproj", "src/OptimusFrame.Core.Infrastructure/"]
RUN dotnet restore "src/OptimusFrame.Core.API/OptimusFrame.Core.API.csproj"
COPY . .
WORKDIR "/src/src/OptimusFrame.Core.API"
RUN dotnet build "OptimusFrame.Core.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OptimusFrame.Core.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OptimusFrame.Core.API.dll"]
