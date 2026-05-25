FROM ubuntu:latest

RUN apt update && apt update
-y

RUN apt-get update && apt-get
 install -y \
    dotnet-runtime-8.0 \
    libgtk-4-1 \
    libadwaita-1-0 \
    libwebkit2gtk-4.1-0 \
    libgl1 \
    libgl1-mesa-dri \
    at-spi2-core \
    dbus-x11 \
    adwaita-icon-theme \
    fonts-dejavu \
    libwebkitgtk-6.0-4 \
 && rm -rf /var/lib/apt/lists
/*

WORKDIR /home

RUN mkdir devtoys

WORKDIR /home/devtoys
