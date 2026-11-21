# ---- Etapa 1: compilar ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NexoraApi.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# ---- Etapa 2: imagen final (solo lo necesario para ejecutar) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENV RUTA_BD=/data/nexora-servidor.db
RUN mkdir -p /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "NexoraApi.dll"]
