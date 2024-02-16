#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Soltech.Samples.Fora.EdgarApi/Soltech.Samples.Fora.EdgarApi.csproj", "Soltech.Samples.Fora.EdgarApi/"]
RUN dotnet restore "./Soltech.Samples.Fora.EdgarApi/./Soltech.Samples.Fora.EdgarApi.csproj"
COPY . .
WORKDIR "/src/Soltech.Samples.Fora.EdgarApi"
RUN dotnet build "./Soltech.Samples.Fora.EdgarApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Soltech.Samples.Fora.EdgarApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Soltech.Samples.Fora.EdgarApi.dll"]