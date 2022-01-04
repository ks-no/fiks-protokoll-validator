FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS core-build-env
WORKDIR /build-app

# Copy csproj and build + publish code
#COPY KS.FiksProtokollValidator.WebAPI ./KS.FiksProtokollValidator.WebAPI

RUN dotnet restore KS.FiksProtokollValidator.WebAPI/KS.FiksProtokollValidator.WebAPI.csproj && dotnet build --configuration Release KS.FiksProtokollValidator.WebAPI/KS.FiksProtokollValidator.WebAPI.csproj && dotnet publish --configuration Release KS.FiksProtokollValidator.WebAPI/KS.FiksProtokollValidator.WebAPI.csproj --no-build --output published-app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
ARG build_version_number=undefined
WORKDIR /app
COPY --from=core-build-env /build-app/published-app .

ENTRYPOINT ["dotnet", "KS.FiksProtokollValidator.WebAPI.dll"]