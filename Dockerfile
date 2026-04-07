FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["POCFlexora.csproj", "."]
RUN dotnet restore "./POCFlexora.csproj"
COPY . .
RUN dotnet build "./POCFlexora.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./POCFlexora.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Force HTTP and set environment for Docker
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_HTTPS_PORT=
ENV DOTNET_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "POCFlexora.dll"]