FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY BusGoSmartTicket.csproj ./
RUN dotnet restore BusGoSmartTicket.csproj
COPY . .
RUN dotnet publish BusGoSmartTicket.csproj -c Release --no-restore -o /app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /app/App_Data && chown -R $APP_UID:$APP_UID /app/App_Data
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BusGoSmartTicket.dll"]
