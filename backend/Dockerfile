FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/ ./backend/
RUN dotnet restore backend/ConversationalCVBuilder.sln
RUN dotnet publish backend/src/ConversationalCVBuilder.Api/ConversationalCVBuilder.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
RUN mkdir -p /data && chown 1654:1654 /data

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
USER 1654

ENTRYPOINT ["dotnet", "ConversationalCVBuilder.Api.dll"]
