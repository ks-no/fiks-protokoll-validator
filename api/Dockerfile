# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.21
ARG build_version_number=undefined
WORKDIR /app

COPY  published-api .

ENTRYPOINT ["dotnet", "KS.FiksProtokollValidator.WebAPI.dll"]