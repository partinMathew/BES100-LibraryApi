#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["LibraryApi2/LibraryApi2.csproj", "LibraryApi2/"]
RUN dotnet restore "LibraryApi2/LibraryApi2.csproj"
COPY . .
WORKDIR "/src/LibraryApi2"
RUN dotnet build "LibraryApi2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LibraryApi2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LibraryApi2.dll"]