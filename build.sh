#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

dotnet restore

dotnet test ./test/Invio.Extensions.Linq.Tests/Invio.Extensions.Linq.Tests.csproj -c Release

dotnet pack -c Release -o ../../artifacts
