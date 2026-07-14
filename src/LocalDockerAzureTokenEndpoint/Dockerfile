FROM mcr.microsoft.com/azure-cli:2.88.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       clang zlib1g-dev

WORKDIR /src
COPY . .
RUN dotnet publish \
	-c release \
	-o result \
	--runtime linux-x64 \
	--self-contained true \
	/p:PublishTrimmed=true \
	/p:TrimMode=Link \
	/p:PublishSingleFile=true \
	LocalDockerAzureTokenEndpoint.csproj

FROM base AS final
EXPOSE 80
WORKDIR /app
COPY --from=build /src/result .
CMD ["./LocalDockerAzureTokenEndpoint"]
