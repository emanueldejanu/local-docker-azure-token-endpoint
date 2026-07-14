#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

cleanup() {
	rm -f Directory.Packages.props
}
trap cleanup EXIT

cp ../Directory.Packages.props .

docker build -t sample-consumer-app .
