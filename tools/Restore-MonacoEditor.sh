#!/usr/bin/env bash

bash --version 2>&1 | head -n 1

set -eo pipefail
SCRIPT_DIR="$1"

if ! [[ -f "$SCRIPT_DIR//.gitignore" ]]; then
    echo "Please run this script from the repository's root folder"
    exit -1
fi

# ------------------------
TEMP_DIR_NAME=".temp"
TOOLS_DIR="$1/tools"
DESTINATION_DIR="../src/app/dev/DevToys.Blazor/wwwroot/lib/monaco-editor"

cd "$TOOLS_DIR"

# Reference to Monaco Version to Use in the Package
MONACO_VERSION=$(head -n 1 "$TOOLS_DIR/monaco-editor-version-number.txt")
MONACO_TGZ_URL="https://registry.npmjs.org/monaco-editor/-/monaco-editor-$MONACO_VERSION.tgz"

# Remove Old Dependency
rm -rf $DESTINATION_DIR

# Clean-up Temp Dir, if already exist
rm -rf "$TOOLS_DIR/$TEMP_DIR_NAME"

# Create Temp Directory and Output
mkdir -p "$TOOLS_DIR/$TEMP_DIR_NAME"
mkdir -p $DESTINATION_DIR

echo "Downloading Monaco v.$MONACO_VERSION"

curl -Lsfo "$TOOLS_DIR/$TEMP_DIR_NAME/monaco.tgz" $MONACO_TGZ_URL

echo "Extracting..."

mkdir "$TOOLS_DIR/$TEMP_DIR_NAME/monaco"
tar -zxf "$TOOLS_DIR/$TEMP_DIR_NAME/monaco.tgz" -C "$TOOLS_DIR/$TEMP_DIR_NAME/monaco"

cp -r "$TOOLS_DIR/$TEMP_DIR_NAME/monaco/package/min" "$DESTINATION_DIR/min"

# Clean-up Temp Dir
rm -rf "$TOOLS_DIR/$TEMP_DIR_NAME"

cd "$1"