ARG VERSION="1.0.0.0"
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY *.sln .
COPY nuget.config .
COPY ./Telenor.Api.TestExecutionManagement.Core/*.csproj ./Telenor.Api.TestExecutionManagement.Core/
COPY ./Telenor.Api.TestExecutionManagement.Infrastructure/*.csproj ./Telenor.Api.TestExecutionManagement.Infrastructure/
COPY ./Telenor.Api.TestExecutionManagement/*.csproj ./Telenor.Api.TestExecutionManagement/
RUN dotnet restore

# copy and build the full solution
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
ARG VERSION
WORKDIR /src/Telenor.Api.TestExecutionManagement
RUN dotnet publish "Telenor.Api.TestExecutionManagement.csproj" -c Release -o /app/publish /p:Version=$VERSION

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Telenor.Api.TestExecutionManagement.dll", "hostBuilder:reloadConfigOnChange=False"]
