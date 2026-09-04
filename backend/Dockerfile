# Imagem oficial do .net
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos do sistema
COPY *.csproj ./
RUN dotnet restore

# Copia o restante do código e publica e gera os binários
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Imagem final que vai subir para produção
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .


ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SistemaGestaoBiblioteca.dll"]
