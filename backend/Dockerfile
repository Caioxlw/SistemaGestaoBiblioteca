# --- Etapa 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia apenas o .csproj primeiro para aproveitar cache de layers
COPY *.csproj ./
RUN dotnet restore

# Copia o restante do código e publica
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Etapa 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configura o Kestrel para escutar na porta 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SistemaGestaoBiblioteca.dll"]
