# Use the .NET SDK image to build and run the app
FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app
COPY . .

# Restore dependencies
RUN dotnet restore

# Expose HTTP port
EXPOSE 5001

# Set environment so .NET uses only HTTP
ENV ASPNETCORE_URLS=http://0.0.0.0:5001

# Run the app using 'dotnet run'
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:5001"]
