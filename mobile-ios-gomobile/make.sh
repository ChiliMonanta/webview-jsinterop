set -e
rm -rf WebView/Frameworks/Golib.framework
cd ../native-lib-gomobile/

if [ ! -f "go.mod" ]; then
    go mod init github.com/youruser/yourrep
fi

gomobile init

gomobile bind -target=ios/arm64,ios/amd64 -iosversion=13.0 -o ../mobile-ios-gomobile/WebView/Frameworks/Golib.framework .
cd ../mobile-ios-gomobile
