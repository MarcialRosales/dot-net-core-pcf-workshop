#!/bin/bash
cd source-code

echo "Restoring dependencies ..."

dotnet restore

echo "Building source ..."

dotnet build
