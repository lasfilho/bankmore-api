# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore BankMore.Transferencia.API/BankMore.Transferencia.API.csproj
RUN dotnet publish BankMore.Transferencia.API/BankMore.Transferencia.API.csproj -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY Database ./Database

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BankMore.Transferencia.API.dll"]
