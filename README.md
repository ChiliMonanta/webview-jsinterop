# WebView with native access

Have you ever thought about access the platform from a WebView.
Then this project might give you some inspiration.

In the examples below the web pages are serverd with a dotnet blazor server.
The webpage makes platform requests to a go lib.
A call chain might look like this:
    blazor page -> javascript -> platform code (dotnet core/Java/Swift) -> go lang (library)

This example covers a couple of different platforms (Clients):
    - Console (not an webview)
    - Desktop app
    - Android app, Mobile phone
    - Ios app, Mobile phone
    - Browser (limited, you cant make platform calls)

Sub projects:

    - console
        A console application
        Build native: In docker shell run make.sh or make-darwin.sh from the Mac shell
        Execute: dotnet run

    - desktop-webwindow
        A desktop application, show a web view (from web-blazorserver)
        Build native: In docker shell run make.sh or make-darwin.sh from the Mac shell
        Start server: See web-blazorserver
        Start App: dotnet run

    - TODO desktop-electron

    - TODO mobile-android
        Use cgo instead of gomobile

    - TODO mobile-ios
        Use cgo instead of gomobile

    - web-blazorserver
        This is the backend for the mobile clients and the Webwindow
        Start server: dotnet run

    - native-lib
        This is the native lib that's included in every application.
        Build: Make script exists in each app that use this lib.
        You can use the docker container to build this for all platforms except MacOS.
        There is another make script for MacOS that you execute in a Mac console.
    
    - mobile-android-gomobile
        This is an Android project, the go code is built with gomobile.
        [Gomobile](https://godoc.org/golang.org/x/mobile/cmd/gomobile) is a tool for building and running mobile apps written in Go.
        Gomobile does only support basic datatypes. An alternative is to use cgo, see project mobile-android.

        This project doesn't support all examples.

        Build native: In docker shell run make.sh
        Start server: See web-blazorserver
        Build and install the android project

    - mobile-ios-gomobile
        This is an ios project, the go code is built with gomobile.
        [Gomobile](https://godoc.org/golang.org/x/mobile/cmd/gomobile) is a tool for building and running mobile apps written in Go.
        Gomobile does only support basic datatypes. An alternative is to use cgo, see project mobile-ios.

        This project doesn't support all examples.

        Build native: In ios shell run make.sh
        Start server: See web-blazorserver
        Build and install the ios project

    - native-lib-gomobile
        Almost the same as the native-lib except that this project is built with gomobile.
        This project is doesn't support all examples.

    - Dockerfile, to build the native-lib for Linux, Windows and Android. It's also possible to build the app mobile-android
        Build image:
            docker build -t builder .
        Run:
            docker run -it --name builder -v ~/<this folder>:/workspace builder bash
            (restart stopped, docker start -ai builder)
            > cd /workspace/<the sub project>

Limitations:
    - Invalid memory access in go, crashes the WebWindow in Windows

Prerequisites:
    - [Dotnet core SDK](https://dotnet.microsoft.com/download)

    - Docker, to build the native lib and Android

    - [Go](https://golang.org/dl/), If you running on MacOS (to be able to build the native lib)
       Install go-mobile, if you are going to build mobile-ios-gomobile
       "go get golang.org/x/mobile/cmd/gomobile"

    - desktop-webwindow
        This project is based on WebWindow, and have some dependencies.

        Window 10:
            - Install the [Microsoft Edge](https://www.microsoftedgeinsider.com/en-us/)
              WebWindow uses the Chromium-based Edge via webview2
            - (Microsoft Visual C++ 2015-2019 Redistributable)[https://aka.ms/vs/16/release/vc_redist.x64.exe]

TODO:
    - Cross compile osx and ios

Resources:
- https://github.com/SteveSandersonMS/WebWindow
- https://blog.stevensanderson.com/2019/11/18/2019-11-18-webwindow-a-cross-platform-webview-for-dotnet-core/
- https://docs.microsoft.com/en-gb/aspnet/core/blazor/get-started?view=aspnetcore-3.1&tabs=visual-studio
- https://golang.org/cmd/cgo/
- https://github.com/vladimirvivien/go-cshared-examples




