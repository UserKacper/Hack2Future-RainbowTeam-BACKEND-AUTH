# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the FraudDetection.csproj file and restore dependencies
COPY Hack2Future-RainbowTeam-BACKEND-AUTH/FraudDetection/FraudDetection.csproj ./FraudDetection/
WORKDIR /app/FraudDetection
RUN dotnet restore

# Copy the rest of the application files
COPY Hack2Future-RainbowTeam-BACKEND-AUTH/FraudDetection/. ./FraudDetection/

# Publish the application in release mode
RUN dotnet publish -c Release -o /app/out

# Use the official .NET runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out ./

# Expose the port that the app will run on
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "FraudDetection.dll"]
