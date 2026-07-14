$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

try {
	Copy-Item ..\Directory.Packages.props .
	docker build -t sample-consumer-app:latest .
}
finally {
	Remove-Item -Force -ErrorAction SilentlyContinue Directory.Packages.props
	Pop-Location
}
