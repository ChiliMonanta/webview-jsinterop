set -e

mkdir -p app/libs
cd ../native-lib-gomobile/ 

if [ ! -f "go.mod" ]; then
    go mod init github.com/youruser/yourrep
fi

gomobile bind -o ../mobile-android-gomobile/app/libs/golib.aar .
cd ../mobile-android-gomobile

