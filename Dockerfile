FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AIJobHelper.sln .
COPY src/AIJobHelper.Domain/AIJobHelper.Domain.csproj src/AIJobHelper.Domain/
COPY src/AIJobHelper.Application/AIJobHelper.Application.csproj src/AIJobHelper.Application/
COPY src/AIJobHelper.Infrastructure/AIJobHelper.Infrastructure.csproj src/AIJobHelper.Infrastructure/
COPY src/AIJobHelper.API/AIJobHelper.API.csproj src/AIJobHelper.API/
RUN dotnet restore

COPY . .
RUN dotnet publish src/AIJobHelper.API/AIJobHelper.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
RUN mkdir -p /app/logs /app/uploads && chown -R appuser:appgroup /app

COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AIJobHelper.API.dll"]
