#!/bin/bash
cd source-code

cd $PROJECT

echo "Restoring dependencies ..."

dotnet restore

echo "Building $PROJECT ..."

dotnet build
