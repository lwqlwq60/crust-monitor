############################################ build CrustMonitorNode ############################################

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-crust-monitor
WORKDIR /app

COPY ./backend/CrustMonitor.sln ./

# copy csproj and restore as distinct layers

COPY ./backend/CrustMonitor/*.csproj ./CrustMonitor/

RUN dotnet restore

# copy and publish app and libraries

COPY ./backend .

RUN dotnet publish -c Release ./CrustMonitor/CrustMonitor.csproj -o /publish

############################################ runtime CrustMonitorNode ############################################

# runtime for crust-monitor-node
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime-crust-monitor
WORKDIR /app
COPY --from=build-crust-monitor /publish ./
ENTRYPOINT ["dotnet", "CrustMonitor.dll"]