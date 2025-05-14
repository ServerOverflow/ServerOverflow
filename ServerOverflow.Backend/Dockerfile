FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "ServerOverflow.Backend/ServerOverflow.Backend.csproj" -c Release -o /app/ /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app /app
EXPOSE 5000
CMD ["dotnet", "ServerOverflow.Backend.dll", "--urls", "http://0.0.0.0:5000"]
