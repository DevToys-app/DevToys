FROM ubuntu:latest
RUN apt update && apt update -y
RUN apt install -y fish git curl vim software-properties-common
WORKDIR /home
RUN mkdir devtoys
WORKDIR /home/devtoys
COPY devtoys.cli_linux_x64.deb /home/devtoys
RUN dpkg -i devtoys.cli_linux_x64.deb
