FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MemoLingo.Domain/MemoLingo.Domain.csproj MemoLingo.Domain/
COPY MemoLingo.Application/MemoLingo.Application.csproj MemoLingo.Application/
COPY MemoLingo.Infrastructure/MemoLingo.Infrastructure.csproj MemoLingo.Infrastructure/
COPY MemoLingo.Api/MemoLingo.Api.csproj MemoLingo.Api/
RUN dotnet restore MemoLingo.Api/MemoLingo.Api.csproj

COPY MemoLingo.Domain/ MemoLingo.Domain/
COPY MemoLingo.Application/ MemoLingo.Application/
COPY MemoLingo.Infrastructure/ MemoLingo.Infrastructure/
COPY MemoLingo.Api/ MemoLingo.Api/
COPY Documents/Seed/ Documents/Seed/

RUN dotnet publish MemoLingo.Api/MemoLingo.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MemoLingo.Api.dll"]
