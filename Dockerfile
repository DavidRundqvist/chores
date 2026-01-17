FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["chores.csproj", "."]
RUN dotnet restore "chores.csproj"

COPY . .
RUN dotnet build "chores.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "chores.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

# Create Data directory for chores and records storage
RUN mkdir -p /app/Data

EXPOSE 8080

ENTRYPOINT ["dotnet", "chores.dll"]
