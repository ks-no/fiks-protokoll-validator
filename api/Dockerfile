# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.24
ARG build_version_number=undefined
WORKDIR /app

COPY  published-api .

ENTRYPOINT ["dotnet", "KS.FiksProtokollValidator.WebAPI.dll"]