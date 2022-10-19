#!/usr/bin/env bash

cp README.md FiatChampAddon/DOCS.md
cp README.md FiatChampAddon/.

VERSION=$(cat FiatChampAddon/config.yaml| grep version | grep -P -o "[\d\.]*")

echo git tag -a $VERSION -m $VERSION
