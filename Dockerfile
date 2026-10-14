FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Pogoda/Pogoda.csproj Pogoda/
RUN dotnet restore Pogoda/Pogoda.csproj
COPY Pogoda/ Pogoda/
RUN dotnet publish Pogoda/Pogoda.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Pogoda.dll"]
