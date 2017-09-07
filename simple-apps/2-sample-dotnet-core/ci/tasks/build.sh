#!/bin/bash

PUBLISH_DIR=${PWD}/artifact

cd source-code
cd $PROJECT

echo "Restoring dependencies ..."

dotnet restore

echo "Building $PROJECT ..."

dotnet publish -o $PUBLISH_DIR -r ubuntu.14.04-x64
