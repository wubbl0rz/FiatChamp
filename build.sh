#!/usr/bin/env bash

cd FiatChampAddon/FiatClient/

~/.dotnet/dotnet publish --configuration Release -r linux-musl-x64 --self-contained=true -o build/amd64
~/.dotnet/dotnet publish --configuration Release -r linux-musl-arm --self-contained=true -o build/armv7

cd ../../

docker run --rm --privileged multiarch/qemu-user-static --reset -p yes
docker run --rm --privileged -v ~/.docker:/root/.docker -v $(pwd)/FiatChampAddon:/data homeassistant/amd64-builder --all -t /data
