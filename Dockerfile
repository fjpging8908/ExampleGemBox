FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build

ARG CODEARTIFACT_REGISTRY
ARG CODEARTIFACT_AUTH_TOKEN

RUN dotnet nuget add source "${CODEARTIFACT_REGISTRY}v3/index.json" \
    --name "localpayment/aws-nuget-repo" \
    --username "aws" --password "$CODEARTIFACT_AUTH_TOKEN" \
    --store-password-in-clear-text

WORKDIR /src
COPY PayOutCore.API/PayOutCore.API.csproj PayOutCore.API/
RUN dotnet restore "PayOutCore.API/PayOutCore.API.csproj"
COPY . .
RUN dotnet publish "PayOutCore.API/PayOutCore.API.csproj" -c Release -o /app/publish

RUN dotnet nuget remove source "localpayment/aws-nuget-repo"

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "PayOutCore.API.dll"]
