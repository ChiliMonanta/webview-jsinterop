set -e

GOOS=darwin CGO_ENABLED=1 go build -o golib.dylib -buildmode=c-shared -i -v ../native-lib/golib.go