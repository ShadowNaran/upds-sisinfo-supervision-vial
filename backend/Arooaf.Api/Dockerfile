# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Arooaf.Api.csproj", "./"]
RUN dotnet restore "Arooaf.Api.csproj"
COPY . .
RUN dotnet publish "Arooaf.Api.csproj" -c Release -o /app/publish

# Etapa 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto 8080 para Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Arooaf.Api.dll"]
