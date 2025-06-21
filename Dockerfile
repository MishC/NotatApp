# 1. Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# 2. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose HTTP port 5001
EXPOSE 5001

# Set environment so .NET uses only HTTP
ENV ASPNETCORE_URLS=http://0.0.0.0:5001

# Run the app
ENTRYPOINT ["dotnet", "NoteApp.dll"]
