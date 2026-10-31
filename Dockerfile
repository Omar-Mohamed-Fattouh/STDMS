# STDMS (ASP.NET Core 8 MVC) - production image
# Deploy this to any container host (Railway, Render, Azure Container Apps, Fly.io).
# NOTE: Vercel cannot run .NET applications; it only serves the Next.js showcase in /app.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY STDMS.csproj ./
RUN dotnet restore STDMS.csproj
COPY . ./
RUN dotnet publish STDMS.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# Provide a real SQL Server at runtime, e.g.:
#   ConnectionStrings__DefaultConnection="Server=...;Database=STDMSDB;User Id=...;Password=...;TrustServerCertificate=true"
ENTRYPOINT ["dotnet", "STDMS.dll"]
