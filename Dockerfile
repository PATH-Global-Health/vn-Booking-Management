FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# copy everything and build the project
COPY /Booking_Service ./
RUN dotnet restore Booking_Service_App/*.csproj
RUN dotnet publish Booking_Service_App/*.csproj -c Release -o out

# Build runtime image
WORKDIR /app
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 
COPY --from=build-env /app/Booking_Service_App/bin/Release/netcoreapp3.1/ .
ENTRYPOINT ["dotnet", "Booking_Service_App.dll"]
