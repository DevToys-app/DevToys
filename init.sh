#!/usr/bin/env bash

bash --version 2>&1 | head -n 1

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)
SCRIPT_DIR="${SCRIPT_DIR}/"

# Install .NET
. "./tools/Install-DotNet.sh" "$SCRIPT_DIR"

# Install .NET workloads
echo "Installing .NET workloads"
sudo dotnet workload install maccatalyst

# Restore NuGet solution dependencies
echo "Restoring all dependencies"
SOLUTIONS=$(find ./src/ -iname "*.sln" -print)
for SOLUTION_FILE in $SOLUTIONS
do
    echo "Restoring packages for $SOLUTION_FILE..."
    "$DOTNET_EXE" restore -v:quiet $SOLUTION_FILE
done

echo "Done."
echo "---------------------------------------"

# Restore Monaco Editor
echo "Restoring Monaco Editor"
. "./tools/Restore-MonacoEditor.sh" "$SCRIPT_DIR"

echo "Done."
echo "---------------------------------------"
