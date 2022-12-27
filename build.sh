#!/usr/bin/env bash

cp README.md FiatAddon/DOCS.md
cp README.md FiatAddon/.

VERSION=$(cat FiatAddon/config.yaml| grep version | grep -P -o "[\d\.]*")

echo git tag -a $VERSION -m $VERSION
