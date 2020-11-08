#!/bin/bash
set -e

# Linux
env GOOS=linux GOARCH=amd64 CGO_ENABLED=1 go build -o golib.so -buildmode=c-shared ../native-lib/golib.go

# Windows
env GOOS=windows GOARCH=amd64 CGO_ENABLED=1 CC=x86_64-w64-mingw32-gcc CXX=x86_64-w64-mingw32-g++ go build -o golib.dll -i -v -buildmode=c-shared ../native-lib/golib.go