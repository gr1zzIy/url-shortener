FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо весь проект
COPY . .

# Відновлюємо залежності та будуємо
RUN dotnet restore "src/UrlShortener.Api/UrlShortener.Api.csproj"
RUN dotnet build "src/UrlShortener.Api/UrlShortener.Api.csproj" -c Release -o /app/build

RUN dotnet publish "src/UrlShortener.Api/UrlShortener.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]