#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM scratch AS test

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["web/OraculumApi/OraculumApi.csproj", "web/OraculumApi/"]
COPY ["src/Oraculum/Oraculum.csproj", "src/Oraculum/"]
RUN dotnet restore "web/OraculumApi/OraculumApi.csproj"
COPY . .
WORKDIR "/src/web/OraculumApi"
RUN dotnet build "OraculumApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OraculumApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS bundle
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OraculumApi.dll"]