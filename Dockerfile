FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
RUN apt-get update -yq && apt-get upgrade -yq 
RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash - \
    && apt-get install -y nodejs
COPY ["Creators.csproj", "./"]
RUN dotnet restore "Creators.csproj"
COPY . .
RUN npm install
RUN dotnet build "Creators.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Creators.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#TODO: Remove next line from prod. Separate dev and rel dockerfiles
COPY --from=build /src/ /src/
ENTRYPOINT ["dotnet", "Creators.dll"]
