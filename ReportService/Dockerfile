FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ReportService.csproj", "./"]
RUN dotnet restore "ReportService.csproj"
COPY . .
RUN dotnet publish "ReportService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENV ASPNETCORE_URLS=http://0.0.0.0:5001
ENTRYPOINT ["dotnet", "ReportService.dll"] 