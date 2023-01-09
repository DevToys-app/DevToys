#!/usr/bin/env bash

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

# Install .NET
. "./tools/Install-DotNet.sh" $SCRIPT_DIR

# Build the build project.
echo "Building the pipeline"
BUILD_PROJECT_FILE="$SCRIPT_DIR/src/build/_build.csproj"

"$DOTNET_EXE" build "$BUILD_PROJECT_FILE" /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet
echo "Done."
echo "--------------------------------------"

# Run the building
"$DOTNET_EXE" run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"
echo "Done."
echo "--------------------------------------"
