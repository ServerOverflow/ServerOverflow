FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "ServerOverflow.Snooper/ServerOverflow.Snooper.csproj" -c Release -o /app/ /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /app /app
EXPOSE 8008
CMD ["dotnet", "ServerOverflow.Snooper.dll"]
