param(
	[Parameter(Mandatory)]
	[string]$AcrName,

	[Parameter(Mandatory)]
	[string]$Version
)

$ErrorActionPreference = 'Stop'

$imageName = "local-docker-azure-token-endpoint"
$remoteTag = "$AcrName/${imageName}:$Version"
$remoteLatest = "$AcrName/${imageName}:latest"

az acr login --name $AcrName

docker tag "${imageName}:latest" $remoteTag
docker tag "${imageName}:latest" $remoteLatest

docker push $remoteTag
docker push $remoteLatest
