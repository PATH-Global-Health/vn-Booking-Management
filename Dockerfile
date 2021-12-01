FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Booking_Service/Booking_Service_App/Booking_Service_App.csproj", "Booking_Service_App/"]
COPY ["Booking_Service/Data/Data.csproj", "Data/"]
COPY ["Booking_Service/Services/Services.csproj", "Services/"]
RUN dotnet restore "Booking_Service_App/Booking_Service_App.csproj"
COPY . .
WORKDIR "/src/Booking_Service/Booking_Service_App"
RUN dotnet build "Booking_Service_App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Booking_Service_App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Booking_Service_App.dll"]
