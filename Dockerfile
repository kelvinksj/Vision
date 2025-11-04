FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 20000
#EXPOSE 20001

ENV ASPNETCORE_URLS=http://+:20000
#ENV ASPNETCORE_URLS=https://+:20001,http://+:20000

# USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["TaskManagerWeb.csproj", "./"]
RUN dotnet restore "TaskManagerWeb.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "TaskManagerWeb.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "TaskManagerWeb.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# COPY /wwwroot/json /app/wwwroot/json
ENTRYPOINT ["dotnet", "TaskManagerWeb.dll"]