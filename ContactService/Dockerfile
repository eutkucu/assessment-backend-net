FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ContactService.csproj", "./"]
RUN dotnet restore "ContactService.csproj"
COPY . .
RUN dotnet build "ContactService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContactService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update && apt-get install -y curl lsof
EXPOSE 5002
ENV ASPNETCORE_URLS=http://0.0.0.0:5002
ENTRYPOINT ["dotnet", "ContactService.dll"] 