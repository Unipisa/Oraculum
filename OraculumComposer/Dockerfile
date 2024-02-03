#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/powershell AS base
WORKDIR /home/Oraculum

RUN pwsh -Command "Install-Module -Force Oraculum"

CMD ["pwsh", "-NoExit", "-Command", "Import-Module Oraculum"]