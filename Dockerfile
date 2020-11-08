FROM golang:latest

RUN echo 'alias ll="ls -la"' >> ~/.bashrc

RUN apt-get update; \
    apt-get -y install wget unzip file
RUN mkdir /usr/lib/jvm

## JDK 8

RUN wget https://download.java.net/openjdk/jdk8u40/ri/openjdk-8u40-b25-linux-x64-10_feb_2015.tar.gz -O /tmp/openjdk-8u40-b25-linux-x64-10_feb_2015.tar.gz
RUN tar xfvz /tmp/openjdk-8u40-b25-linux-x64-10_feb_2015.tar.gz --directory /usr/lib/jvm
RUN rm -f /tmp/openjdk-8u40-b25-linux-x64-10_feb_2015.tar.gz

RUN sh -c 'for bin in /usr/lib/jvm/java-se-8u40-ri/bin/*; do update-alternatives --install /usr/bin/$(basename $bin) $(basename $bin) $bin 100; done'
RUN sh -c 'for bin in /usr/lib/jvm/java-se-8u40-ri/bin/*; do update-alternatives --set $(basename $bin) $bin; done'

## CERTS JAVA - WORKAROUND
# Unexpected error: java.security.InvalidAlgorithmParameterException: the trustAnchors parameter must be non-empty
RUN apt-get -y install ca-certificates-java
RUN ln -sf /etc/ssl/certs/java/cacerts /usr/lib/jvm/java-se-8u40-ri/jre/lib/security/cacerts

## JDK 12

RUN wget https://download.java.net/openjdk/jdk12/ri/openjdk-12+32_linux-x64_bin.tar.gz -O /tmp/openjdk-12+32_linux-x64_bin.tar.gz
RUN tar xfvz /tmp/openjdk-12+32_linux-x64_bin.tar.gz --directory /usr/lib/jvm
RUN rm -f /tmp/openjdk-12+32_linux-x64_bin.tar.gz

## Update java alternatives

RUN sh -c 'for bin in /usr/lib/jvm/jdk-12/bin/*; do update-alternatives --install /usr/bin/$(basename $bin) $(basename $bin) $bin 100; done'
RUN sh -c 'for bin in /usr/lib/jvm/jdk-12/bin/*; do update-alternatives --set $(basename $bin) $bin; done'

RUN update-alternatives --list java
RUN update-alternatives --set java /usr/lib/jvm/java-se-8u40-ri/bin/java

RUN update-alternatives --list javac
RUN update-alternatives --set javac /usr/lib/jvm/java-se-8u40-ri/bin/javac

RUN echo 'alias j8="update-alternatives --set java /usr/lib/jvm/java-se-8u40-ri/bin/java && update-alternatives --set javac /usr/lib/jvm/java-se-8u40-ri/bin/javac && export JAVA_HOME=/usr/lib/jvm/java-se-8u40-ri"' >> ~/.bashrc
RUN echo 'alias j12="update-alternatives --set java /usr/lib/jvm/jdk-12/bin/java && update-alternatives --set javac /usr/lib/jvm/jdk-12/bin/javac && export JAVA_HOME=/usr/lib/jvm/jdk-12"' >> ~/.bashrc

# ?? Warning: File /root/.android/repositories.cfg could not be loaded.

# ANDROID NDK

ENV SDK_URL="https://dl.google.com/android/repository/sdk-tools-linux-4333796.zip" \
    ANDROID_HOME="/usr/local/android-sdk" \
    ANDROID_VERSION=28 \
    ANDROID_BUILD_TOOLS_VERSION=28.0.1

## Download Android SDK
RUN mkdir "$ANDROID_HOME" .android \
    && cd "$ANDROID_HOME" \
    && curl -o sdk.zip $SDK_URL \
    && unzip sdk.zip \
    && rm sdk.zip \
    && yes | $ANDROID_HOME/tools/bin/sdkmanager --licenses

## Install Android Build Tool and Libraries
RUN $ANDROID_HOME/tools/bin/sdkmanager --update
RUN $ANDROID_HOME/tools/bin/sdkmanager "build-tools;${ANDROID_BUILD_TOOLS_VERSION}" \
    "platforms;android-${ANDROID_VERSION}" \
    "platform-tools"

# Install NDK
RUN $ANDROID_HOME/tools/bin/sdkmanager "ndk-bundle"

# Go mobile
RUN go get golang.org/x/mobile/cmd/gomobile
RUN gomobile init

# CGO, if CGO_ENABLED = 1 
# Cgo enables the creation of Go packages that call C code

# Cross compile windows
# Example: env GOOS=windows GOARCH=amd64 CGO_ENABLED=1 CC=x86_64-w64-mingw32-gcc CXX=x86_64-w64-mingw32-g++ go build -o awesome.dll -i -v -buildmode=c-shared ../native-lib/awesome.go
RUN apt-get -y install gcc-mingw-w64 gcc-multilib

# Cross compile OSX
# TODO, investigate tool chain
# http://crosstool-ng.github.io/
# https://github.com/tpoechtrager/osxcross
